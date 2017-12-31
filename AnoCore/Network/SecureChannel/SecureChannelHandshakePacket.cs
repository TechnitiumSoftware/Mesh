/*
Technitium Bit Chat
Copyright (C) 2017  Shreyas Zare (shreyas@technitium.com)

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
using TechnitiumLibrary.Security.Cryptography;

/*  Secure Channel Handshake Packet
* 
*  +----------------+---------------//----------------+
*  |  code (8 bits) |     optional packet data        |
*  +----------------+---------------//----------------+
*  
*/

namespace BitChatCore.Network.SecureChannel
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

        readonly BinaryNumber _nonce;
        readonly SecureChannelCryptoOptionFlags _cryptoOptions;

        #endregion

        #region constructor

        public SecureChannelHandshakeHello(BinaryNumber nonce, SecureChannelCryptoOptionFlags cryptoOptions)
            : base(SecureChannelCode.None)
        {
            _nonce = nonce;
            _cryptoOptions = cryptoOptions;
        }

        public SecureChannelHandshakeHello(Stream s)
            : base(s)
        {
            _nonce = new BinaryNumber(s);
            _cryptoOptions = (SecureChannelCryptoOptionFlags)s.ReadByte();
        }

        #endregion

        #region public

        public override void WriteTo(Stream s)
        {
            base.WriteTo(s);

            _nonce.WriteTo(s);
            s.WriteByte((byte)_cryptoOptions);
        }

        #endregion

        #region properties

        public BinaryNumber Nonce
        { get { return _nonce; } }

        public SecureChannelCryptoOptionFlags CryptoOptions
        { get { return _cryptoOptions; } }

        #endregion
    }

    class SecureChannelHandshakeKeyExchange : SecureChannelHandshakePacket
    {
        #region variables

        readonly byte[] _publicKey;
        readonly byte[] _signature;

        #endregion

        #region constructor

        public SecureChannelHandshakeKeyExchange(byte[] publicKey, AsymmetricCryptoKey privateKey, string hashAlgo)
            : base(SecureChannelCode.None)
        {
            _publicKey = publicKey;
            _signature = privateKey.Sign(new MemoryStream(_publicKey, false), hashAlgo);
        }

        public SecureChannelHandshakeKeyExchange(Stream s)
            : base(s)
        {
            byte[] buffer = new byte[2];
            ushort length;

            OffsetStream.StreamRead(s, buffer, 0, 2);
            length = BitConverter.ToUInt16(buffer, 0);
            _publicKey = new byte[length];
            OffsetStream.StreamRead(s, _publicKey, 0, length);

            OffsetStream.StreamRead(s, buffer, 0, 2);
            length = BitConverter.ToUInt16(buffer, 0);
            _signature = new byte[length];
            OffsetStream.StreamRead(s, _signature, 0, length);
        }

        #endregion

        #region public

        public bool IsSignatureValid(Certificate signingCert, string hashAlgo)
        {
            return AsymmetricCryptoKey.Verify(new MemoryStream(_publicKey, false), _signature, hashAlgo, signingCert);
        }

        public override void WriteTo(Stream s)
        {
            base.WriteTo(s);

            s.Write(BitConverter.GetBytes(Convert.ToUInt16(_publicKey.Length)), 0, 2);
            s.Write(_publicKey, 0, _publicKey.Length);

            s.Write(BitConverter.GetBytes(Convert.ToUInt16(_signature.Length)), 0, 2);
            s.Write(_signature, 0, _signature.Length);
        }

        #endregion

        #region properties

        public byte[] PublicKey
        { get { return _publicKey; } }

        #endregion
    }

    class SecureChannelHandshakeAuthentication : SecureChannelHandshakePacket
    {
        #region variables

        readonly BinaryNumber _hmac;

        #endregion

        #region constructor

        public SecureChannelHandshakeAuthentication(SecureChannelHandshakeHello hello, byte[] masterKey)
            : base(SecureChannelCode.None)
        {
            using (MemoryStream mS = new MemoryStream(32))
            {
                hello.WriteTo(mS);
                mS.Position = 0;

                _hmac = new BinaryNumber((new HMACSHA256(masterKey)).ComputeHash(mS));
            }
        }

        public SecureChannelHandshakeAuthentication(Stream s)
            : base(s)
        {
            _hmac = new BinaryNumber(s);
        }

        #endregion

        #region public

        public bool IsValid(SecureChannelHandshakeHello hello, byte[] masterKey)
        {
            using (MemoryStream mS = new MemoryStream(32))
            {
                hello.WriteTo(mS);
                mS.Position = 0;

                BinaryNumber computedHmac = new BinaryNumber((new HMACSHA256(masterKey)).ComputeHash(mS));
                return _hmac.Equals(computedHmac);
            }
        }

        public override void WriteTo(Stream s)
        {
            base.WriteTo(s);

            _hmac.WriteTo(s);
        }

        #endregion
    }

    class SecureChannelHandshakeCertificate : SecureChannelHandshakePacket
    {
        #region variables

        readonly Certificate _cert;

        #endregion

        #region constructor

        public SecureChannelHandshakeCertificate(Certificate cert)
            : base(SecureChannelCode.None)
        {
            _cert = cert;
        }

        public SecureChannelHandshakeCertificate(Stream s)
            : base(s)
        {
            _cert = new Certificate(s);
        }

        #endregion

        #region public

        public override void WriteTo(Stream s)
        {
            base.WriteTo(s);

            _cert.WriteTo(s);
        }

        #endregion

        #region properties

        public Certificate Certificate
        { get { return _cert; } }

        #endregion
    }
}
