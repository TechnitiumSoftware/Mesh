/*
Technitium Mesh
Copyright (C) 2019  Shreyas Zare (shreyas@technitium.com)

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
                                                (optional client authentication based on hello options)
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
    [Flags]
    public enum SecureChannelCipherSuite : byte
    {
        None = 0,
        DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256 = 1,
        DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256 = 2,
        ECDHE256_ANON_WITH_AES256_CBC_HMAC_SHA256 = 4,
        ECDHE256_RSA2048_WITH_AES256_CBC_HMAC_SHA256 = 8
    }

    [Flags]
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

        readonly protected static RandomNumberGenerator _rng = new RNGCryptoServiceProvider();

        readonly protected EndPoint _remotePeerEP;
        readonly EndPoint _viaRemotePeerEP;
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
        readonly long _renegotiateAfterBytesSent;
        readonly int _renegotiateAfterSeconds;
        long _bytesSent = 0;
        DateTime _connectedOn;
        Timer _renegotiationTimer;
        const int RENEGOTIATION_TIMER_INTERVAL = 30000;
        volatile bool _isRenegotiating;
        readonly object _renegotiationLock = new object();

        readonly object _writeLock = new object();
        readonly object _readLock = new object();

        //buffering
        public const int MAX_PACKET_SIZE = 65520; //mod 16 round figure
        const int BUFFER_SIZE = MAX_PACKET_SIZE + 32;

        readonly byte[] _writeBufferData = new byte[BUFFER_SIZE];
        volatile int _writeBufferPosition = 3;
        byte[] _writeBufferPadding;
        readonly byte[] _writeEncryptedData = new byte[BUFFER_SIZE];

        readonly byte[] _readBufferData = new byte[BUFFER_SIZE];
        volatile int _readBufferPosition;
        volatile int _readBufferCount;
        readonly byte[] _readEncryptedData = new byte[BUFFER_SIZE];

        #endregion

        #region constructor

        public SecureChannelStream(EndPoint remotePeerEP, EndPoint viaRemotePeerEP, int renegotiateAfterBytesSent, int renegotiateAfterSeconds)
        {
            _remotePeerEP = remotePeerEP;
            _viaRemotePeerEP = viaRemotePeerEP;
            _renegotiateAfterBytesSent = renegotiateAfterBytesSent;
            _renegotiateAfterSeconds = renegotiateAfterSeconds;
        }

        #endregion

        #region IDisposable

        volatile bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //send close channel signal
                lock (_writeLock)
                {
                    try
                    {
                        FlushBuffer(HEADER_FLAG_CLOSE_CHANNEL);
                    }
                    catch
                    { }
                }

                //dispose
                if (_baseStream != null)
                    _baseStream.Dispose();

                if (_renegotiationTimer != null)
                    _renegotiationTimer.Dispose();

                if (_encryptor != null)
                    _encryptor.Dispose();

                if (_decryptor != null)
                    _decryptor.Dispose();

                if (_encryptionAlgo != null)
                    _encryptionAlgo.Dispose();

                if (_decryptionAlgo != null)
                    _decryptionAlgo.Dispose();

                if (_authHMACEncrypt != null)
                    _authHMACEncrypt.Dispose();

                if (_authHMACDecrypt != null)
                    _authHMACDecrypt.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        #endregion

        #region static

        public static byte[] GetPublicKeyFromPrivateKey(byte[] privateKey)
        {
            return DEREncoding.EncodeRSAPublicKey(DEREncoding.DecodeRSAPrivateKey(privateKey));
        }

        private static byte[] GenerateUserId(byte[] publicKey, byte[] random)
        {
            byte[] value;

            using (HMAC hmac = new HMACSHA256(random))
            {
                value = hmac.ComputeHash(publicKey);
            }

            Buffer.BlockCopy(random, 0, value, 0, 4); //overwrite random 4 bytes at start

            byte[] userId = new byte[20];
            Buffer.BlockCopy(value, 0, userId, 0, 20);

            return userId;
        }

        public static BinaryNumber GenerateUserId(byte[] publicKey)
        {
            byte[] random = new byte[4];
            _rng.GetBytes(random);

            return new BinaryNumber(GenerateUserId(publicKey, random));
        }

        public static bool IsUserIdValid(byte[] publicKey, BinaryNumber userId)
        {
            if (userId.Value.Length != 20)
                return false;

            byte[] random = new byte[4];
            Buffer.BlockCopy(userId.Value, 0, random, 0, 4);

            byte[] generatedValue = GenerateUserId(publicKey, random);

            return BinaryNumber.Equals(userId.Value, generatedValue);
        }

        #endregion

        #region stream support

        public override bool CanRead
        {
            get { return !_disposed; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return !_disposed; }
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
                if (_disposed)
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
                if (_disposed)
                    throw new ObjectDisposedException("SecureChannelStream"); //channel already closed

                FlushBuffer(HEADER_FLAG_NONE);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("Count cannot be less than 1.");

            lock (_readLock)
            {
                int bytesAvailableForRead = _readBufferCount - _readBufferPosition;

                while (bytesAvailableForRead < 1)
                {
                    if (_disposed)
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
                                    FlushBuffer(HEADER_FLAG_NONE); //flush buffered data
                                    FlushBuffer(HEADER_FLAG_RENEGOTIATE); //send RENEGOTIATE packet

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
                _writeBufferData[0] = (byte)(dataLength & 0xff);
                _writeBufferData[1] = (byte)(dataLength >> 8);
                _writeBufferData[2] = headerFlag;

                //write padding
                if (bytesPadding > 0)
                {
                    _rng.GetBytes(_writeBufferPadding);

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

                try
                {
                    //write encrypted data + auth hmac
                    _baseStream.Write(_writeEncryptedData, 0, _writeBufferPosition);
                    _baseStream.Flush();
                }
                finally
                {
                    //reset buffer
                    _writeBufferPosition = 3;
                }
            }
        }

        private int ReadSecureChannelPacket()
        {
            //read secure channel packet

            //read first block to read the encrypted packet size
            _baseStream.ReadBytes(_readEncryptedData, 0, _blockSizeBytes);
            _decryptor.TransformBlock(_readEncryptedData, 0, _blockSizeBytes, _readBufferData, 0);
            _readBufferPosition = _blockSizeBytes;

            //read packet header 2 byte length
            int dataLength = BitConverter.ToUInt16(_readBufferData, 0);
            _readBufferCount = dataLength;

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
                    _baseStream.ReadBytes(_readEncryptedData, _readBufferPosition, pendingBytes);
                    _decryptor.TransformBlock(_readEncryptedData, _readBufferPosition, pendingBytes, _readBufferData, _readBufferPosition);
                    _readBufferPosition += pendingBytes;
                }
            }
            else
            {
                while (dataLength > 0)
                {
                    //read next block
                    _baseStream.ReadBytes(_readEncryptedData, _readBufferPosition, _blockSizeBytes);
                    _decryptor.TransformBlock(_readEncryptedData, _readBufferPosition, _blockSizeBytes, _readBufferData, _readBufferPosition);
                    _readBufferPosition += _blockSizeBytes;

                    dataLength -= _blockSizeBytes;
                }
            }

            //read auth hmac
            BinaryNumber authHMAC = new BinaryNumber(new byte[_authHMACSizeBytes]);
            _baseStream.ReadBytes(authHMAC.Value, 0, _authHMACSizeBytes);

            //verify auth hmac with computed hmac
            BinaryNumber computedAuthHMAC = new BinaryNumber(_authHMACDecrypt.ComputeHash(_readEncryptedData, 0, _readBufferPosition));

            if (!computedAuthHMAC.Equals(authHMAC))
                throw new SecureChannelException(SecureChannelCode.MessageAuthenticationFailed, _remotePeerEP, _remotePeerUserId);

            _readBufferPosition = 3;

            //return bytes available in this packet to read
            return _readBufferCount - _readBufferPosition;
        }

        #endregion

        #region public

        public void RenegotiateNow()
        {
            lock (_writeLock)
            {
                if (_disposed)
                    return; //channel already closed

                lock (_renegotiationLock)
                {
                    _isRenegotiating = true;

                    FlushBuffer(HEADER_FLAG_NONE); //flush buffered data
                    FlushBuffer(HEADER_FLAG_RENEGOTIATE); //send RENEGOTIATE packet

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
                mS.Write(serverHello.Nonce.Value);
                mS.Write(clientHello.Nonce.Value);

                keyAgreement.HmacKey = mS.ToArray();
            }

            byte[] masterKey = keyAgreement.DeriveKeyMaterial(otherPartyKeyExchange.EphemeralPublicKey);

            switch (serverHello.SupportedCiphers)
            {
                case SecureChannelCipherSuite.DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCipherSuite.ECDHE256_ANON_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCipherSuite.ECDHE256_RSA2048_WITH_AES256_CBC_HMAC_SHA256:

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
                {
                    _renegotiationTimer = new Timer(delegate (object state)
                    {
                        try
                        {
                            if (((_renegotiateAfterBytesSent > 0) && (_bytesSent > _renegotiateAfterBytesSent)) || ((_renegotiateAfterSeconds > 0) && (_connectedOn.AddSeconds(_renegotiateAfterSeconds) < DateTime.UtcNow)))
                            {
                                Debug.Write(this.GetType().Name, "Renegotiation triggered");

                                RenegotiateNow();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(this.GetType().Name, ex);
                        }
                    }, null, RENEGOTIATION_TIMER_INTERVAL, RENEGOTIATION_TIMER_INTERVAL);
                }
            }
        }

        #endregion

        #region properties

        public EndPoint RemotePeerEP
        { get { return _remotePeerEP; } }

        public EndPoint ViaRemotePeerEP
        { get { return _viaRemotePeerEP; } }

        public BinaryNumber RemotePeerUserId
        { get { return _remotePeerUserId; } }

        public SecureChannelCipherSuite SelectedCipher
        { get { return _selectedCipher; } }

        #endregion
    }
}
