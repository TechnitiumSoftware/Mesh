/*
Technitium Mesh
Copyright (C) 2018  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.IO;
using System.Security.Cryptography;
using TechnitiumLibrary.IO;

namespace MeshCore.Message
{
    public class MessageStore : IDisposable
    {
        #region variables

        readonly Stream _index;
        readonly Stream _data;
        readonly byte[] _key;

        readonly SymmetricAlgorithm _crypto;

        readonly object _lock = new object();

        #endregion

        #region constructor

        public MessageStore(Stream index, Stream data, byte[] key)
        {
            _index = index;
            _data = data;
            _key = key;

            _crypto = Aes.Create();
            _crypto.Key = key;
            _crypto.GenerateIV();
            _crypto.Mode = CipherMode.CBC;
            _crypto.Padding = PaddingMode.ISO10126;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    _index?.Dispose();
                    _data?.Dispose();
                    _crypto?.Dispose();
                }

                for (int i = 0; i < _key.Length; i++)
                    _key[i] = 255;

                _disposed = true;
            }
        }

        #endregion

        #region private

        private void Encrypt(Stream clearText, Stream cipherText)
        {
            using (CryptoStream cS = new CryptoStream(cipherText, _crypto.CreateEncryptor(), CryptoStreamMode.Write))
            {
                clearText.CopyTo(cS, 256);
            }
        }

        private void Decrypt(Stream cipherText, Stream clearText)
        {
            using (CryptoStream cS = new CryptoStream(cipherText, _crypto.CreateDecryptor(), CryptoStreamMode.Read))
            {
                cS.CopyTo(clearText, 256);
            }
        }

        #endregion

        #region public

        public int WriteMessage(Stream input)
        {
            lock (_lock)
            {
                if (_disposed)
                    return -1;

                //encrypt message data
                byte[] encryptedData;
                using (MemoryStream mS = new MemoryStream())
                {
                    _crypto.GenerateIV(); //generate new IV for each message encryption since key is same
                    Encrypt(input, mS);

                    encryptedData = mS.ToArray();
                }

                //authenticated encryption in Encrypt-then-MAC (EtM) mode
                byte[] aeHmac;
                using (HMAC hmac = new HMACSHA256(_key))
                {
                    aeHmac = hmac.ComputeHash(encryptedData);
                }

                //write encrypted message data

                //seek to end of stream
                _index.Position = _index.Length;
                _data.Position = _data.Length;

                //get message offset
                uint messageOffset = Convert.ToUInt32(_data.Position);

                //write data
                BinaryWriter bW = new BinaryWriter(_data);

                bW.Write((byte)1); //version
                bW.WriteBuffer(_crypto.IV);
                bW.WriteBuffer(encryptedData);
                bW.WriteBuffer(aeHmac);

                //write message offset to index stream
                _index.Write(BitConverter.GetBytes(messageOffset), 0, 4);

                //return message number
                return Convert.ToInt32(_index.Position / 4) - 1;
            }
        }

        public void UpdateMessage(int number, Stream input)
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                //seek to index location
                int indexPosition = number * 4;

                if (indexPosition >= _index.Length)
                    throw new IOException("Cannot read message from message store: message number out of range.");

                _index.Position = indexPosition;

                //read message offset
                byte[] buffer = new byte[4];
                _index.ReadBytes(buffer, 0, 4);
                uint messageOffset = BitConverter.ToUInt32(buffer, 0);

                //seek to message offset
                _data.Position = messageOffset;

                //read data
                BinaryReader bR = new BinaryReader(_data);
                byte[] existingEncryptedData;

                switch (bR.ReadByte()) //version
                {
                    case 1:
                        bR.ReadBuffer(); //IV
                        existingEncryptedData = bR.ReadBuffer();
                        break;

                    default:
                        throw new IOException("Cannot read message from message store: message version not supported.");
                }

                //encrypt message data
                byte[] newEncryptedData;
                using (MemoryStream mS = new MemoryStream())
                {
                    _crypto.GenerateIV(); //generate new IV for each message encryption since key is same
                    Encrypt(input, mS);

                    newEncryptedData = mS.ToArray();
                }

                //authenticated encryption in Encrypt-then-MAC (EtM) mode
                byte[] aeHmac;
                using (HMAC hmac = new HMACSHA256(_key))
                {
                    aeHmac = hmac.ComputeHash(newEncryptedData);
                }

                bool lengthIsInLimit = (newEncryptedData.Length <= existingEncryptedData.Length);

                if (lengthIsInLimit)
                {
                    //seek to message offset
                    _data.Position = messageOffset;

                    //overwrite new data
                    BinaryWriter bW = new BinaryWriter(_data);

                    bW.Write((byte)1); //version
                    bW.WriteBuffer(_crypto.IV);
                    bW.WriteBuffer(newEncryptedData);
                    bW.WriteBuffer(aeHmac);
                }
                else
                {
                    //seek to index location
                    _index.Position = number * 4;

                    //seek to end of data stream
                    _data.Position = _data.Length;

                    //get message offset
                    messageOffset = Convert.ToUInt32(_data.Position);

                    //write new data
                    BinaryWriter bW = new BinaryWriter(_data);

                    bW.Write((byte)1); //version
                    bW.WriteBuffer(_crypto.IV);
                    bW.WriteBuffer(newEncryptedData);
                    bW.WriteBuffer(aeHmac);

                    //overwrite message offset to index stream
                    _index.Write(BitConverter.GetBytes(messageOffset), 0, 4);
                }
            }
        }

        public void ReadMessage(int number, Stream output)
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                //seek to index location
                int indexPosition = number * 4;

                if (indexPosition >= _index.Length)
                    throw new IOException("Cannot read message from message store: message number out of range.");

                _index.Position = indexPosition;

                //read message offset
                byte[] buffer = new byte[4];
                _index.ReadBytes(buffer, 0, 4);
                uint messageOffset = BitConverter.ToUInt32(buffer, 0);

                //seek to message offset
                _data.Position = messageOffset;

                //read data
                BinaryReader bR = new BinaryReader(_data);
                byte[] IV;
                byte[] encryptedData;

                switch (bR.ReadByte()) //version
                {
                    case 1:
                        IV = bR.ReadBuffer();
                        encryptedData = bR.ReadBuffer();
                        byte[] aeHmac = bR.ReadBuffer();

                        //verification for authenticated encryption in Encrypt-then-MAC (EtM) mode
                        BinaryNumber computedAeHmac;

                        using (HMAC hmac = new HMACSHA256(_key))
                        {
                            computedAeHmac = new BinaryNumber(hmac.ComputeHash(encryptedData));
                        }

                        if (!computedAeHmac.Equals(new BinaryNumber(aeHmac)))
                            throw new CryptographicException("Cannot read message from message store: message is corrupt or tampered.");

                        break;

                    default:
                        throw new InvalidDataException("Cannot read message from message store: message version not supported.");
                }

                using (MemoryStream src = new MemoryStream(encryptedData, 0, encryptedData.Length))
                {
                    _crypto.IV = IV;
                    Decrypt(src, output);
                }
            }
        }

        public int GetMessageCount()
        {
            lock (_lock)
            {
                if (_disposed)
                    return -1;

                return Convert.ToInt32(_index.Length / 4);
            }
        }

        #endregion
    }
}
