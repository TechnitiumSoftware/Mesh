/*
Technitium Ano
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

using System.IO;
using System.Net;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Security.Cryptography;

namespace AnoCore.Network.SecureChannel
{
    public class SecureChannelServerStream : SecureChannelStream
    {
        #region variables

        readonly SecureChannelCipherSuite _supportedCiphers;
        readonly SecureChannelOptions _options;
        readonly byte[] _preSharedKey;
        readonly byte[] _privateKey;

        #endregion

        #region constructor

        public SecureChannelServerStream(Stream stream, IPEndPoint remotePeerEP, string remotePeerAnoId, int reNegotiateOnBytesSent, int reNegotiateAfterSeconds, SecureChannelCipherSuite supportedCiphers, SecureChannelOptions options, byte[] preSharedKey, byte[] privateKey)
            : base(remotePeerEP, remotePeerAnoId, reNegotiateOnBytesSent, reNegotiateAfterSeconds)
        {
            _supportedCiphers = supportedCiphers;
            _options = options;
            _preSharedKey = preSharedKey;
            _privateKey = privateKey;

            Start(stream);
        }

        #endregion

        #region private

        private void Start(Stream stream)
        {
            try
            {
                WriteBufferedStream bufferedStream = new WriteBufferedStream(stream, 8 * 1024);

                //read client hello
                SecureChannelHandshakeHello clientHello = new SecureChannelHandshakeHello(bufferedStream);

                switch (clientHello.Version)
                {
                    case 5:
                        ProtocolV5(bufferedStream, clientHello);
                        break;

                    default:
                        throw new SecureChannelException(SecureChannelCode.ProtocolVersionNotSupported, _remotePeerEP, _remotePeerAnoId, "SecureChannel protocol version not supported: " + clientHello.Version);
                }
            }
            catch (SecureChannelException ex)
            {
                if (ex.Code == SecureChannelCode.RemoteError)
                {
                    throw new SecureChannelException(ex.Code, _remotePeerEP, _remotePeerAnoId, ex.Message, ex);
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

                    if (ex.PeerEP == null)
                        throw new SecureChannelException(ex.Code, _remotePeerEP, _remotePeerAnoId, ex.Message, ex);

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

        private void ProtocolV5(WriteBufferedStream bufferedStream, SecureChannelHandshakeHello clientHello)
        {
            #region 1. hello handshake check

            //select crypto option
            SecureChannelCipherSuite availableCiphers = _supportedCiphers & clientHello.SupportedCiphers;

            if (availableCiphers == SecureChannelCipherSuite.None)
            {
                throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, _remotePeerEP, _remotePeerAnoId);
            }
            else if (availableCiphers.HasFlag(SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256))
            {
                _selectedCipher = SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256;
            }
            else if (availableCiphers.HasFlag(SecureChannelCipherSuite.DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256))
            {
                _selectedCipher = SecureChannelCipherSuite.DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256;
            }
            else
            {
                throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, _remotePeerEP, _remotePeerAnoId);
            }

            //match options
            if (_options != clientHello.Options)
                throw new SecureChannelException(SecureChannelCode.NoMatchingOptionsAvailable, _remotePeerEP, _remotePeerAnoId);

            //write server hello
            SecureChannelHandshakeHello serverHello = new SecureChannelHandshakeHello(_selectedCipher, _options);
            serverHello.WriteTo(bufferedStream);

            #endregion

            #region 2. key exchange

            KeyAgreement keyAgreement;

            switch (_selectedCipher)
            {
                case SecureChannelCipherSuite.DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256:
                case SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                    keyAgreement = new DiffieHellman(DiffieHellmanGroupType.RFC3526, 2048, KeyAgreementKeyDerivationFunction.Hmac, KeyAgreementKeyDerivationHashAlgorithm.SHA256);
                    break;

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, _remotePeerEP, _remotePeerAnoId);
            }

            //send server key exchange data
            SecureChannelHandshakeKeyExchange serverKeyExchange = new SecureChannelHandshakeKeyExchange(keyAgreement, serverHello, clientHello, _preSharedKey);
            serverKeyExchange.WriteTo(bufferedStream);
            bufferedStream.Flush();

            //read client key exchange data
            SecureChannelHandshakeKeyExchange clientKeyExchange = new SecureChannelHandshakeKeyExchange(bufferedStream);

            if (_options.HasFlag(SecureChannelOptions.PRE_SHARED_KEY_AUTHENTICATION_REQUIRED))
            {
                if (!clientKeyExchange.IsPskAuthValid(serverHello, clientHello, _preSharedKey))
                    throw new SecureChannelException(SecureChannelCode.PskAuthenticationFailed, _remotePeerEP, _remotePeerAnoId);
            }

            #endregion

            #region 3. enable encryption

            EnableEncryption(bufferedStream, serverHello, clientHello, keyAgreement, clientKeyExchange);

            #endregion

            #region 4. AnoId based authentication

            switch (_selectedCipher)
            {
                case SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256:
                    if (_options.HasFlag(SecureChannelOptions.CLIENT_AUTHENTICATION_REQUIRED))
                    {
                        //read client auth
                        SecureChannelHandshakeAuthentication clientAuth = new SecureChannelHandshakeAuthentication(this);

                        //authenticate client
                        if (!clientAuth.IsPublicKeyValid(_remotePeerAnoId))
                            throw new SecureChannelException(SecureChannelCode.InvalidPeerPublicKey, _remotePeerEP, _remotePeerAnoId);

                        if (!clientAuth.IsSignatureValid(clientKeyExchange, serverHello, clientHello))
                            throw new SecureChannelException(SecureChannelCode.PeerAuthenticationFailed, _remotePeerEP, _remotePeerAnoId);
                    }

                    //write server auth
                    new SecureChannelHandshakeAuthentication(serverKeyExchange, serverHello, clientHello, _privateKey).WriteTo(this);
                    this.Flush();
                    break;

                case SecureChannelCipherSuite.DHE2048_ANON_WITH_AES256_CBC_HMAC_SHA256:
                    break; //no auth for ANON

                default:
                    throw new SecureChannelException(SecureChannelCode.NoMatchingCipherAvailable, _remotePeerEP, _remotePeerAnoId);
            }

            #endregion
        }

        #endregion

        #region overrides

        protected override void StartRenegotiation()
        {
            Start(_baseStream);
        }

        #endregion
    }
}
