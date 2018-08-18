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

using MeshCore.Network;
using MeshCore.Network.Connections;
using MeshCore.Network.SecureChannel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;
using TechnitiumLibrary.Net.Proxy;
using TechnitiumLibrary.Net.Tor;

namespace MeshCore
{
    public delegate void MeshNetworkInvitation(MeshNetwork network);

    public enum MeshNodeType : byte
    {
        Invalid = 0,
        P2P = 1,
        Anonymous = 2
    }

    public enum MeshProfileStatus : byte
    {
        None = 0,
        Active = 1,
        Inactive = 2,
        Busy = 3
    }

    public class MeshNode : IDisposable
    {
        #region events

        public event MeshNetworkInvitation InvitationReceived;

        #endregion

        #region variables

        static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        readonly SynchronizationContext _syncCxt = SynchronizationContext.Current;

        string _password;
        readonly string _profileFolder;

        MeshNodeType _type; //serialize
        byte[] _privateKey; //serialize
        SecureChannelCipherSuite _supportedCiphers; //serialize

        BinaryNumber _userId; //serialize
        BinaryNumber _maskedUserId;

        ushort _localServicePort; //serialize
        string _downloadFolder; //serialize

        DateTime _profileDateModified; //serialize
        string _profileDisplayName; //serialize
        MeshProfileStatus _profileStatus; //serialize
        string _profileStatusMessage = ""; //serialize

        DateTime _profileImageDateModified; //serialize
        byte[] _profileDisplayImage = new byte[] { }; //serialize

        EndPoint[] _ipv4BootstrapDhtNodes = new EndPoint[] { }; //serialize
        EndPoint[] _ipv6BootstrapDhtNodes = new EndPoint[] { }; //serialize
        EndPoint[] _torBootstrapDhtNodes = new EndPoint[] { }; //serialize

        bool _enableUPnP; //serialize
        bool _allowInboundInvitations; //serialize
        bool _allowOnlyLocalInboundInvitations; //serialize; this option works only when _allowInboundInvitations=true

        //proxy
        NetProxy _proxy;

        //UI settings data
        byte[] _appData = new byte[] { }; //serialize

        ConnectionManager _connectionManager;
        readonly Dictionary<BinaryNumber, MeshNetwork> _networks = new Dictionary<BinaryNumber, MeshNetwork>(); //serialize

        //user id DHT announce timer
        const int USER_ID_ANNOUNCE_INTERVAL = 60000;
        Timer _userIdAnnounceTimer;

        #endregion

        #region constructor

        static MeshNode()
        {
            //set min threads since the default value is too small for node at startup due to multiple networks queuing too many tasks immediately which block for a while
            {
                int minWorker = Environment.ProcessorCount * 64;
                int minIOC = Environment.ProcessorCount * 64;

                ThreadPool.SetMinThreads(minWorker, minIOC);
            }
        }

        public MeshNode(MeshNodeType type, byte[] privateKey, SecureChannelCipherSuite supportedCiphers, ushort localServicePort, string profileDisplayName, string profileFolder, string downloadFolder, TorController torController)
        {
            _type = type;
            _privateKey = privateKey;
            _supportedCiphers = supportedCiphers;
            _localServicePort = localServicePort;
            _profileDateModified = DateTime.UtcNow;
            _profileDisplayName = profileDisplayName;

            if (_type == MeshNodeType.Anonymous)
            {
                _enableUPnP = false;
                _allowInboundInvitations = true;
                _allowOnlyLocalInboundInvitations = true;
            }
            else
            {
                _enableUPnP = true;
                _allowInboundInvitations = true;
                _allowOnlyLocalInboundInvitations = false;
            }

            _profileFolder = profileFolder;
            _downloadFolder = downloadFolder;

            GenerateNewUserId();

            //start connection manager
            _connectionManager = new ConnectionManager(this, torController);

            InitAnnounceTimer();
        }

        public MeshNode(Stream s, string password, string profileFolder, TorController torController)
        {
            _password = password;
            _profileFolder = profileFolder;

            switch (s.ReadByte()) //version
            {
                case 1:
                    //read headers and init decryptor
                    Aes decryptionAlgo = Aes.Create();
                    decryptionAlgo.Key = MeshNetwork.GetKdfValue32(Encoding.UTF8.GetBytes(password), s.ReadBytes(32), 1, 1 * 1024 * 1024); //salt
                    decryptionAlgo.IV = s.ReadBytes(16); //IV
                    decryptionAlgo.Padding = PaddingMode.ISO10126;
                    decryptionAlgo.Mode = CipherMode.CBC;

                    byte[] hmac = s.ReadBytes(32); //hmac
                    long cipherTextStartPosition = s.Position;

                    //authenticate data in Encrypt-then-MAC (EtM) mode
                    using (HMAC aeHmac = new HMACSHA256(decryptionAlgo.Key))
                    {
                        byte[] computedHmac = aeHmac.ComputeHash(s);

                        if (!BinaryNumber.Equals(hmac, computedHmac))
                            throw new CryptographicException("Invalid password or data tampered.");
                    }

                    //decrypt data and init node
                    s.Position = cipherTextStartPosition;
                    CryptoStream cS = new CryptoStream(s, decryptionAlgo.CreateDecryptor(), CryptoStreamMode.Read);

                    InitMeshNode(new BinaryReader(cS), torController);
                    break;

                case -1:
                    throw new EndOfStreamException();

                default:
                    throw new InvalidDataException("MeshNode format version not supported.");
            }
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
            if (_disposed)
                return;

            if (disposing)
            {
                if (_userIdAnnounceTimer != null)
                    _userIdAnnounceTimer.Dispose();

                lock (_networks)
                {
                    foreach (KeyValuePair<BinaryNumber, MeshNetwork> network in _networks)
                    {
                        network.Value.Dispose();
                    }

                    _networks.Clear();
                }

                if (_connectionManager != null)
                    _connectionManager.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region private event functions

        private void RaiseEventInvitationReceived(MeshNetwork network)
        {
            _syncCxt.Send(delegate (object state)
            {
                InvitationReceived?.Invoke(network);
            }, null);
        }

        #endregion

        #region private

        private void InitAnnounceTimer()
        {
            _userIdAnnounceTimer = new Timer(delegate (object state)
            {
                BinaryNumber maskedUserId = _maskedUserId; //separate variable to mitigate possible rare race condition

                if (maskedUserId == null)
                {
                    maskedUserId = MeshNetwork.GetMaskedUserId(_userId);
                    _maskedUserId = maskedUserId;
                }

                _connectionManager.DhtManager.AnnounceAsync(maskedUserId, _allowOnlyLocalInboundInvitations, null);

                if (!_allowOnlyLocalInboundInvitations)
                    _connectionManager.TcpRelayClientRegisterHostedNetwork(_maskedUserId);
            }, null, Timeout.Infinite, Timeout.Infinite);

            if (_allowInboundInvitations)
                _userIdAnnounceTimer.Change(5000, USER_ID_ANNOUNCE_INTERVAL);
        }

        private void InitMeshNode(BinaryReader bR, TorController torController)
        {
            switch (bR.ReadByte()) //version
            {
                case 1:
                    _type = (MeshNodeType)bR.ReadByte();
                    _privateKey = bR.ReadBuffer();
                    _supportedCiphers = (SecureChannelCipherSuite)bR.ReadByte();

                    //
                    _userId = new BinaryNumber(bR.BaseStream);

                    //
                    _localServicePort = bR.ReadUInt16();
                    _downloadFolder = bR.ReadShortString();

                    //
                    _profileDateModified = bR.ReadDate();
                    _profileDisplayName = bR.ReadShortString();
                    _profileStatus = (MeshProfileStatus)bR.ReadByte();
                    _profileStatusMessage = bR.ReadShortString();

                    //
                    _profileImageDateModified = bR.ReadDate();
                    _profileDisplayImage = bR.ReadBuffer();

                    //
                    _ipv4BootstrapDhtNodes = new EndPoint[bR.ReadInt32()];
                    for (int i = 0; i < _ipv4BootstrapDhtNodes.Length; i++)
                        _ipv4BootstrapDhtNodes[i] = EndPointExtension.Parse(bR);

                    _ipv6BootstrapDhtNodes = new EndPoint[bR.ReadInt32()];
                    for (int i = 0; i < _ipv6BootstrapDhtNodes.Length; i++)
                        _ipv6BootstrapDhtNodes[i] = EndPointExtension.Parse(bR);

                    _torBootstrapDhtNodes = new EndPoint[bR.ReadInt32()];
                    for (int i = 0; i < _torBootstrapDhtNodes.Length; i++)
                        _torBootstrapDhtNodes[i] = EndPointExtension.Parse(bR);

                    //
                    _enableUPnP = bR.ReadBoolean();
                    _allowInboundInvitations = bR.ReadBoolean();
                    _allowOnlyLocalInboundInvitations = bR.ReadBoolean();

                    //
                    if (bR.ReadBoolean())
                        _proxy = new NetProxy((NetProxyType)bR.ReadByte(), bR.ReadShortString(), bR.ReadUInt16(), (bR.ReadBoolean() ? new NetworkCredential(bR.ReadShortString(), bR.ReadShortString()) : null));

                    //
                    _appData = bR.ReadBuffer();

                    //start connection manager
                    _connectionManager = new ConnectionManager(this, torController);

                    //
                    int networkCount = bR.ReadInt32();

                    for (int i = 0; i < networkCount; i++)
                    {
                        MeshNetwork network = new MeshNetwork(_connectionManager, bR);
                        _networks.Add(network.NetworkId, network);
                    }

                    InitAnnounceTimer();
                    break;

                default:
                    throw new InvalidDataException("MeshNode format version not supported.");
            }
        }

        #endregion

        #region internal

        internal void MeshNetworkRequest(Connection connection, BinaryNumber networkId, Stream channel)
        {
            MeshNetwork network = null;

            lock (_networks)
            {
                if (_networks.ContainsKey(networkId))
                    network = _networks[networkId];
            }

            if (network == null)
            {
                if (!networkId.Equals(_maskedUserId))
                {
                    //no network found
                    channel.Dispose();
                    return;
                }

                //private network connection invitation attempt
                if (!_allowInboundInvitations || (_allowOnlyLocalInboundInvitations && ((connection.RemotePeerEP.AddressFamily == AddressFamily.Unspecified) || !NetUtilities.IsPrivateIP((connection.RemotePeerEP as IPEndPoint).Address))))
                {
                    //not allowed
                    channel.Dispose();
                    return;
                }

                //accept invitation
                network = MeshNetwork.AcceptPrivateNetworkInvitation(_connectionManager, connection, channel);

                //add network
                lock (_networks)
                {
                    _networks.Add(network.NetworkId, network);
                }

                //notify UI
                RaiseEventInvitationReceived(network);
            }
            else
            {
                network.AcceptConnectionAndJoinNetwork(connection, channel);
            }
        }

        internal void ReceivedMeshNetworkPeersViaTcpRelay(Connection viaConnection, BinaryNumber channelId, List<EndPoint> peerEPs)
        {
            MeshNetwork foundNetwork = null;

            lock (_networks)
            {
                foreach (KeyValuePair<BinaryNumber, MeshNetwork> network in _networks)
                {
                    if (network.Key.Equals(channelId) || ((network.Value.Type == MeshNetworkType.Private) && network.Value.OtherPeer.MaskedPeerUserId.Equals(channelId)))
                    {
                        foundNetwork = network.Value;
                        break;
                    }
                }
            }

            if (foundNetwork != null)
                foundNetwork.TcpRelayClientReceivedPeers(viaConnection, peerEPs);
        }

        internal void MeshNetworkChanged(MeshNetwork network, BinaryNumber newNetworkId)
        {
            lock (_networks)
            {
                _networks.Add(newNetworkId, network);
                _networks.Remove(network.NetworkId);
            }
        }

        internal void RemoveMeshNetwork(MeshNetwork network)
        {
            lock (_networks)
            {
                _networks.Remove(network.NetworkId);
            }
        }

        internal void UpdateProfileWithoutTriggerUpdate(DateTime profileDateModified, string profileDisplayName, MeshProfileStatus profileStatus, string profileStatusMessage)
        {
            _profileDateModified = profileDateModified;
            _profileDisplayName = profileDisplayName;
            _profileStatus = profileStatus;
            _profileStatusMessage = profileStatusMessage;
        }

        internal void UpdateProfileDisplayImageWithoutTriggerUpdate(DateTime profileImageDateModified, byte[] profileDisplayImage)
        {
            _profileImageDateModified = profileImageDateModified;
            _profileDisplayImage = profileDisplayImage;
        }

        #endregion

        #region public

        public MeshNetwork CreatePrivateChat(BinaryNumber peerUserId, string peerName, bool localNetworkOnly, string invitationMessage)
        {
            //use random userId for each independent chat network for privacy reasons
            BinaryNumber randomUserId = SecureChannelStream.GenerateUserId(SecureChannelStream.GetPublicKeyFromPrivateKey(_privateKey));
            MeshNetwork network = new MeshNetwork(_connectionManager, randomUserId, peerUserId, peerName, localNetworkOnly, invitationMessage);

            lock (_networks)
            {
                _networks.Add(network.NetworkId, network);
            }

            return network;
        }

        public MeshNetwork CreateGroupChat(string networkName, string sharedSecret, bool localNetworkOnly)
        {
            //use random userId for each independent chat network for privacy reasons
            BinaryNumber randomUserId = SecureChannelStream.GenerateUserId(SecureChannelStream.GetPublicKeyFromPrivateKey(_privateKey));
            MeshNetwork network = new MeshNetwork(_connectionManager, randomUserId, networkName, sharedSecret, localNetworkOnly);

            lock (_networks)
            {
                if (_networks.ContainsKey(network.NetworkId))
                {
                    network.Dispose();
                    throw new MeshException("Mesh network for group chat '" + network.NetworkName + "' already exists.");
                }

                _networks.Add(network.NetworkId, network);
            }

            return network;
        }

        public MeshNetwork[] GetNetworks()
        {
            lock (_networks)
            {
                MeshNetwork[] networks = new MeshNetwork[_networks.Values.Count];
                _networks.Values.CopyTo(networks, 0);

                return networks;
            }
        }

        public void GenerateNewUserId()
        {
            _userId = SecureChannelStream.GenerateUserId(SecureChannelStream.GetPublicKeyFromPrivateKey(_privateKey));
            _maskedUserId = null; //will be generated auto in timer thread

            //trigger announce for new userId
            if (_userIdAnnounceTimer != null)
                _userIdAnnounceTimer.Change(1000, USER_ID_ANNOUNCE_INTERVAL);
        }

        public void UpdateProfile(string profileDisplayName, MeshProfileStatus profileStatus, string profileStatusMessage)
        {
            _profileDateModified = DateTime.UtcNow;
            _profileDisplayName = profileDisplayName;
            _profileStatus = profileStatus;
            _profileStatusMessage = profileStatusMessage;

            //trigger profile update broadcast messages and UI events
            lock (_networks)
            {
                foreach (KeyValuePair<BinaryNumber, MeshNetwork> network in _networks)
                {
                    network.Value.ProfileTriggerUpdate(false);
                }
            }
        }

        public void UpdateProfileDisplayImage(byte[] profileDisplayImage)
        {
            _profileImageDateModified = DateTime.UtcNow;
            _profileDisplayImage = profileDisplayImage;

            //trigger profile update broadcast messages and UI events
            lock (_networks)
            {
                foreach (KeyValuePair<BinaryNumber, MeshNetwork> network in _networks)
                {
                    network.Value.ProfileTriggerUpdate(true);
                }
            }
        }

        public void ConfigureProxy(NetProxyType proxyType, string proxyAddress, ushort proxyPort, NetworkCredential proxyCredentials)
        {
            if (_type == MeshNodeType.Anonymous)
                throw new NotSupportedException("Mesh tor profile does not support proxy configuration.");

            if (proxyType == NetProxyType.None)
                _proxy = null;
            else
                _proxy = new NetProxy(proxyType, proxyAddress, proxyPort, proxyCredentials);

            //update proxy for networks
            lock (_networks)
            {
                foreach (KeyValuePair<BinaryNumber, MeshNetwork> network in _networks)
                    network.Value.ProxyUpdated();
            }
        }

        public void DisableProxy()
        {
            if (_type == MeshNodeType.Anonymous)
                throw new NotSupportedException("Mesh tor profile does not support proxy configuration.");

            if (_proxy != null)
            {
                _proxy = null;

                //update proxy for networks
                lock (_networks)
                {
                    foreach (KeyValuePair<BinaryNumber, MeshNetwork> network in _networks)
                        network.Value.ProxyUpdated();
                }
            }
        }

        public void ReCheckConnectivity()
        {
            _connectionManager.ReCheckConnectivity();
        }

        public EndPoint[] GetIPv4DhtNodes()
        {
            return _connectionManager.DhtManager.GetIPv4DhtNodes();
        }

        public EndPoint[] GetIPv6DhtNodes()
        {
            return _connectionManager.DhtManager.GetIPv6DhtNodes();
        }

        public EndPoint[] GetTorDhtNodes()
        {
            return _connectionManager.DhtManager.GetTorDhtNodes();
        }

        public EndPoint[] GetLanDhtNodes()
        {
            return _connectionManager.DhtManager.GetLanDhtNodes();
        }

        public EndPoint[] GetIPv4TcpRelayNodes()
        {
            return _connectionManager.GetIPv4TcpRelayConnectionEndPoints();
        }

        public EndPoint[] GetIPv6TcpRelayNodes()
        {
            return _connectionManager.GetIPv6TcpRelayConnectionEndPoints();
        }

        public void WriteTo(BinaryWriter bW)
        {
            bW.Write((byte)1); //version

            //
            bW.Write((byte)_type);
            bW.WriteBuffer(_privateKey);
            bW.Write((byte)_supportedCiphers);

            //
            _userId.WriteTo(bW.BaseStream);

            //
            bW.Write(_localServicePort);
            bW.WriteShortString(_downloadFolder);

            //
            bW.Write(_profileDateModified);
            bW.WriteShortString(_profileDisplayName);
            bW.Write((byte)_profileStatus);
            bW.WriteShortString(_profileStatusMessage);

            //
            bW.Write(_profileImageDateModified);
            bW.WriteBuffer(_profileDisplayImage);

            //
            _ipv4BootstrapDhtNodes = _connectionManager.DhtManager.GetIPv4DhtNodes();
            bW.Write(_ipv4BootstrapDhtNodes.Length);
            foreach (EndPoint ep in _ipv4BootstrapDhtNodes)
                ep.WriteTo(bW);

            _ipv6BootstrapDhtNodes = _connectionManager.DhtManager.GetIPv6DhtNodes();
            bW.Write(_ipv6BootstrapDhtNodes.Length);
            foreach (EndPoint ep in _ipv6BootstrapDhtNodes)
                ep.WriteTo(bW);

            _torBootstrapDhtNodes = _connectionManager.DhtManager.GetTorDhtNodes();
            bW.Write(_torBootstrapDhtNodes.Length);
            foreach (EndPoint ep in _torBootstrapDhtNodes)
                ep.WriteTo(bW);

            //
            bW.Write(_enableUPnP);
            bW.Write(_allowInboundInvitations);
            bW.Write(_allowOnlyLocalInboundInvitations);

            //
            if (_proxy == null)
            {
                bW.Write(false);
            }
            else
            {
                bW.Write(true);
                bW.Write((byte)_proxy.Type);
                bW.WriteShortString(_proxy.Address);
                bW.Write(_proxy.Port);

                if (_proxy.Credential == null)
                {
                    bW.Write(false);
                }
                else
                {
                    bW.Write(true);
                    bW.WriteShortString(_proxy.Credential.UserName);
                    bW.WriteShortString(_proxy.Credential.Password);
                }
            }

            //
            bW.WriteBuffer(_appData);

            //
            lock (_networks)
            {
                bW.Write(_networks.Count);

                foreach (KeyValuePair<BinaryNumber, MeshNetwork> network in _networks)
                    network.Value.WriteTo(bW);
            }
        }

        public void ChangePassword(string password)
        {
            _password = password;
        }

        public void SaveTo(Stream s)
        {
            //generate salt for KDF
            byte[] salt = new byte[32];
            _rng.GetBytes(salt);

            //create encryptor
            Aes encryptionAlgo = Aes.Create();
            encryptionAlgo.Key = MeshNetwork.GetKdfValue32(Encoding.UTF8.GetBytes(_password), salt, 1, 1 * 1024 * 1024);
            encryptionAlgo.GenerateIV();
            encryptionAlgo.Padding = PaddingMode.ISO10126;
            encryptionAlgo.Mode = CipherMode.CBC;

            //write headers
            s.WriteByte((byte)1); //version
            s.Write(salt); //salt
            s.Write(encryptionAlgo.IV); //IV
            s.Write(new byte[32]); //placeholder for HMAC

            long cipherTextStartPosition = s.Position;

            //write encrypted data
            {
                CryptoStream cS = new CryptoStream(s, encryptionAlgo.CreateEncryptor(), CryptoStreamMode.Write);
                BufferedStream bS = new BufferedStream(cS);
                WriteTo(new BinaryWriter(bS));
                bS.Flush();
                cS.FlushFinalBlock();
            }

            //write hmac for authenticated encryption Encrypt-then-MAC (EtM) mode
            using (HMAC aeHmac = new HMACSHA256(encryptionAlgo.Key))
            {
                s.Position = cipherTextStartPosition;
                byte[] hmac = aeHmac.ComputeHash(s);

                //write hmac
                s.Position = cipherTextStartPosition - 32;
                s.Write(hmac);
            }
        }

        #endregion

        #region properties

        internal SynchronizationContext SynchronizationContext
        { get { return _syncCxt; } }

        public string ProfileFolder
        { get { return _profileFolder; } }

        public MeshNodeType Type
        { get { return _type; } }

        public BinaryNumber UserId
        { get { return _userId; } }

        public byte[] PrivateKey
        { get { return _privateKey; } }

        public SecureChannelCipherSuite SupportedCiphers
        { get { return _supportedCiphers; } }

        public ushort LocalServicePort
        {
            get { return _localServicePort; }
            set { _localServicePort = value; }
        }

        public string DownloadFolder
        {
            get { return _downloadFolder; }
            set { _downloadFolder = value; }
        }

        internal DateTime ProfileDateModified
        {
            get { return _profileDateModified; }
            set { _profileDateModified = value; }
        }

        public string ProfileDisplayName
        { get { return _profileDisplayName; } }

        public MeshProfileStatus ProfileStatus
        { get { return _profileStatus; } }

        public string ProfileStatusMessage
        { get { return _profileStatusMessage; } }

        internal DateTime ProfileDisplayImageDateModified
        {
            get { return _profileImageDateModified; }
            set { _profileImageDateModified = value; }
        }

        public byte[] ProfileDisplayImage
        { get { return _profileDisplayImage; } }

        public EndPoint[] IPv4BootstrapDhtNodes
        {
            get { return _ipv4BootstrapDhtNodes; }
            set { _ipv4BootstrapDhtNodes = value; }
        }

        public EndPoint[] IPv6BootstrapDhtNodes
        {
            get { return _ipv6BootstrapDhtNodes; }
            set { _ipv6BootstrapDhtNodes = value; }
        }

        public EndPoint[] TorBootstrapDhtNodes
        {
            get { return _torBootstrapDhtNodes; }
            set { _torBootstrapDhtNodes = value; }
        }

        public bool EnableUPnP
        {
            get { return _enableUPnP; }
            set { _enableUPnP = value; }
        }

        public bool AllowInboundInvitations
        {
            get { return _allowInboundInvitations; }
            set
            {
                _allowInboundInvitations = value;

                if (_allowInboundInvitations)
                    _userIdAnnounceTimer.Change(1000, USER_ID_ANNOUNCE_INTERVAL);
                else
                    _userIdAnnounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public bool AllowOnlyLocalInboundInvitations
        {
            get { return _allowOnlyLocalInboundInvitations; }
            set { _allowOnlyLocalInboundInvitations = value; }
        }

        public NetProxy Proxy
        {
            get
            {
                if (_type == MeshNodeType.Anonymous)
                    return null;

                return _proxy;
            }
        }

        public byte[] AppData
        {
            get { return _appData; }
            set { _appData = value; }
        }

        public int ActiveLocalServicePort
        { get { return _connectionManager.LocalServicePort; } }

        public BinaryNumber IPv4DhtNodeID
        { get { return _connectionManager.DhtManager.IPv4DhtNodeId; } }

        public BinaryNumber IPv6DhtNodeID
        { get { return _connectionManager.DhtManager.IPv6DhtNodeId; } }

        public BinaryNumber TorDhtNodeId
        { get { return _connectionManager.DhtManager.TorDhtNodeId; } }

        public int IPv4DhtTotalNodes
        { get { return _connectionManager.DhtManager.IPv4DhtTotalNodes; } }

        public int IPv6DhtTotalNodes
        { get { return _connectionManager.DhtManager.IPv6DhtTotalNodes; } }

        public int TorDhtTotalNodes
        { get { return _connectionManager.DhtManager.TorDhtTotalNodes; } }

        public int LanDhtTotalNodes
        { get { return _connectionManager.DhtManager.LanDhtTotalNodes; } }

        public InternetConnectivityStatus IPv4InternetStatus
        { get { return _connectionManager.IPv4InternetStatus; } }

        public InternetConnectivityStatus IPv6InternetStatus
        { get { return _connectionManager.IPv6InternetStatus; } }

        public bool IsTorRunning
        { get { return _connectionManager.IsTorRunning; } }

        public EndPoint TorHiddenEndPoint
        { get { return _connectionManager.TorHiddenEndPoint; } }

        public UPnPDeviceStatus UPnPStatus
        { get { return _connectionManager.UPnPStatus; } }

        public IPAddress UPnPDeviceIP
        { get { return _connectionManager.UPnPDeviceIP; } }

        public IPAddress UPnPExternalIP
        { get { return _connectionManager.UPnPExternalIP; } }

        public EndPoint IPv4ExternalEndPoint
        { get { return _connectionManager.IPv4ExternalEndPoint; } }

        public EndPoint IPv6ExternalEndPoint
        { get { return _connectionManager.IPv6ExternalEndPoint; } }

        #endregion
    }
}
