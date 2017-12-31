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
using System.Net;
using System.Security.Cryptography;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Security.Cryptography;

namespace BitChatCore.Network.SecureChannel
{
    class SecureChannelClientStream : SecureChannelStream
    {
        #region variables

        readonly int _version;

        readonly CertificateStore _clientCredentials;
        readonly Certificate[] _trustedRootCertificates;
        readonly ISecureChannelSecurityManager _manager;
        readonly SecureChannelCryptoOptionFlags _supportedOptions;
        readonly byte[] _preSharedKey;

        #endregion

        #region constructor

        public SecureChannelClientStream(Stream stream, IPEndPoint remotePeerEP, CertificateStore clientCredentials, Certificate[] trustedRootCertificates, ISecureChannelSecurityManager manager, SecureChannelCryptoOptionFlags supportedOptions, int reNegotiateOnBytesSent, int reNegotiateAfterSeconds, byte[] preSharedKey)
            : base(remotePeerEP, reNegotiateOnBytesSent, reNegotiateAfterSeconds)
        {
            _clientCredentials = clientCredentials;
            _trustedRootCertificates = trustedRootCertificates;
            _manager = manager;
            _supportedOptions = supportedOptions;
            _preSharedKey = preSharedKey;

            try
            {
                //read server protocol version
                _version = stream.ReadByte();

                switch (_version)
                {
                    case 4:
                        //send supported client version
                        stream.WriteByte(4);

                        ProtocolV4(stream);
                        break;

                    case -1:
                        throw new EndOfStreamException();

                    default:
                        throw new SecureChannelException(SecureChannelCode.ProtocolVersionNotSupported, _remotePeerEP, _remotePeerCert, "SecureChannel protocol version '" + _version + "' not supported.");
                }
            }
            catch (SecureChannelException ex)
            {
                if (ex.Code == SecureChannelCode.RemoteError)
                {
                    throw new SecureChannelException(ex.Code, _remotePeerEP, _remotePeerCert, ex.Message, ex);
                }
                else
                {
                    try
                    {
                        Stream s;

                        if (_baseStream == null)
                            s = stream;
                        else
                            s = this;

                        new SecureChannelHandshakePacket(ex.Code).WriteTo(s);
                        s.Flush();
                    }
                    catch
                    { }

                    throw;
                }
            }
            catch (IOException)
            {
                throw;
            }
            catch
            {
                try
                {
                    Stream s;

                    if (_baseStream == null)
                        s = stream;
                    else
                        s = this;

                    new SecureChannelHandshakePacket(SecureChannelCode.UnknownException).WriteTo(s);
                    s.Flush();
                }
                catch
                { }

                throw;
            }
        }

        #endregion

        #region private

        private void ProtocolV4(Stream stream)
        {
            WriteBufferedStream bufferedStream = new WriteBufferedStream(stream, 8 * 1024);

            #region 1. hello handshake

            //send client hello
            SecureChannelHandshakeHello clientHello = new SecureChannelHandshakeHello(BinaryNumber.GenerateRandomNumber256(), _supportedOptions);
            clientHello.WriteTo(bufferedStream);
            bufferedStream.Flush();

            //read server hello
            SecureChannelHandshakeHello serverHello = new SecureChannelHandshakeHello(bufferedStream);

            //read selected crypto option
            _selectedCryptoOption = _supportedOptions & serverHello.CryptoOptions;

            if (_selectedCryptoOption == SecureChannelCryptoOptionFlags.None)
                throw new SecureChannelException(SecureChannelCode.NoMatchingCryptoAvailable, _remotePeerEP, _remotePeerCert);

            #endregion

            #region 2. key exchange

            //read server key exchange data
            SecureChannelHandshakeKeyExchange serverKeyExchange = new SecureChannelHandshakeKeyExchange(bufferedStream);

            SymmetricEncryptionAlgorithm encAlgo;
            string hashAlgo;
            KeyAgreement keyAgreement;

            switch (_selectedCryptoOption)
            {
                case SecureChannelCryptoOptionFlags.DHE2048_RSA_WITH_AES256_CBC_HMAC_SHA256:
                    encAlgo = SymmetricEncryptionAlgorithm.Rijndael;
                    hashAlgo = "SHA256";
                    keyAgreement = new DiffieHellman(DiffieHellmanGroupType.RFC3526, 2048, KeyAgreementKeyDerivationFunction.Hmac, KeyAgreementKeyDerivationHashAlgorithm.SHA256);
                    break;

                case SecureChannelCryptoOptionFlags.ECDHE256_RSA_WITH_AES256_CBC_HMAC_SHA256:
                    encAlgo = SymmetricEncryptionAlgorithm.Rijndael;
                    hashAlgo = "SHA256";
                    keyAgreement = new TechnitiumLibrary.Security.Cryptography.ECDiffieHellman(256, KeyAgreementKeyDerivationFunction.Hmac, KeyAgreementKeyDerivationHashAlgorithm.SHA256);
                    break;

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCryptoAvailable, _remotePeerEP, _remotePeerCert);
            }

            //send client key exchange data
            new SecureChannelHandshakeKeyExchange(keyAgreement.GetPublicKey(), _clientCredentials.PrivateKey, hashAlgo).WriteTo(bufferedStream);

            //generate master key
            byte[] masterKey = GenerateMasterKey(clientHello, serverHello, _preSharedKey, keyAgreement, serverKeyExchange.PublicKey);

            //verify master key using HMAC authentication
            {
                SecureChannelHandshakeAuthentication clientAuthentication = new SecureChannelHandshakeAuthentication(serverHello, masterKey);
                clientAuthentication.WriteTo(bufferedStream);
                bufferedStream.Flush();

                SecureChannelHandshakeAuthentication serverAuthentication = new SecureChannelHandshakeAuthentication(bufferedStream);
                if (!serverAuthentication.IsValid(clientHello, masterKey))
                    throw new SecureChannelException(SecureChannelCode.ProtocolAuthenticationFailed, _remotePeerEP, _remotePeerCert);
            }

            //enable channel encryption
            switch (encAlgo)
            {
                case SymmetricEncryptionAlgorithm.Rijndael:
                    //using MD5 for generating AES IV of 128bit block size
                    HashAlgorithm md5Hash = HashAlgorithm.Create("MD5");
                    byte[] eIV = md5Hash.ComputeHash(clientHello.Nonce.Number);
                    byte[] dIV = md5Hash.ComputeHash(serverHello.Nonce.Number);

                    //create encryption and decryption objects
                    SymmetricCryptoKey encryptionKey = new SymmetricCryptoKey(SymmetricEncryptionAlgorithm.Rijndael, masterKey, eIV, PaddingMode.None);
                    SymmetricCryptoKey decryptionKey = new SymmetricCryptoKey(SymmetricEncryptionAlgorithm.Rijndael, masterKey, dIV, PaddingMode.None);

                    //enable encryption
                    EnableEncryption(stream, encryptionKey, decryptionKey, new HMACSHA256(masterKey), new HMACSHA256(masterKey));
                    break;

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCryptoAvailable, _remotePeerEP, _remotePeerCert);
            }

            //channel encryption is ON!

            #endregion

            #region 3. exchange & verify certificates & signatures

            if (!IsReNegotiating())
            {
                //send client certificate
                new SecureChannelHandshakeCertificate(_clientCredentials.Certificate).WriteTo(this);
                this.Flush();

                //read server certificate
                _remotePeerCert = new SecureChannelHandshakeCertificate(this).Certificate;

                //verify server certificate
                try
                {
                    _remotePeerCert.Verify(_trustedRootCertificates);
                }
                catch (Exception ex)
                {
                    throw new SecureChannelException(SecureChannelCode.InvalidRemoteCertificate, _remotePeerEP, _remotePeerCert, "Invalid remote certificate.", ex);
                }
            }

            //verify key exchange signature
            switch (_selectedCryptoOption)
            {
                case SecureChannelCryptoOptionFlags.DHE2048_RSA_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCryptoOptionFlags.ECDHE256_RSA_WITH_AES256_CBC_HMAC_SHA256:
                    if (_remotePeerCert.PublicKeyEncryptionAlgorithm != AsymmetricEncryptionAlgorithm.RSA)
                        throw new SecureChannelException(SecureChannelCode.InvalidRemoteCertificateAlgorithm, _remotePeerEP, _remotePeerCert);

                    if (!serverKeyExchange.IsSignatureValid(_remotePeerCert, "SHA256"))
                        throw new SecureChannelException(SecureChannelCode.InvalidRemoteKeyExchangeSignature, _remotePeerEP, _remotePeerCert);

                    break;

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCryptoAvailable, _remotePeerEP, _remotePeerCert);
            }

            if ((_manager != null) && !_manager.ProceedConnection(_remotePeerCert))
                throw new SecureChannelException(SecureChannelCode.SecurityManagerDeclinedAccess, _remotePeerEP, _remotePeerCert, "Security manager declined access.");

            #endregion
        }

        #endregion

        #region overrides

        protected override void StartReNegotiation()
        {
            try
            {
                switch (_version)
                {
                    case 4:
                        ProtocolV4(_baseStream);
                        break;

                    default:
                        throw new SecureChannelException(SecureChannelCode.ProtocolVersionNotSupported, _remotePeerEP, _remotePeerCert, "SecureChannel protocol version '" + _version + "' not supported.");
                }
            }
            catch (SecureChannelException ex)
            {
                try
                {
                    new SecureChannelHandshakePacket(ex.Code).WriteTo(_baseStream);
                    _baseStream.Flush();
                }
                catch
                { }

                throw;
            }
        }

        #endregion
    }
}
