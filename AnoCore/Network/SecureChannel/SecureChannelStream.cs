/*
Technitium Bit Chat
Copyright (C) 2015  Shreyas Zare (shreyas@technitium.com)

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
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Security.Cryptography;

/*
=============
= VERSION 3 =
=============

FEATURES-
---------
 - random challenge on both ends to prevent replay attacks.
 - ephemeral keys used for key exchange to provide perfect forward secrecy.
 - pre-shared key based auth to prevent direct certificate disclosure, preventing identity disclosure to active attacker.
 - encrypted digital certificate exchange to prevent certificate disclosure while exchange, preventing identity disclosure to passive attacker.
 - digital certificate based authentication for ensuring identity and prevent MiTM.
 - secure channel data packet authenticated by HMACSHA256(cipher-text) to provide authenticated encryption.
 - key re-negotation feature for allowing the secure channel to remain always on.
 
<==============================================================================>
                              SERVER        CLIENT
<==============================================================================>
                             version  --->
                                      <---  version supported
<------------------------------------------------------------------------------> version selection done
                                            client nonce +
                                      <---  crypto options  
                      server nonce +  
              selected crypto option  ---> 
<------------------------------------------------------------------------------> hello handshake done
              ephemeral public key +
                           signature  --->
                                      <---  ephemeral public key +
                                            signature
<------------------------------------------------------------------------------> key exchange done
  master key = HMACSHA256(client hello + server hello, derived key)
      OR
  master key = HMACSHA256(HMACSHA256(client hello + server hello, psk), derived key)
<------------------------------------------------------------------------------> master key generated on both sides with optional pre-shared key
                                      <---  HMACSHA256(server hello, master key)
HMACSHA256(client hello, master key)  --->
<------------------------------------------------------------------------------> verify master key using HMAC authentication; encryption layer ON
                                      <---  certificate
                         certificate  --->
<------------------------------------------------------------------------------> cert exchange done
      verify certificate and ephemeral public key signature
<------------------------------------------------------------------------------> final authentication done; data exchange ON
                                data  <-->  data
<==============================================================================>
 */

/*  Encrypted Secure Channel data packet with HMAC of the packet appended
* 
*  +----------------+----------------+----------------+---------------//----------------++--------------//----------------+
*  |     data length (uint16)        | flags (8 bits) |              data               ||          HMAC (EtM)            |
*  +----------------+----------------+----------------+---------------//----------------++--------------//----------------+
*  
*/

namespace BitChatCore.Network.SecureChannel
{
    public enum SecureChannelCryptoOptionFlags : byte
    {
        None = 0,
        DHE2048_RSA_WITH_AES256_CBC_HMAC_SHA256 = 1,
        ECDHE256_RSA_WITH_AES256_CBC_HMAC_SHA256 = 2
    }

    abstract class SecureChannelStream : Stream
    {
        #region variables

        const byte HEADER_FLAG_NONE = 0;
        const byte HEADER_FLAG_RENEGOTIATE = 1;
        const byte HEADER_FLAG_CLOSE_CHANNEL = 2;

        readonly protected static RandomNumberGenerator _rnd = new RNGCryptoServiceProvider();
        readonly static RandomNumberGenerator _rndPadding = new RNGCryptoServiceProvider();

        readonly protected IPEndPoint _remotePeerEP;
        protected Certificate _remotePeerCert;

        //io & crypto related
        protected Stream _baseStream;
        protected SecureChannelCryptoOptionFlags _selectedCryptoOption;
        int _blockSizeBytes;
        SymmetricCryptoKey _encryptionKey;
        ICryptoTransform _cryptoEncryptor;
        ICryptoTransform _cryptoDecryptor;
        HMAC _authHMACEncrypt;
        HMAC _authHMACDecrypt;

        //re-negotiation
        long _reNegotiateOnBytesSent;
        int _reNegotiateAfterSeconds;
        bool _reNegotiating = false;
        long _bytesSent = 0;
        DateTime _connectedOn;
        Timer _reNegotiationTimer;
        const int _reNegotiationTimerInterval = 30000;

        readonly object _writeLock = new object();
        readonly object _readLock = new object();

        //buffering
        public const int MAX_PACKET_SIZE = 65504; //mod 32 round figure
        const int BUFFER_SIZE = 65535;

        int _authHMACSize;

        readonly byte[] _writeBufferData = new byte[BUFFER_SIZE];
        int _writeBufferPosition = 3;
        byte[] _writeBufferPadding;
        readonly byte[] _writeEncryptedData = new byte[BUFFER_SIZE];

        readonly byte[] _readBufferData = new byte[BUFFER_SIZE];
        int _readBufferPosition;
        int _readBufferLength;
        readonly byte[] _readEncryptedData = new byte[BUFFER_SIZE];

        MemoryStream _reNegotiateReadBuffer;
        bool _channelClosed = false;

        #endregion

        #region constructor

        public SecureChannelStream(IPEndPoint remotePeerEP, int reNegotiateOnBytesSent, int reNegotiateAfterSeconds)
        {
            _remotePeerEP = remotePeerEP;
            _reNegotiateOnBytesSent = reNegotiateOnBytesSent;
            _reNegotiateAfterSeconds = reNegotiateAfterSeconds;
        }

        #endregion

        #region IDisposable

        bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    _baseStream.Dispose();

                    _reNegotiationTimer.Dispose();

                    lock (_readLock)
                    {
                        if (_reNegotiateReadBuffer != null)
                        {
                            _reNegotiateReadBuffer.Dispose();
                            _reNegotiateReadBuffer = null;
                        }
                    }

                    _cryptoEncryptor.Dispose();
                    _cryptoDecryptor.Dispose();

                    _encryptionKey.Dispose();

                    _authHMACEncrypt.Dispose();
                    _authHMACDecrypt.Dispose();
                }
                finally
                {
                    base.Dispose(disposing);
                }

                _disposed = true;
            }
        }

        #endregion

        #region stream support

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanTimeout
        {
            get { return _baseStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return _baseStream.ReadTimeout; }
            set { _baseStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _baseStream.WriteTimeout; }
            set { _baseStream.WriteTimeout = value; }
        }

        public override long Length
        {
            get { throw new NotSupportedException("SecureChannel stream is not seekable."); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException("SecureChannel stream is not seekable.");
            }
            set
            {
                throw new NotSupportedException("SecureChannel stream is not seekable.");
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("SecureChannel stream is not seekable.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("SecureChannel stream is not seekable.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < 1)
                return;

            lock (_writeLock)
            {
                if (_channelClosed)
                    throw new ObjectDisposedException("SecureChannelStream"); //channel already closed

                do
                {
                    int bytesAvailable = MAX_PACKET_SIZE - _writeBufferPosition - _authHMACSize;
                    if (bytesAvailable < count)
                    {
                        if (bytesAvailable > 0)
                        {
                            Buffer.BlockCopy(buffer, offset, _writeBufferData, _writeBufferPosition, bytesAvailable);
                            _writeBufferPosition += bytesAvailable;
                            offset += bytesAvailable;
                            count -= bytesAvailable;
                        }

                        FlushBuffer(HEADER_FLAG_NONE);
                    }
                    else
                    {
                        Buffer.BlockCopy(buffer, offset, _writeBufferData, _writeBufferPosition, count);
                        _writeBufferPosition += count;
                        break;
                    }
                }
                while (true);
            }
        }

        public override void Flush()
        {
            lock (_writeLock)
            {
                if (_channelClosed)
                    throw new ObjectDisposedException("SecureChannelStream"); //channel already closed

                FlushBuffer(HEADER_FLAG_NONE);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count < 1)
                return 0;

            lock (_readLock)
            {
                int bytesAvailableForRead;

                if (_reNegotiateReadBuffer == null)
                    bytesAvailableForRead = _readBufferLength - _readBufferPosition;
                else
                    bytesAvailableForRead = Convert.ToInt32(_reNegotiateReadBuffer.Length - _reNegotiateReadBuffer.Position);

                while (bytesAvailableForRead < 1)
                {
                    if (_channelClosed)
                        return 0; //channel closed; end of stream

                    try
                    {
                        bytesAvailableForRead = ReadSecureChannelPacket();
                    }
                    catch (EndOfStreamException)
                    {
                        return 0;
                    }

                    //check header flags
                    switch (_readBufferData[2])
                    {
                        case HEADER_FLAG_RENEGOTIATE:
                            //received re-negotiate flag
                            lock (_writeLock)
                            {
                                _reNegotiating = true;
                                FlushBuffer(HEADER_FLAG_RENEGOTIATE);

                                StartReNegotiation();
                                _reNegotiating = false;
                            }
                            break;

                        case HEADER_FLAG_CLOSE_CHANNEL:
                            //received close channel flag
                            Close();
                            return 0;

                        default:
                            break;
                    }
                }

                {
                    int bytesToRead = count;

                    if (bytesToRead > bytesAvailableForRead)
                        bytesToRead = bytesAvailableForRead;

                    if (_reNegotiateReadBuffer == null)
                    {
                        Buffer.BlockCopy(_readBufferData, _readBufferPosition, buffer, offset, bytesToRead);
                        _readBufferPosition += bytesToRead;
                    }
                    else
                    {
                        _reNegotiateReadBuffer.Read(buffer, offset, bytesToRead);

                        if (_reNegotiateReadBuffer.Length == _reNegotiateReadBuffer.Position)
                        {
                            _reNegotiateReadBuffer.Dispose();
                            _reNegotiateReadBuffer = null;
                        }
                    }

                    return bytesToRead;
                }
            }
        }

        public override void Close()
        {
            lock (_writeLock)
            {
                if (_channelClosed)
                    return; //channel already closed

                try
                {
                    FlushBuffer(HEADER_FLAG_CLOSE_CHANNEL);
                }
                catch
                { }

                _channelClosed = true;
            }

            base.Close();
        }

        #endregion

        #region private

        private void FlushBuffer(byte headerFlag)
        {
            lock (_writeLock)
            {
                if ((_writeBufferPosition == 3) && (headerFlag == HEADER_FLAG_NONE))
                    return;

                _bytesSent += _writeBufferPosition - 3;

                //write secure channel packet

                //calc header and padding
                ushort dataLength = Convert.ToUInt16(_writeBufferPosition); //includes 3 bytes header length
                int bytesPadding = 0;
                int pendingBytes = dataLength % _blockSizeBytes;
                if (pendingBytes > 0)
                    bytesPadding = _blockSizeBytes - pendingBytes;

                //write header
                byte[] header = BitConverter.GetBytes(dataLength);
                _writeBufferData[0] = header[0];
                _writeBufferData[1] = header[1];
                _writeBufferData[2] = headerFlag;

                //write padding
                if (bytesPadding > 0)
                {
                    _rndPadding.GetBytes(_writeBufferPadding);

                    Buffer.BlockCopy(_writeBufferPadding, 0, _writeBufferData, _writeBufferPosition, bytesPadding);
                    _writeBufferPosition += bytesPadding;
                }

                //encrypt buffered data
                if (_cryptoEncryptor.CanTransformMultipleBlocks)
                {
                    _cryptoEncryptor.TransformBlock(_writeBufferData, 0, _writeBufferPosition, _writeEncryptedData, 0);
                }
                else
                {
                    for (int offset = 0; offset < _writeBufferPosition; offset += _blockSizeBytes)
                        _cryptoEncryptor.TransformBlock(_writeBufferData, offset, _blockSizeBytes, _writeEncryptedData, offset);
                }

                //append auth hmac to encrypted data
                byte[] authHMAC = _authHMACEncrypt.ComputeHash(_writeEncryptedData, 0, _writeBufferPosition);
                Buffer.BlockCopy(authHMAC, 0, _writeEncryptedData, _writeBufferPosition, authHMAC.Length);
                _writeBufferPosition += authHMAC.Length;

                //write encrypted data + auth hmac
                _baseStream.Write(_writeEncryptedData, 0, _writeBufferPosition);
                _baseStream.Flush();

                //reset buffer
                _writeBufferPosition = 3;
            }
        }

        private int ReadSecureChannelPacket()
        {
            //read secure channel packet

            //read first block to read the encrypted packet size
            OffsetStream.StreamRead(_baseStream, _readEncryptedData, 0, _blockSizeBytes);
            _cryptoDecryptor.TransformBlock(_readEncryptedData, 0, _blockSizeBytes, _readBufferData, 0);
            _readBufferPosition = _blockSizeBytes;

            //read packet header 2 byte length
            int dataLength = BitConverter.ToUInt16(_readBufferData, 0);
            _readBufferLength = dataLength;

            dataLength -= _blockSizeBytes;

            if (_cryptoDecryptor.CanTransformMultipleBlocks)
            {
                if (dataLength > 0)
                {
                    int pendingBlocks = dataLength / _blockSizeBytes;

                    if (dataLength % _blockSizeBytes > 0)
                        pendingBlocks++;

                    int pendingBytes = pendingBlocks * _blockSizeBytes;

                    //read pending blocks
                    OffsetStream.StreamRead(_baseStream, _readEncryptedData, _readBufferPosition, pendingBytes);
                    _cryptoDecryptor.TransformBlock(_readEncryptedData, _readBufferPosition, pendingBytes, _readBufferData, _readBufferPosition);
                    _readBufferPosition += pendingBytes;
                }
            }
            else
            {
                while (dataLength > 0)
                {
                    //read next block
                    OffsetStream.StreamRead(_baseStream, _readEncryptedData, _readBufferPosition, _blockSizeBytes);
                    _cryptoDecryptor.TransformBlock(_readEncryptedData, _readBufferPosition, _blockSizeBytes, _readBufferData, _readBufferPosition);
                    _readBufferPosition += _blockSizeBytes;

                    dataLength -= _blockSizeBytes;
                }
            }

            //read auth hmac
            BinaryNumber authHMAC = new BinaryNumber(new byte[_authHMACSize]);
            OffsetStream.StreamRead(_baseStream, authHMAC.Number, 0, _authHMACSize);

            //verify auth hmac with computed hmac
            BinaryNumber computedAuthHMAC = new BinaryNumber(_authHMACDecrypt.ComputeHash(_readEncryptedData, 0, _readBufferPosition));

            if (!computedAuthHMAC.Equals(authHMAC))
                throw new SecureChannelException(SecureChannelCode.InvalidMessageHMACReceived, _remotePeerEP, _remotePeerCert);

            _readBufferPosition = 3;

            //return bytes available in this packet to read
            return _readBufferLength - _readBufferPosition;
        }

        private void ReNegotiationTimerCallback(object state)
        {
            try
            {
                if (((_reNegotiateOnBytesSent > 0) && (_bytesSent > _reNegotiateOnBytesSent)) || ((_reNegotiateAfterSeconds > 0) && (_connectedOn.AddSeconds(_reNegotiateAfterSeconds) < DateTime.UtcNow)))
                    ReNegotiateNow();
            }
            catch
            { }
            finally
            {
                _reNegotiationTimer.Change(_reNegotiationTimerInterval, Timeout.Infinite);
            }
        }

        #endregion

        #region public

        public void ReNegotiateNow()
        {
            //re-negotiotion initiator needs to have a special read buffer to store the decrypted data that was on the way from other end while the negotiation start is indicated.

            lock (_readLock)
            {
                if (_reNegotiateReadBuffer != null)
                    return; //read buffer from previous re-negotiation is still not empty to proceed

                if (_channelClosed)
                    return; //channel already closed

                lock (_writeLock)
                {
                    _reNegotiating = true;
                    FlushBuffer(HEADER_FLAG_RENEGOTIATE);

                    //write available buffered data into special read buffer
                    int bytesAvailableForRead = _readBufferLength - _readBufferPosition;
                    if (bytesAvailableForRead > 0)
                    {
                        _reNegotiateReadBuffer = new MemoryStream(BUFFER_SIZE);
                        _reNegotiateReadBuffer.Write(_readBufferData, _readBufferPosition, bytesAvailableForRead);
                    }

                    //write in-transit data into special read buffer
                    do
                    {
                        bytesAvailableForRead = ReadSecureChannelPacket();
                        if (bytesAvailableForRead > 0)
                        {
                            if (_reNegotiateReadBuffer == null)
                                _reNegotiateReadBuffer = new MemoryStream(BUFFER_SIZE);

                            _reNegotiateReadBuffer.Write(_readBufferData, _readBufferPosition, bytesAvailableForRead);
                        }
                    }
                    while (_readBufferData[2] != 1);

                    //received re-negotiate flag

                    StartReNegotiation();
                    _reNegotiating = false;

                    if (_reNegotiateReadBuffer != null)
                        _reNegotiateReadBuffer.Position = 0;

                    _readBufferLength = 0;
                    _readBufferPosition = 0;
                }
            }
        }

        #endregion

        #region protected

        protected abstract void StartReNegotiation();

        protected bool IsReNegotiating()
        {
            return _reNegotiating;
        }

        protected byte[] GenerateMasterKey(SecureChannelHandshakeHello clientHello, SecureChannelHandshakeHello serverHello, byte[] preSharedKey, KeyAgreement keyAgreement, byte[] otherPartyPublicKey)
        {
            using (MemoryStream mS = new MemoryStream(128))
            {
                clientHello.WriteTo(mS);
                serverHello.WriteTo(mS);

                if (preSharedKey == null)
                    keyAgreement.HmacMessage = mS.ToArray();
                else
                    keyAgreement.HmacMessage = (new HMACSHA256(preSharedKey)).ComputeHash(mS.ToArray());
            }

            return keyAgreement.DeriveKeyMaterial(otherPartyPublicKey);
        }

        protected void EnableEncryption(Stream inputStream, SymmetricCryptoKey encryptionKey, SymmetricCryptoKey decryptionKey, HMAC authHMACEncrypt, HMAC authHMACDecrypt)
        {
            //create reader and writer objects
            _encryptionKey = encryptionKey;
            _cryptoEncryptor = encryptionKey.GetEncryptor();
            _cryptoDecryptor = decryptionKey.GetDecryptor();

            //init variables
            _baseStream = inputStream;
            _blockSizeBytes = encryptionKey.BlockSize / 8;
            _writeBufferPadding = new byte[_blockSizeBytes];
            _authHMACEncrypt = authHMACEncrypt;
            _authHMACDecrypt = authHMACDecrypt;
            _authHMACSize = authHMACEncrypt.HashSize / 8;

            _bytesSent = 0;
            _connectedOn = DateTime.UtcNow;

            if (_reNegotiationTimer == null)
            {
                if ((_reNegotiateOnBytesSent > 0) || (_reNegotiateAfterSeconds > 0))
                    _reNegotiationTimer = new Timer(ReNegotiationTimerCallback, null, _reNegotiationTimerInterval, Timeout.Infinite);
            }
        }

        #endregion

        #region properties

        public IPEndPoint RemotePeerEP
        { get { return _remotePeerEP; } }

        public Certificate RemotePeerCertificate
        { get { return _remotePeerCert; } }

        public SecureChannelCryptoOptionFlags SelectedCryptoOption
        { get { return _selectedCryptoOption; } }

        #endregion
    }
}
