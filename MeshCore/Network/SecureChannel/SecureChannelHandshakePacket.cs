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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Security.Cryptography;

/*  Secure Channel Handshake Packet
* 
*  +----------------+---------------//----------------+
*  |  code (8 bits) |     optional packet data        |
*  +----------------+---------------//----------------+
*  
*/

namespace MeshCore.Network.SecureChannel
{
    public class SecureChannelHandshakePacket
    {
        #region variables

        readonly SecureChannelCode _code;

        #endregion

        #region constructor

        public SecureChannelHandshakePacket(SecureChannelCode code)
        {
            _code = code;
        }

        public SecureChannelHandshakePacket(Stream s)
        {
            int code = s.ReadByte();
            if (code == -1)
                throw new EndOfStreamException();

            _code = (SecureChannelCode)code;
            if (_code != SecureChannelCode.None)
                throw new SecureChannelException(SecureChannelCode.RemoteError, null, null, "Remote client sent an error response code: " + _code.ToString());
        }

        #endregion

        #region public

        public virtual void WriteTo(Stream s)
        {
            s.WriteByte((byte)_code);
        }

        #endregion
    }

    public class SecureChannelHandshakeHello : SecureChannelHandshakePacket
    {
        #region variables

        readonly byte _version;
        readonly BinaryNumber _nonce;
        readonly SecureChannelCipherSuite _supportedCiphers;
        readonly SecureChannelOptions _options;

        #endregion

        #region constructor

        public SecureChannelHandshakeHello(SecureChannelCipherSuite supportedCiphers, SecureChannelOptions options)
            : base(SecureChannelCode.None)
        {
            _version = 1;
            _nonce = BinaryNumber.GenerateRandomNumber256();
            _supportedCiphers = supportedCiphers;
            _options = options;
        }

        public SecureChannelHandshakeHello(Stream s)
            : base(s)
        {
            int version = s.ReadByte();

            switch (version)
            {
                case -1:
                    throw new EndOfStreamException();

                case 1:
                    _version = (byte)version;
                    _nonce = new BinaryNumber(s);
                    _supportedCiphers = (SecureChannelCipherSuite)s.ReadByte();
                    _options = (SecureChannelOptions)s.ReadByte();
                    break;

                default:
                    _version = (byte)version;
                    break;
            }
        }

        #endregion

        #region public

        public override void WriteTo(Stream s)
        {
            base.WriteTo(s);

            s.WriteByte(_version);
            _nonce.WriteTo(s);
            s.WriteByte((byte)_supportedCiphers);
            s.WriteByte((byte)_options);
        }

        #endregion

        #region properties

        public byte Version
        { get { return _version; } }

        public BinaryNumber Nonce
        { get { return _nonce; } }

        public SecureChannelCipherSuite SupportedCiphers
        { get { return _supportedCiphers; } }

        public SecureChannelOptions Options
        { get { return _options; } }

        #endregion
    }

    public class SecureChannelHandshakeKeyExchange : SecureChannelHandshakePacket
    {
        #region variables

        readonly byte[] _ephemeralPublicKey;
        readonly BinaryNumber _pskAuth;

        #endregion

        #region constructor

        public SecureChannelHandshakeKeyExchange(KeyAgreement keyAgreement, SecureChannelHandshakeHello serverHello, SecureChannelHandshakeHello clientHello, byte[] psk)
            : base(SecureChannelCode.None)
        {
            _ephemeralPublicKey = keyAgreement.GetPublicKey();

            if (serverHello.Options.HasFlag(SecureChannelOptions.PRE_SHARED_KEY_AUTHENTICATION_REQUIRED))
                _pskAuth = GetPskAuthValue(serverHello.SupportedCiphers, _ephemeralPublicKey, serverHello.Nonce.Value, clientHello.Nonce.Value, psk);
            else
                _pskAuth = new BinaryNumber(new byte[] { });
        }

        public SecureChannelHandshakeKeyExchange(Stream s)
            : base(s)
        {
            byte[] buffer = new byte[2];
            s.ReadBytes(buffer, 0, 2);
            _ephemeralPublicKey = new byte[BitConverter.ToUInt16(buffer, 0)];
            s.ReadBytes(_ephemeralPublicKey, 0, _ephemeralPublicKey.Length);

            _pskAuth = new BinaryNumber(s);
        }

        #endregion

        #region private

        private BinaryNumber GetPskAuthValue(SecureChannelCipherSuite cipher, byte[] ephemeralPublicKey, byte[] serverNonce, byte[] clientNonce, byte[] psk)
        {
            using (MemoryStream mS = new MemoryStream())
            {
                mS.Write(ephemeralPublicKey, 0, ephemeralPublicKey.Length);
                mS.Write(serverNonce, 0, serverNonce.Length);
                mS.Write(clientNonce, 0, clientNonce.Length);
                mS.Position = 0;

                if (psk == null)
                    psk = new byte[] { };

                switch (cipher)
                {
                    case SecureChannelCipherSuite.DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256:
                    case SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                    case SecureChannelCipherSuite.ECDHE256_ANON_WITH_AES256_CBC_HMAC_SHA256:
                    case SecureChannelCipherSuite.ECDHE256_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                        using (HMAC hmac = new HMACSHA256(psk))
                        {
                            return new BinaryNumber(hmac.ComputeHash(mS));
                        }

                    default:
                        throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, null, null);
                }
            }
        }

        #endregion

        #region public

        public bool IsPskAuthValid(SecureChannelHandshakeHello serverHello, SecureChannelHandshakeHello clientHello, byte[] psk)
        {
            BinaryNumber generatedPskAuthValue = GetPskAuthValue(serverHello.SupportedCiphers, _ephemeralPublicKey, serverHello.Nonce.Value, clientHello.Nonce.Value, psk);

            return _pskAuth.Equals(generatedPskAuthValue);
        }

        public override void WriteTo(Stream s)
        {
            base.WriteTo(s);

            s.Write(BitConverter.GetBytes(Convert.ToUInt16(_ephemeralPublicKey.Length)), 0, 2);
            s.Write(_ephemeralPublicKey, 0, _ephemeralPublicKey.Length);

            _pskAuth.WriteTo(s);
        }

        #endregion

        #region properties

        public byte[] EphemeralPublicKey
        { get { return _ephemeralPublicKey; } }

        #endregion
    }

    public class SecureChannelHandshakeAuthentication : SecureChannelHandshakePacket
    {
        #region variables

        readonly BinaryNumber _userId;
        readonly byte[] _publicKey;
        readonly byte[] _signature;

        #endregion

        #region constructor

        public SecureChannelHandshakeAuthentication(SecureChannelHandshakeKeyExchange keyExchange, SecureChannelHandshakeHello serverHello, SecureChannelHandshakeHello clientHello, BinaryNumber userId, byte[] privateKey)
            : base(SecureChannelCode.None)
        {
            _userId = userId;

            switch (serverHello.SupportedCiphers)
            {
                case SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCipherSuite.ECDHE256_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                    using (RSA rsa = RSA.Create())
                    {
                        RSAParameters rsaPrivateKey = DEREncoding.DecodeRSAPrivateKey(privateKey);
                        rsa.ImportParameters(rsaPrivateKey);

                        if (rsa.KeySize != 2048)
                            throw new SecureChannelException(SecureChannelCode.PeerAuthenticationFailed, null, _userId, "RSA key size is not valid for selected crypto option: " + serverHello.SupportedCiphers.ToString());

                        _publicKey = DEREncoding.EncodeRSAPublicKey(rsaPrivateKey);

                        if (!SecureChannelStream.IsUserIdValid(_publicKey, _userId))
                            throw new SecureChannelException(SecureChannelCode.PeerAuthenticationFailed, null, _userId, "UserId does not match with public key.");

                        using (MemoryStream mS = new MemoryStream())
                        {
                            mS.Write(keyExchange.EphemeralPublicKey);
                            mS.Write(serverHello.Nonce.Value);
                            mS.Write(clientHello.Nonce.Value);
                            mS.Position = 0;

                            _signature = rsa.SignData(mS, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                        }
                    }
                    break;

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, null, null);
            }
        }

        public SecureChannelHandshakeAuthentication(Stream s)
            : base(s)
        {
            _userId = new BinaryNumber(s);

            byte[] buffer = new byte[2];

            s.ReadBytes(buffer, 0, 2);
            _publicKey = new byte[BitConverter.ToUInt16(buffer, 0)];
            s.ReadBytes(_publicKey, 0, _publicKey.Length);

            s.ReadBytes(buffer, 0, 2);
            _signature = new byte[BitConverter.ToUInt16(buffer, 0)];
            s.ReadBytes(_signature, 0, _signature.Length);
        }

        #endregion

        #region public

        public bool IsTrustedUserId(IEnumerable<BinaryNumber> trustedUserIds)
        {
            if (trustedUserIds == null)
                return true; //trust any userId

            foreach (BinaryNumber trustedUserId in trustedUserIds)
            {
                if (trustedUserId == _userId)
                    return true;
            }

            return false;
        }

        public bool IsSignatureValid(SecureChannelHandshakeKeyExchange keyExchange, SecureChannelHandshakeHello serverHello, SecureChannelHandshakeHello clientHello)
        {
            if (!SecureChannelStream.IsUserIdValid(_publicKey, _userId))
                return false;

            switch (serverHello.SupportedCiphers)
            {
                case SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCipherSuite.ECDHE256_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                    using (RSA rsa = RSA.Create())
                    {
                        RSAParameters rsaPublicKey = DEREncoding.DecodeRSAPublicKey(_publicKey);
                        rsa.ImportParameters(rsaPublicKey);

                        if (rsa.KeySize != 2048)
                            throw new SecureChannelException(SecureChannelCode.PeerAuthenticationFailed, null, _userId, "RSA key size is not valid for selected crypto option: " + serverHello.SupportedCiphers.ToString());

                        using (MemoryStream mS = new MemoryStream())
                        {
                            mS.Write(keyExchange.EphemeralPublicKey);
                            mS.Write(serverHello.Nonce.Value);
                            mS.Write(clientHello.Nonce.Value);
                            mS.Position = 0;

                            return rsa.VerifyData(mS, _signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                        }
                    }

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, null, null);
            }
        }

        public override void WriteTo(Stream s)
        {
            base.WriteTo(s);

            _userId.WriteTo(s);

            s.Write(BitConverter.GetBytes(Convert.ToUInt16(_publicKey.Length)), 0, 2);
            s.Write(_publicKey, 0, _publicKey.Length);

            s.Write(BitConverter.GetBytes(Convert.ToUInt16(_signature.Length)), 0, 2);
            s.Write(_signature, 0, _signature.Length);
        }

        #endregion

        #region properties

        public BinaryNumber UserId
        { get { return _userId; } }

        public byte[] PublicKey
        { get { return _publicKey; } }

        #endregion
    }
}
