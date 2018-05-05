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
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Security.Cryptography;

/*
=============
= VERSION 1 =
=============

FEATURES-
---------
 - random nonce on both ends to prevent replay attacks.
 - ephemeral keys used for key exchange to provide perfect forward secrecy (PFS).
 - optional pre-shared key based auth to prevent public key disclosure, preventing identity disclosure to active attacker.
 - encrypted public key authentication to prevent identity disclosure to passive sniffing attack.
 - UserId based public key authentication for ensuring identity and prevent MiTM.
 - secure channel data packet authenticated by HMACSHA256(cipher-text) to provide authenticated encryption (AE) in Encrypt-then-MAC (EtM) mode.
 - key auto renegotation feature for allowing the secure channel to remain always on.
 - server only UserId based authentication to allow blog feed services.
 - ANON mode to allow open group chat based on only PSK authentication.
 
<=======================================================================================>
                                  SERVER        CLIENT
<=======================================================================================>
                                                version +
                                                client nonce +
                                                supported ciphers +
                                          <---  options
                               version +  ---> 
                          server nonce +  
                       selected cipher +
                               options
<---------------------------------------------------------------------------------------> hello exchange done
           server ephemeral public key +
     PSK auth = HMACSHA256(
       server ephemeral public key + 
       server nonce + client nonce, PSK)  --->
                                          <---  client ephemeral public key +
                                                PSK auth = HMACSHA256(
                                                  client ephemeral public key + 
                                                  server nonce + client nonce, PSK)
<---------------------------------------------------------------------------------------> key exchange + PSK auth done
          master key = HMACSHA256(server nonce + client nonce, derived key)
<---------------------------------------------------------------------------------------> encryption layer ON
                                                (optional client authentication)
                                                userId +
                                                client public key +
                                                signature(client ephemeral public key + 
                                                  server nonce + client nonce, 
                                          <---    client public key)
                                 userId +  --->
                     server public key +
 signature(server ephemeral public key + 
   server nonce + client nonce, 
   server public key)
<---------------------------------------------------------------------------------------> do userId based authentication
                   match public key with userId and verify signature
<=======================================================================================> handshake complete
                                    data  <-->  data
<=======================================================================================>
 */

/*  Encrypted Secure Channel data packet with HMAC of the packet appended for authenticated encryption (AE) in Encrypt-then-MAC (EtM) mode
  
   +----------------+----------------+----------------+---------------//----------------++--------------//----------------+
   |    packet length (uint16)       | flags (8 bits) |              data               ||          HMAC (EtM)            |
   +----------------+----------------+----------------+---------------//----------------++--------------//----------------+
   
*/

namespace MeshCore.Network.SecureChannel
{
    public enum SecureChannelCipherSuite : byte
    {
        None = 0,
        DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256 = 1,
        DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256 = 2
    }

    public enum SecureChannelOptions : byte
    {
        None = 0,
        PRE_SHARED_KEY_AUTHENTICATION_REQUIRED = 1,
        CLIENT_AUTHENTICATION_REQUIRED = 2
    }

    public abstract class SecureChannelStream : Stream
    {
        #region variables

        const byte HEADER_FLAG_NONE = 0;
        const byte HEADER_FLAG_RENEGOTIATE = 1;
        const byte HEADER_FLAG_CLOSE_CHANNEL = 2;

        readonly protected static RandomNumberGenerator _rnd = new RNGCryptoServiceProvider();

        readonly protected IPEndPoint _remotePeerEP;
        protected BinaryNumber _remotePeerUserId;

        //io & crypto related
        protected Stream _baseStream;
        protected SecureChannelCipherSuite _selectedCipher;
        SymmetricAlgorithm _encryptionAlgo;
        SymmetricAlgorithm _decryptionAlgo;
        ICryptoTransform _encryptor;
        ICryptoTransform _decryptor;
        HMAC _authHMACEncrypt;
        HMAC _authHMACDecrypt;
        int _blockSizeBytes;
        int _authHMACSizeBytes;

        //renegotiation
        long _renegotiateAfterBytesSent;
        int _renegotiateAfterSeconds;
        long _bytesSent = 0;
        DateTime _connectedOn;
        Timer _renegotiationTimer;
        const int RENEGOTIATION_TIMER_INTERVAL = 30000;
        bool _isRenegotiating;
        readonly object _renegotiationLock = new object();

        readonly object _writeLock = new object();
        readonly object _readLock = new object();

        //buffering
        public const int MAX_PACKET_SIZE = 65504; //mod 32 round figure
        const int BUFFER_SIZE = 65535;

        readonly byte[] _writeBufferData = new byte[BUFFER_SIZE];
        int _writeBufferPosition = 3;
        byte[] _writeBufferPadding;
        readonly byte[] _writeEncryptedData = new byte[BUFFER_SIZE];

        readonly byte[] _readBufferData = new byte[BUFFER_SIZE];
        int _readBufferPosition;
        int _readBufferLength;
        readonly byte[] _readEncryptedData = new byte[BUFFER_SIZE];

        bool _secureChannelStreamClosed = false;

        #endregion

        #region constructor

        public SecureChannelStream(IPEndPoint remotePeerEP, int renegotiateAfterBytesSent, int renegotiateAfterSeconds)
        {
            _remotePeerEP = remotePeerEP;
            _renegotiateAfterBytesSent = renegotiateAfterBytesSent;
            _renegotiateAfterSeconds = renegotiateAfterSeconds;
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

                    _renegotiationTimer.Dispose();

                    _encryptor.Dispose();
                    _decryptor.Dispose();

                    _encryptionAlgo.Dispose();
                    _decryptionAlgo.Dispose();

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

        #region static

        public static BinaryNumber GenerateUserId(byte[] publicKey)
        {
            byte[] random = new byte[4];
            _rnd.GetBytes(random);

            byte[] hashValue;

            using (HMAC hash = new HMACSHA1(random))
            {
                hashValue = hash.ComputeHash(publicKey);
            }

            byte[] buffer = new byte[24];

            Buffer.BlockCopy(random, 0, buffer, 0, 4);
            Buffer.BlockCopy(hashValue, 0, buffer, 4, 20);

            return new BinaryNumber(buffer);
        }

        public static bool IsUserIdValid(BinaryNumber userId, byte[] publicKey)
        {
            if (userId.Value.Length != 24)
                return false;

            byte[] random = new byte[4];
            byte[] hashValue = new byte[20];

            Buffer.BlockCopy(userId.Value, 0, random, 0, 4);
            Buffer.BlockCopy(userId.Value, 4, hashValue, 0, 20);

            BinaryNumber generatedHashValue;

            using (HMAC hash = new HMACSHA1(random))
            {
                generatedHashValue = new BinaryNumber(hash.ComputeHash(publicKey));
            }

            return generatedHashValue.Equals(new BinaryNumber(hashValue));
        }

        #endregion

        #region stream support

        public override bool CanRead
        {
            get { return !_secureChannelStreamClosed; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return !_secureChannelStreamClosed; }
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
                if (_secureChannelStreamClosed)
                    throw new ObjectDisposedException("SecureChannelStream"); //channel already closed

                while (true)
                {
                    int bytesAvailable = MAX_PACKET_SIZE - _writeBufferPosition - _authHMACSizeBytes;
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
            }
        }

        public override void Flush()
        {
            lock (_writeLock)
            {
                if (_secureChannelStreamClosed)
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
                int bytesAvailableForRead = _readBufferLength - _readBufferPosition;

                while (bytesAvailableForRead < 1)
                {
                    if (_secureChannelStreamClosed)
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
                            //received renegotiate flag
                            if (_isRenegotiating)
                            {
                                //renegotiation initiator party
                                lock (_renegotiationLock)
                                {
                                    _isRenegotiating = false;

                                    //signal wait handle to proceed with renegotiation
                                    Monitor.Pulse(_renegotiationLock);
                                }

                                //release read lock for allowing renegotiation to take read lock and wait for it to complete
                                if (!Monitor.Wait(_readLock, ReadTimeout))
                                    throw new SecureChannelException(SecureChannelCode.RenegotiationFailed, _remotePeerEP, _remotePeerUserId, "Renegotiation timed out.");
                            }
                            else
                            {
                                //renegotiation receiver party
                                lock (_writeLock)
                                {
                                    FlushBuffer(HEADER_FLAG_RENEGOTIATE);

                                    StartRenegotiation();
                                }
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

                    Buffer.BlockCopy(_readBufferData, _readBufferPosition, buffer, offset, bytesToRead);
                    _readBufferPosition += bytesToRead;

                    return bytesToRead;
                }
            }
        }

        public override void Close()
        {
            lock (_writeLock)
            {
                if (_secureChannelStreamClosed)
                    return; //channel already closed

                try
                {
                    FlushBuffer(HEADER_FLAG_CLOSE_CHANNEL);
                }
                catch
                { }

                _secureChannelStreamClosed = true;
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
                    _rnd.GetBytes(_writeBufferPadding);

                    Buffer.BlockCopy(_writeBufferPadding, 0, _writeBufferData, _writeBufferPosition, bytesPadding);
                    _writeBufferPosition += bytesPadding;
                }

                //encrypt buffered data
                if (_encryptor.CanTransformMultipleBlocks)
                {
                    _encryptor.TransformBlock(_writeBufferData, 0, _writeBufferPosition, _writeEncryptedData, 0);
                }
                else
                {
                    for (int offset = 0; offset < _writeBufferPosition; offset += _blockSizeBytes)
                        _encryptor.TransformBlock(_writeBufferData, offset, _blockSizeBytes, _writeEncryptedData, offset);
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
            _decryptor.TransformBlock(_readEncryptedData, 0, _blockSizeBytes, _readBufferData, 0);
            _readBufferPosition = _blockSizeBytes;

            //read packet header 2 byte length
            int dataLength = BitConverter.ToUInt16(_readBufferData, 0);
            _readBufferLength = dataLength;

            dataLength -= _blockSizeBytes;

            if (_decryptor.CanTransformMultipleBlocks)
            {
                if (dataLength > 0)
                {
                    int pendingBlocks = dataLength / _blockSizeBytes;

                    if (dataLength % _blockSizeBytes > 0)
                        pendingBlocks++;

                    int pendingBytes = pendingBlocks * _blockSizeBytes;

                    //read pending blocks
                    OffsetStream.StreamRead(_baseStream, _readEncryptedData, _readBufferPosition, pendingBytes);
                    _decryptor.TransformBlock(_readEncryptedData, _readBufferPosition, pendingBytes, _readBufferData, _readBufferPosition);
                    _readBufferPosition += pendingBytes;
                }
            }
            else
            {
                while (dataLength > 0)
                {
                    //read next block
                    OffsetStream.StreamRead(_baseStream, _readEncryptedData, _readBufferPosition, _blockSizeBytes);
                    _decryptor.TransformBlock(_readEncryptedData, _readBufferPosition, _blockSizeBytes, _readBufferData, _readBufferPosition);
                    _readBufferPosition += _blockSizeBytes;

                    dataLength -= _blockSizeBytes;
                }
            }

            //read auth hmac
            BinaryNumber authHMAC = new BinaryNumber(new byte[_authHMACSizeBytes]);
            OffsetStream.StreamRead(_baseStream, authHMAC.Value, 0, _authHMACSizeBytes);

            //verify auth hmac with computed hmac
            BinaryNumber computedAuthHMAC = new BinaryNumber(_authHMACDecrypt.ComputeHash(_readEncryptedData, 0, _readBufferPosition));

            if (!computedAuthHMAC.Equals(authHMAC))
                throw new SecureChannelException(SecureChannelCode.MessageAuthenticationFailed, _remotePeerEP, _remotePeerUserId);

            _readBufferPosition = 3;

            //return bytes available in this packet to read
            return _readBufferLength - _readBufferPosition;
        }

        private void RenegotiationTimerCallback(object state)
        {
            try
            {
                if (((_renegotiateAfterBytesSent > 0) && (_bytesSent > _renegotiateAfterBytesSent)) || ((_renegotiateAfterSeconds > 0) && (_connectedOn.AddSeconds(_renegotiateAfterSeconds) < DateTime.UtcNow)))
                    RenegotiateNow();
            }
            catch
            { }
            finally
            {
                _renegotiationTimer.Change(RENEGOTIATION_TIMER_INTERVAL, Timeout.Infinite);
            }
        }

        #endregion

        #region public

        public void RenegotiateNow()
        {
            lock (_writeLock)
            {
                if (_secureChannelStreamClosed)
                    return; //channel already closed

                lock (_renegotiationLock)
                {
                    _isRenegotiating = true;

                    //send RENEGOTIATE packet
                    FlushBuffer(HEADER_FLAG_RENEGOTIATE);

                    //wait till other party responds with a return RENEGOTIATE packet
                    if (!Monitor.Wait(_renegotiationLock, ReadTimeout))
                        throw new SecureChannelException(SecureChannelCode.RenegotiationFailed, _remotePeerEP, _remotePeerUserId, "Renegotiation timed out.");

                    lock (_readLock)
                    {
                        StartRenegotiation();

                        //signal read thread to stop waiting and take read lock
                        Monitor.Pulse(_readLock);
                    }
                }
            }
        }

        #endregion

        #region protected

        protected abstract void StartRenegotiation();

        protected void EnableEncryption(Stream inputStream, SecureChannelHandshakeHello serverHello, SecureChannelHandshakeHello clientHello, KeyAgreement keyAgreement, SecureChannelHandshakeKeyExchange otherPartyKeyExchange)
        {
            using (MemoryStream mS = new MemoryStream(128))
            {
                mS.Write(serverHello.Nonce.Value, 0, serverHello.Nonce.Value.Length);
                mS.Write(clientHello.Nonce.Value, 0, clientHello.Nonce.Value.Length);

                keyAgreement.HmacMessage = mS.ToArray();
            }

            byte[] masterKey = keyAgreement.DeriveKeyMaterial(otherPartyKeyExchange.EphemeralPublicKey);

            switch (serverHello.SupportedCiphers)
            {
                case SecureChannelCipherSuite.DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256:

                    //generating AES IV of 128bit block size using MD5
                    byte[] eIV;
                    byte[] dIV;

                    using (HashAlgorithm hash = HashAlgorithm.Create("MD5"))
                    {
                        if (this is SecureChannelServerStream)
                        {
                            eIV = hash.ComputeHash(serverHello.Nonce.Value);
                            dIV = hash.ComputeHash(clientHello.Nonce.Value);
                        }
                        else
                        {
                            eIV = hash.ComputeHash(clientHello.Nonce.Value);
                            dIV = hash.ComputeHash(serverHello.Nonce.Value);
                        }
                    }

                    //create encryptor
                    _encryptionAlgo = Aes.Create();
                    _encryptionAlgo.Key = masterKey;
                    _encryptionAlgo.IV = eIV;
                    _encryptionAlgo.Padding = PaddingMode.None; //padding is managed by secure channel
                    _encryptionAlgo.Mode = CipherMode.CBC;

                    _encryptor = _encryptionAlgo.CreateEncryptor();
                    _authHMACEncrypt = new HMACSHA256(masterKey);

                    //create decryptor
                    _decryptionAlgo = Aes.Create();
                    _decryptionAlgo.Key = masterKey;
                    _decryptionAlgo.IV = dIV;
                    _decryptionAlgo.Padding = PaddingMode.None; //padding is managed by secure channel
                    _decryptionAlgo.Mode = CipherMode.CBC;

                    _decryptor = _decryptionAlgo.CreateDecryptor();
                    _authHMACDecrypt = new HMACSHA256(masterKey);

                    //init variables
                    _blockSizeBytes = _encryptionAlgo.BlockSize / 8;
                    _writeBufferPadding = new byte[_blockSizeBytes];

                    _authHMACSizeBytes = _authHMACEncrypt.HashSize / 8;
                    break;

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, _remotePeerEP, _remotePeerUserId);
            }

            //init variables
            _baseStream = inputStream;
            _bytesSent = 0;
            _connectedOn = DateTime.UtcNow;

            if (_renegotiationTimer == null)
            {
                if ((_renegotiateAfterBytesSent > 0) || (_renegotiateAfterSeconds > 0))
                    _renegotiationTimer = new Timer(RenegotiationTimerCallback, null, RENEGOTIATION_TIMER_INTERVAL, Timeout.Infinite);
            }
        }

        #endregion

        #region properties

        public IPEndPoint RemotePeerEP
        { get { return _remotePeerEP; } }

        public BinaryNumber RemotePeerUserId
        { get { return _remotePeerUserId; } }

        public SecureChannelCipherSuite SelectedCipher
        { get { return _selectedCipher; } }

        #endregion
    }
}
