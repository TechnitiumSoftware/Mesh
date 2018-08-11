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

using MeshCore.Message;
using MeshCore.Network.Connections;
using MeshCore.Network.DHT;
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
using TechnitiumLibrary.Security.Cryptography;

namespace MeshCore.Network
{
    internal delegate void NetworkChanged(MeshNetwork network, BinaryNumber newNetworkId);

    public delegate void PeerNotification(MeshNetwork.Peer peer);
    public delegate void MessageNotification(MeshNetwork.Peer peer, MessageItem message);

    public enum MeshNetworkType : byte
    {
        Private = 1,
        Group = 2
    }

    public enum MeshNetworkStatus : byte
    {
        Offline = 1,
        Online = 2
    }

    /* PENDING-
     * file transfer - UI events pending
    */

    public class MeshNetwork : IDisposable
    {
        #region events

        public event PeerNotification PeerAdded;
        public event PeerNotification PeerTyping;
        public event PeerNotification GroupImageChanged;
        public event MessageNotification MessageReceived;
        public event MessageNotification MessageDeliveryNotification;

        #endregion

        #region variables

        const int MAX_MESSAGE_SIZE = 4 * 1024;
        const int DATA_STREAM_BUFFER_SIZE = 8 * 1024;

        const int RENEGOTIATE_AFTER_BYTES_SENT = 104857600; //100mb
        const int RENEGOTIATE_AFTER_SECONDS = 3600; //1hr

        static readonly byte[] USER_ID_MASK_INITIAL_SALT = new byte[] { 0x9B, 0x68, 0xA9, 0xAE, 0xDE, 0x04, 0x09, 0x2C, 0x18, 0xF1, 0xBF, 0x14, 0x8C, 0xC5, 0xEE, 0x08, 0x0D, 0x7A, 0x62, 0x7C, 0xD2, 0xB2, 0x4F, 0x1E, 0xFC, 0x28, 0x40, 0x6A, 0xDA, 0x18, 0x4A, 0xFE };
        static readonly byte[] NETWORK_SECRET_INITIAL_SALT = new byte[] { 0x28, 0x4B, 0xAC, 0x0D, 0x34, 0x58, 0xE4, 0x7C, 0x34, 0x0A, 0xA5, 0x4A, 0xF1, 0xC8, 0x21, 0xC5, 0x69, 0x4C, 0x98, 0x29, 0x77, 0xAE, 0xED, 0x93, 0xBF, 0xC6, 0x5E, 0x2D, 0x3D, 0xDF, 0xE4, 0x47 };

        readonly ConnectionManager _connectionManager;
        readonly MeshNetworkType _type; //serialize 
        readonly BinaryNumber _userId; //serialize 
        readonly string _networkName; //serialize - only for group chat
        string _sharedSecret; //serialize
        MeshNetworkStatus _status; //serialize

        BinaryNumber _networkId; //serialize - for loading performance reasons
        BinaryNumber _networkSecret; //serialize - for loading performance reasons

        //encrypted message store
        readonly string _messageStoreId; //serialize
        readonly byte[] _messageStoreKey; //serialize
        MessageStore _store;

        //feature to run the network on local LAN network only
        DateTime _localNetworkOnlyDateModified; //serialize
        bool _localNetworkOnly; //serialize

        //group display image
        DateTime _groupDisplayImageDateModified; //serialize
        byte[] _groupDisplayImage = new byte[] { }; //serialize

        //feature to disallow new users from joining group
        DateTime _groupLockNetworkDateModified; //serialize
        bool _groupLockNetwork; //serialize

        //feature to let ui know to mute notifications for this network
        bool _mute; //serialize

        Peer _selfPeer;
        Peer _otherPeer; //serialize; only for private chat
        Dictionary<BinaryNumber, Peer> _peers; //serialize; only for group chat
        ReaderWriterLockSlim _peersLock; //only for group chat

        //DHT announce timer
        const int DHT_ANNOUNCE_TIMER_INITIAL_INTERVAL = 5000;
        const int DHT_ANNOUNCE_TIMER_INTERVAL = 60000;
        Timer _dhtAnnounceTimer;
        bool _dhtAnnounceTimerIsRunning;
        readonly List<EndPoint> _dhtIPv4Peers = new List<EndPoint>();
        readonly List<EndPoint> _dhtIPv6Peers = new List<EndPoint>();
        readonly List<EndPoint> _dhtLanPeers = new List<EndPoint>();
        readonly List<EndPoint> _dhtTorPeers = new List<EndPoint>();
        DateTime _dhtLastUpdated;

        //tcp relay
        readonly List<EndPoint> _ipv4TcpRelayPeers = new List<EndPoint>();
        readonly List<EndPoint> _ipv6TcpRelayPeers = new List<EndPoint>();

        //ping keep-alive timer
        const int PING_TIMER_INTERVAL = 15000;
        Timer _pingTimer;

        #endregion

        #region constructor

        public MeshNetwork(ConnectionManager connectionManager, BinaryNumber userId, BinaryNumber peerUserId, string peerDisplayName, bool localNetworkOnly, string invitationMessage)
            : this(connectionManager, userId, peerUserId, peerDisplayName, localNetworkOnly, MeshNetworkStatus.Online, invitationMessage)
        { }

        private MeshNetwork(ConnectionManager connectionManager, BinaryNumber userId, BinaryNumber peerUserId, string peerDisplayName, bool localNetworkOnly, MeshNetworkStatus status, string invitationMessage)
        {
            _connectionManager = connectionManager;
            _type = MeshNetworkType.Private;
            _userId = userId;
            _sharedSecret = "";
            _status = status;
            _localNetworkOnlyDateModified = DateTime.UtcNow;
            _localNetworkOnly = localNetworkOnly;

            //generate id
            _networkId = GetPrivateNetworkId(_userId, peerUserId, _sharedSecret);
            _networkSecret = GetPrivateNetworkSecret(_userId, peerUserId, _sharedSecret);

            _messageStoreId = BinaryNumber.GenerateRandomNumber256().ToString();
            _messageStoreKey = BinaryNumber.GenerateRandomNumber256().Value;

            InitMeshNetwork(new MeshNetworkPeerInfo[] { new MeshNetworkPeerInfo(peerUserId, peerDisplayName, null) });

            //save invitation message to store
            if (invitationMessage != null)
            {
                MessageItem msg = new MessageItem(DateTime.UtcNow, _userId, new MessageRecipient[] { new MessageRecipient(peerUserId) }, MessageType.TextMessage, invitationMessage, null, null, 0, null);
                msg.WriteTo(_store);
            }
        }

        public MeshNetwork(ConnectionManager connectionManager, BinaryNumber userId, string networkName, string sharedSecret, bool localNetworkOnly)
        {
            _connectionManager = connectionManager;
            _type = MeshNetworkType.Group;
            _userId = userId;
            _networkName = networkName;
            _sharedSecret = sharedSecret;
            _status = MeshNetworkStatus.Online;
            _localNetworkOnlyDateModified = DateTime.UtcNow;
            _localNetworkOnly = localNetworkOnly;

            //generate ids
            _networkId = GetGroupNetworkId(_networkName, _sharedSecret);
            _networkSecret = GetGroupNetworkSecret(_networkName, _sharedSecret);

            _messageStoreId = BinaryNumber.GenerateRandomNumber256().ToString();
            _messageStoreKey = BinaryNumber.GenerateRandomNumber256().Value;

            InitMeshNetwork();
        }

        public MeshNetwork(ConnectionManager connectionManager, BinaryReader bR)
        {
            _connectionManager = connectionManager;

            //parse
            switch (bR.ReadByte()) //version
            {
                case 1:
                    _type = (MeshNetworkType)bR.ReadByte();
                    _userId = new BinaryNumber(bR.BaseStream);

                    if (_type == MeshNetworkType.Group)
                        _networkName = bR.ReadShortString();

                    _sharedSecret = bR.ReadShortString();
                    _status = (MeshNetworkStatus)bR.ReadByte();

                    //
                    _networkId = new BinaryNumber(bR.BaseStream);
                    _networkSecret = new BinaryNumber(bR.BaseStream);

                    //
                    _messageStoreId = bR.ReadShortString();
                    _messageStoreKey = bR.ReadBuffer();

                    //
                    _localNetworkOnlyDateModified = bR.ReadDate();
                    _localNetworkOnly = bR.ReadBoolean();

                    //
                    _groupDisplayImageDateModified = bR.ReadDate();
                    _groupDisplayImage = bR.ReadBuffer();

                    //
                    _groupLockNetworkDateModified = bR.ReadDate();
                    _groupLockNetwork = bR.ReadBoolean();

                    //
                    _mute = bR.ReadBoolean();

                    //known peers
                    MeshNetworkPeerInfo[] knownPeers;

                    if (_type == MeshNetworkType.Private)
                    {
                        knownPeers = new MeshNetworkPeerInfo[] { new MeshNetworkPeerInfo(bR) };
                    }
                    else
                    {
                        knownPeers = new MeshNetworkPeerInfo[bR.ReadByte()];

                        for (int i = 0; i < knownPeers.Length; i++)
                            knownPeers[i] = new MeshNetworkPeerInfo(bR);
                    }

                    InitMeshNetwork(knownPeers);
                    break;

                default:
                    throw new InvalidDataException("MeshNetwork format version not supported.");
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
            lock (this)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    //stop ping timer
                    if (_pingTimer != null)
                        _pingTimer.Dispose();

                    //stop peer search & announce timer
                    if (_dhtAnnounceTimer != null)
                        _dhtAnnounceTimer.Dispose();

                    //dispose all peers
                    if (_peersLock != null)
                    {
                        _peersLock.EnterWriteLock();
                        try
                        {
                            foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                                peer.Value.Dispose();

                            _peers.Clear();
                        }
                        finally
                        {
                            _peersLock.ExitWriteLock();
                        }

                        _peersLock.Dispose();
                    }

                    if (_selfPeer != null)
                        _selfPeer.Dispose();

                    if (_otherPeer != null)
                        _otherPeer.Dispose();

                    //close message store
                    if (_store != null)
                        _store.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion

        #region private event functions

        private void RaiseEventPeerAdded(Peer peer)
        {
            _connectionManager.Node.SynchronizationContext.Send(delegate (object state)
            {
                PeerAdded?.Invoke(peer);
            }, null);

            //send info message
            MessageItem msg = new MessageItem(peer.ProfileDisplayName + " joined chat");
            msg.WriteTo(_store);

            RaiseEventMessageReceived(peer, msg);
        }

        private void RaiseEventPeerTyping(Peer peer)
        {
            _connectionManager.Node.SynchronizationContext.Send(delegate (object state)
            {
                PeerTyping?.Invoke(peer);
            }, null);
        }

        private void RaiseEventGroupImageChanged(Peer peer)
        {
            _connectionManager.Node.SynchronizationContext.Send(delegate (object state)
            {
                GroupImageChanged?.Invoke(peer);
            }, null);

            //send info message
            MessageItem msg = new MessageItem(peer.ProfileDisplayName + " changed group image");
            msg.WriteTo(_store);

            RaiseEventMessageReceived(peer, msg);
        }

        private void RaiseEventMessageReceived(Peer peer, MessageItem message)
        {
            _connectionManager.Node.SynchronizationContext.Send(delegate (object state)
            {
                MessageReceived?.Invoke(peer, message);
            }, null);
        }

        private void RaiseEventMessageDeliveryNotification(Peer peer, MessageItem message)
        {
            _connectionManager.Node.SynchronizationContext.Send(delegate (object state)
            {
                MessageDeliveryNotification?.Invoke(peer, message);
            }, null);
        }

        #endregion

        #region static

        internal static BinaryNumber GetMaskedUserId(BinaryNumber userId)
        {
            using (HMAC hmac = new HMACSHA256(userId.Value))
            {
                return new BinaryNumber(hmac.ComputeHash(USER_ID_MASK_INITIAL_SALT));
            }
        }

        internal static byte[] GetKdfValue32(byte[] password, byte[] salt, int c = 1, int m = 1 * 1024 * 1024)
        {
            using (PBKDF2 kdf1 = PBKDF2.CreateHMACSHA256(password, salt, c))
            {
                using (PBKDF2 kdf2 = PBKDF2.CreateHMACSHA256(password, kdf1.GetBytes(m), c))
                {
                    return kdf2.GetBytes(32);
                }
            }
        }

        private static BinaryNumber GetPrivateNetworkId(BinaryNumber userId1, BinaryNumber userId2, string sharedSecret)
        {
            return new BinaryNumber(GetKdfValue32(Encoding.UTF8.GetBytes(sharedSecret ?? ""), (userId1 ^ userId2).Value));
        }

        private static BinaryNumber GetGroupNetworkId(string networkName, string sharedSecret)
        {
            return new BinaryNumber(GetKdfValue32(Encoding.UTF8.GetBytes(sharedSecret ?? ""), Encoding.UTF8.GetBytes(networkName.ToLower())));
        }

        private static BinaryNumber GetPrivateNetworkSecret(BinaryNumber userId1, BinaryNumber userId2, string sharedSecret)
        {
            using (HMAC hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedSecret ?? "")))
            {
                return new BinaryNumber(GetKdfValue32(hmac.ComputeHash(NETWORK_SECRET_INITIAL_SALT), (userId1 ^ userId2).Value));
            }
        }

        private static BinaryNumber GetGroupNetworkSecret(string networkName, string sharedSecret)
        {
            using (HMAC hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedSecret ?? "")))
            {
                return new BinaryNumber(GetKdfValue32(hmac.ComputeHash(NETWORK_SECRET_INITIAL_SALT), Encoding.UTF8.GetBytes(networkName.ToLower())));
            }
        }

        internal static MeshNetwork AcceptPrivateNetworkInvitation(ConnectionManager connectionManager, Connection connection, Stream channel)
        {
            //establish secure channel with untrusted client using psk as userId expecting the opposite side to know the userId
            using (SecureChannelStream secureChannel = new SecureChannelServerStream(channel, connection.RemotePeerEP, connection.ViaRemotePeerEP, RENEGOTIATE_AFTER_BYTES_SENT, RENEGOTIATE_AFTER_SECONDS, connectionManager.Node.SupportedCiphers, SecureChannelOptions.PRE_SHARED_KEY_AUTHENTICATION_REQUIRED | SecureChannelOptions.CLIENT_AUTHENTICATION_REQUIRED, connectionManager.Node.UserId.Value, connectionManager.Node.UserId, connectionManager.Node.PrivateKey, null))
            {
                //recv invitation text message
                MeshNetworkPacketMessage invitationMessage = MeshNetworkPacket.Parse(new BinaryReader(secureChannel)) as MeshNetworkPacketMessage;
                if (invitationMessage == null)
                    throw new MeshException("Invalid message received: expected invitation text message.");

                //create new private network with offline status
                MeshNetwork privateNetwork = new MeshNetwork(connectionManager, connectionManager.Node.UserId, secureChannel.RemotePeerUserId, secureChannel.RemotePeerUserId.ToString(), false, MeshNetworkStatus.Offline, null);

                //store the invitation message in network store
                (new MessageItem("This private chat invitation was sent by " + connection.RemotePeerEP.ToString() + ".")).WriteTo(privateNetwork._store);
                (new MessageItem(secureChannel.RemotePeerUserId, invitationMessage)).WriteTo(privateNetwork._store);
                (new MessageItem("To accept this invitation, uncheck the 'Go Offline' option from the chat list context menu.")).WriteTo(privateNetwork._store);

                //send delivery notification
                new MeshNetworkPacketMessageDeliveryNotification(invitationMessage.MessageNumber).WriteTo(new BinaryWriter(secureChannel));
                secureChannel.Flush();

                return privateNetwork;
            }
        }

        #endregion

        #region private / internal

        private void InitMeshNetwork(MeshNetworkPeerInfo[] knownPeers = null)
        {
            //init message store
            string messageStoreFolder = Path.Combine(_connectionManager.Node.ProfileFolder, "messages");
            if (!Directory.Exists(messageStoreFolder))
                Directory.CreateDirectory(messageStoreFolder);

            _store = new MessageStore(new FileStream(Path.Combine(messageStoreFolder, _messageStoreId + ".index"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), new FileStream(Path.Combine(messageStoreFolder, _messageStoreId + ".data"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), _messageStoreKey);

            //load self as peer
            _selfPeer = new Peer(this, _userId, _connectionManager.Node.ProfileDisplayName);

            if (_type == MeshNetworkType.Private)
            {
                //load other peer
                _otherPeer = new Peer(this, knownPeers[0].PeerUserId, knownPeers[0].PeerDisplayName);

                if ((_status == MeshNetworkStatus.Online) && (knownPeers[0].PeerEPs != null))
                {
                    foreach (EndPoint peerEP in knownPeers[0].PeerEPs)
                        BeginMakeConnection(peerEP);
                }
            }
            else
            {
                _peers = new Dictionary<BinaryNumber, Peer>();
                _peersLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

                //add self
                _peers.Add(_userId, _selfPeer);

                //load known peers
                if (knownPeers != null)
                {
                    foreach (MeshNetworkPeerInfo knownPeer in knownPeers)
                    {
                        _peers.Add(knownPeer.PeerUserId, new Peer(this, knownPeer.PeerUserId, knownPeer.PeerDisplayName));

                        if ((_status == MeshNetworkStatus.Online) && (knownPeer.PeerEPs != null))
                        {
                            foreach (EndPoint peerEP in knownPeer.PeerEPs)
                                BeginMakeConnection(peerEP);
                        }
                    }
                }
            }

            //init timers
            _dhtAnnounceTimer = new Timer(DhtAnnounceAsync, null, Timeout.Infinite, Timeout.Infinite);
            _pingTimer = new Timer(PingAsync, null, Timeout.Infinite, Timeout.Infinite);

            if (_status == MeshNetworkStatus.Online)
            {
                //start ping timer
                _pingTimer.Change(PING_TIMER_INTERVAL, PING_TIMER_INTERVAL);

                //start dht announce
                _dhtAnnounceTimer.Change(DHT_ANNOUNCE_TIMER_INITIAL_INTERVAL, DHT_ANNOUNCE_TIMER_INTERVAL);
                _dhtAnnounceTimerIsRunning = true;

                //connectivity status update
                UpdateConnectivityStatus();
            }
        }

        private void DhtAnnounceAsync(object state)
        {
            try
            {
                lock (_dhtLanPeers)
                {
                    _dhtLanPeers.Clear();
                }

                if ((_type == MeshNetworkType.Private) && IsInvitationPending())
                {
                    //find other peer via its masked user id to send invitation
                    _connectionManager.DhtManager.BeginFindPeers(_otherPeer.MaskedPeerUserId, _localNetworkOnly, DhtCallback);
                }
                else
                {
                    _connectionManager.DhtManager.BeginAnnounce(_networkId, _localNetworkOnly, new IPEndPoint(IPAddress.Any, _connectionManager.LocalPort), DhtCallback);

                    if (!_localNetworkOnly)
                    {
                        //register network on tcp relays. tcp relays will auto announce network over DHT and register their network end point
                        _connectionManager.TcpRelayClientRegisterHostedNetwork(_networkId);
                    }
                }

                _dhtLastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);
            }
        }

        private void DhtCallback(DhtNetworkType networkType, ICollection<EndPoint> peerEPs)
        {
            if (peerEPs.Count > 0)
            {
                switch (networkType)
                {
                    case DhtNetworkType.IPv4Internet:
                        lock (_dhtIPv4Peers)
                        {
                            _dhtIPv4Peers.Clear();

                            foreach (EndPoint peerEP in peerEPs)
                            {
                                _dhtIPv4Peers.Add(peerEP);
                                BeginMakeConnection(peerEP);
                            }
                        }
                        break;

                    case DhtNetworkType.IPv6Internet:
                        lock (_dhtIPv6Peers)
                        {
                            _dhtIPv6Peers.Clear();

                            foreach (EndPoint peerEP in peerEPs)
                            {
                                _dhtIPv6Peers.Add(peerEP);
                                BeginMakeConnection(peerEP);
                            }
                        }
                        break;

                    case DhtNetworkType.LocalNetwork:
                        lock (_dhtLanPeers)
                        {
                            foreach (EndPoint peerEP in peerEPs)
                            {
                                _dhtLanPeers.Add(peerEP);
                                BeginMakeConnection(peerEP);
                            }
                        }
                        break;

                    case DhtNetworkType.TorNetwork:
                        lock (_dhtTorPeers)
                        {
                            _dhtTorPeers.Clear();

                            foreach (EndPoint peerEP in peerEPs)
                            {
                                _dhtTorPeers.Add(peerEP);
                                BeginMakeConnection(peerEP);
                            }
                        }
                        break;
                }
            }
        }

        internal void TcpRelayClientReceivedPeers(Connection viaConnection, List<EndPoint> peerEPs)
        {
            Debug.Write(this.GetType().Name, "Tcp relay received network [" + _networkId + "] peers via " + viaConnection.RemotePeerId + " [" + viaConnection.RemotePeerEP + "]");

            lock (_ipv4TcpRelayPeers)
            {
                lock (_ipv6TcpRelayPeers)
                {
                    foreach (EndPoint peerEP in peerEPs)
                    {
                        switch (peerEP.AddressFamily)
                        {
                            case AddressFamily.InterNetwork:
                                if (!_ipv4TcpRelayPeers.Contains(peerEP))
                                    _ipv4TcpRelayPeers.Add(peerEP);
                                break;

                            case AddressFamily.InterNetworkV6:
                                if (!_ipv6TcpRelayPeers.Contains(peerEP))
                                    _ipv6TcpRelayPeers.Add(peerEP);
                                break;
                        }


                        BeginMakeVirtualConnection(peerEP, viaConnection);
                    }
                }
            }
        }

        private void PingAsync(object state)
        {
            try
            {
                SendMessageBroadcast(new MeshNetworkPacket(MeshNetworkPacketType.PingRequest));
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);
            }
        }

        internal void BeginMakeConnection(EndPoint peerEP, Connection fallbackViaConnection = null)
        {
            if (_status == MeshNetworkStatus.Offline)
                return;

            if (_localNetworkOnly && ((peerEP.AddressFamily == AddressFamily.Unspecified) || !NetUtilities.IsPrivateIP((peerEP as IPEndPoint).Address)))
                return;

            if (IsPeerConnected(peerEP))
                return;

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                Connection connection;

                try
                {
                    connection = _connectionManager.MakeConnection(peerEP);
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);

                    if ((fallbackViaConnection == null) || fallbackViaConnection.IsVirtualConnection)
                        return;

                    try
                    {
                        connection = _connectionManager.MakeVirtualConnection(fallbackViaConnection, peerEP); //make virtual connection
                    }
                    catch (Exception ex2)
                    {
                        Debug.Write(this.GetType().Name, ex2);

                        return;
                    }
                }

                try
                {
                    EstablishSecureChannelAndJoinNetwork(connection);
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);
                }
            });
        }

        private void BeginMakeVirtualConnection(EndPoint peerEP, Connection viaConnection)
        {
            if (_status == MeshNetworkStatus.Offline)
                return;

            if (_localNetworkOnly && ((peerEP.AddressFamily == AddressFamily.Unspecified) || !NetUtilities.IsPrivateIP((peerEP as IPEndPoint).Address)))
                return;

            if (IsPeerConnected(peerEP))
                return;

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                Connection connection;

                try
                {
                    connection = _connectionManager.MakeVirtualConnection(viaConnection, peerEP); //make virtual connection
                }
                catch (Exception ex2)
                {
                    Debug.Write(this.GetType().Name, ex2);

                    return;
                }

                try
                {
                    EstablishSecureChannelAndJoinNetwork(connection);
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);
                }
            });
        }

        private void EstablishSecureChannelAndJoinNetwork(Connection connection)
        {
            bool isInvitationPending = false;
            byte[] psk;
            BinaryNumber channelId;
            ICollection<BinaryNumber> trustedUserIds;

            switch (_type)
            {
                case MeshNetworkType.Private:
                    isInvitationPending = IsInvitationPending();

                    if (isInvitationPending)
                    {
                        psk = _otherPeer.PeerUserId.Value;
                        channelId = _otherPeer.MaskedPeerUserId;
                    }
                    else
                    {
                        psk = _networkSecret.Value;
                        channelId = _networkId;
                    }

                    trustedUserIds = new BinaryNumber[] { _otherPeer.PeerUserId };
                    break;

                case MeshNetworkType.Group:
                    psk = _networkSecret.Value;
                    channelId = _networkId;

                    if (_groupLockNetwork)
                        trustedUserIds = GetKnownPeerUserIdList();
                    else
                        trustedUserIds = null;

                    break;

                default:
                    throw new NotSupportedException();
            }

            //check if channel exists
            if (connection.ChannelExists(channelId))
                return;

            //request channel
            Stream channel = connection.ConnectMeshNetwork(channelId);

            try
            {
                //establish secure channel
                SecureChannelStream secureChannel = new SecureChannelClientStream(channel, connection.RemotePeerEP, connection.ViaRemotePeerEP, RENEGOTIATE_AFTER_BYTES_SENT, RENEGOTIATE_AFTER_SECONDS, _connectionManager.Node.SupportedCiphers, SecureChannelOptions.PRE_SHARED_KEY_AUTHENTICATION_REQUIRED | SecureChannelOptions.CLIENT_AUTHENTICATION_REQUIRED, psk, _userId, _connectionManager.Node.PrivateKey, trustedUserIds);

                if (isInvitationPending)
                {
                    //send invitation message
                    MessageItem invitationMessage = GetPendingInvitationMessage();
                    invitationMessage.GetMeshNetworkPacket().WriteTo(new BinaryWriter(secureChannel));
                    secureChannel.Flush();

                    //read delivery notification message
                    MeshNetworkPacketMessageDeliveryNotification notification = MeshNetworkPacket.Parse(new BinaryReader(secureChannel)) as MeshNetworkPacketMessageDeliveryNotification;

                    foreach (MessageRecipient rcpt in invitationMessage.Recipients)
                    {
                        if (rcpt.UserId.Equals(_otherPeer.PeerUserId))
                        {
                            rcpt.SetDeliveredStatus();
                            break;
                        }
                    }

                    //update message to store
                    invitationMessage.WriteTo(_store);

                    //notify ui
                    RaiseEventMessageDeliveryNotification(_otherPeer, invitationMessage);

                    //close channel
                    secureChannel.Dispose();
                }
                else
                {
                    //join network
                    JoinNetwork(secureChannel, connection);
                }
            }
            catch (SecureChannelException ex)
            {
                SendSecureChannelFailedInfoMessage(ex);

                channel.Dispose();
                throw;
            }
            catch
            {
                channel.Dispose();
                throw;
            }
        }

        internal void AcceptConnectionAndJoinNetwork(Connection connection, Stream channel)
        {
            try
            {
                if (_localNetworkOnly && ((connection.RemotePeerEP.AddressFamily == AddressFamily.Unspecified) || !NetUtilities.IsPrivateIP((connection.RemotePeerEP as IPEndPoint).Address)))
                {
                    channel.Dispose();
                    return;
                }

                //create secure channel
                ICollection<BinaryNumber> trustedUserIds;

                switch (_type)
                {
                    case MeshNetworkType.Private:
                        trustedUserIds = new BinaryNumber[] { _otherPeer.PeerUserId };
                        break;

                    case MeshNetworkType.Group:
                        if (_groupLockNetwork)
                            trustedUserIds = GetKnownPeerUserIdList();
                        else
                            trustedUserIds = null;

                        break;

                    default:
                        throw new InvalidOperationException("Invalid network type.");
                }

                SecureChannelStream secureChannel = new SecureChannelServerStream(channel, connection.RemotePeerEP, connection.ViaRemotePeerEP, RENEGOTIATE_AFTER_BYTES_SENT, RENEGOTIATE_AFTER_SECONDS, _connectionManager.Node.SupportedCiphers, SecureChannelOptions.PRE_SHARED_KEY_AUTHENTICATION_REQUIRED | SecureChannelOptions.CLIENT_AUTHENTICATION_REQUIRED, _networkSecret.Value, _userId, _connectionManager.Node.PrivateKey, trustedUserIds);

                //join network
                JoinNetwork(secureChannel, connection);
            }
            catch (SecureChannelException ex)
            {
                channel.Dispose();

                SendSecureChannelFailedInfoMessage(ex);
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);

                channel.Dispose();
            }
        }

        private void JoinNetwork(SecureChannelStream channel, Connection connection)
        {
            if (_status == MeshNetworkStatus.Offline)
                throw new MeshException("Mesh network is offline.");

            Peer peer;

            if (_type == MeshNetworkType.Private)
            {
                if (channel.RemotePeerUserId.Equals(_otherPeer.PeerUserId))
                    peer = _otherPeer;
                else if (channel.RemotePeerUserId.Equals(_userId))
                    peer = _selfPeer;
                else
                    throw new InvalidOperationException();
            }
            else
            {
                bool peerAdded = false;

                _peersLock.EnterWriteLock();
                try
                {
                    BinaryNumber peerUserId = channel.RemotePeerUserId;

                    if (_peers.ContainsKey(peerUserId))
                    {
                        peer = _peers[peerUserId];
                    }
                    else
                    {
                        peer = new Peer(this, peerUserId, peerUserId.ToString());
                        _peers.Add(peerUserId, peer);

                        peerAdded = true;
                    }
                }
                finally
                {
                    _peersLock.ExitWriteLock();
                }

                if (peerAdded)
                    RaiseEventPeerAdded(peer);
            }

            peer.AddSession(channel, connection);
        }

        private bool IsPeerConnected(EndPoint peerEP)
        {
            //check if peer already connected

            if (_type == MeshNetworkType.Private)
            {
                return _otherPeer.IsOnline && _otherPeer.IsConnectedVia(peerEP);
            }
            else
            {
                _peersLock.EnterReadLock();
                try
                {
                    foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                    {
                        if (peer.Value.IsOnline && peer.Value.IsConnectedVia(peerEP))
                            return true;
                    }
                }
                finally
                {
                    _peersLock.ExitReadLock();
                }

                return false;
            }
        }

        private ICollection<BinaryNumber> GetKnownPeerUserIdList()
        {
            if (_type == MeshNetworkType.Private)
            {
                return new BinaryNumber[] { _userId, _otherPeer.PeerUserId };
            }
            else
            {
                List<BinaryNumber> peerUserIdList;

                _peersLock.EnterReadLock();
                try
                {
                    peerUserIdList = new List<BinaryNumber>(_peers.Count);

                    foreach (KeyValuePair<BinaryNumber, Peer> item in _peers)
                        peerUserIdList.Add(item.Value.PeerUserId);
                }
                finally
                {
                    _peersLock.ExitReadLock();
                }

                return peerUserIdList;
            }
        }

        private void SendMessageBroadcast(byte[] data, int offset, int count)
        {
            if (_type == MeshNetworkType.Private)
            {
                _selfPeer.SendMessage(data, offset, count);
                _otherPeer.SendMessage(data, offset, count);
            }
            else
            {
                _peersLock.EnterReadLock();
                try
                {
                    foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                    {
                        if (peer.Value.IsOnline)
                            peer.Value.SendMessage(data, offset, count);
                    }
                }
                finally
                {
                    _peersLock.ExitReadLock();
                }
            }
        }

        private void SendMessageBroadcast(MeshNetworkPacket message)
        {
            using (MemoryStream mS = new MemoryStream())
            {
                message.WriteTo(new BinaryWriter(mS));

                byte[] buffer = mS.ToArray();
                SendMessageBroadcast(buffer, 0, buffer.Length);
            }
        }

        private void DoPeerExchange()
        {
            SendMessageBroadcast(new MeshNetworkPacketPeerExchange(_selfPeer.GetConnectedPeerList()));
        }

        private void UpdateConnectivityStatus()
        {
            lock (this)
            {
                List<MeshNetworkPeerInfo> uniquePeerInfoList = new List<MeshNetworkPeerInfo>();

                if (_type == MeshNetworkType.Private)
                {
                    //get each peer unique connections
                    uniquePeerInfoList.AddRange(_selfPeer.GetConnectedPeerList());

                    if (_otherPeer.IsOnline)
                    {
                        foreach (MeshNetworkPeerInfo info in _otherPeer.GetConnectedPeerList())
                        {
                            if (!uniquePeerInfoList.Contains(info))
                                uniquePeerInfoList.Add(info);
                        }
                    }

                    //update each peer connectivity status
                    _selfPeer.UpdateConnectivityStatus(uniquePeerInfoList);
                    _otherPeer.UpdateConnectivityStatus(uniquePeerInfoList);
                }
                else
                {
                    _peersLock.EnterReadLock();
                    try
                    {
                        //get each peer unique connections
                        foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                        {
                            if (peer.Value.IsOnline)
                            {
                                List<MeshNetworkPeerInfo> peerConnectedPeerInfo = peer.Value.GetConnectedPeerList();

                                foreach (MeshNetworkPeerInfo info in peerConnectedPeerInfo)
                                {
                                    if (!uniquePeerInfoList.Contains(info))
                                        uniquePeerInfoList.Add(info);
                                }
                            }
                        }

                        //update each peer connectivity status
                        foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                            peer.Value.UpdateConnectivityStatus(uniquePeerInfoList);
                    }
                    finally
                    {
                        _peersLock.ExitReadLock();
                    }
                }
            }
        }

        internal void ProfileTriggerUpdate(bool profileImageUpdated)
        {
            _selfPeer.RaiseEventProfileChanged();

            MeshNode node = _connectionManager.Node;

            if (profileImageUpdated)
                SendMessageBroadcast(new MeshNetworkPacketProfileDisplayImage(node.ProfileDisplayImageDateModified, node.ProfileDisplayImage));
            else
                SendMessageBroadcast(new MeshNetworkPacketProfile(node.ProfileDateModified, node.ProfileDisplayName, node.ProfileStatus, node.ProfileStatusMessage));
        }

        internal void ProxyUpdated()
        {
            GoOffline();
            GoOnline();
        }

        private bool IsInvitationPending()
        {
            int totalMessages = _store.GetMessageCount();
            if (totalMessages > 0)
            {
                MessageItem msg = new MessageItem(_store, 0);

                if ((msg.Type == MessageType.TextMessage) && msg.SenderUserId.Equals(_userId))
                {
                    if (msg.GetDeliveryStatus() != MessageDeliveryStatus.Delivered)
                        return true;
                }
            }

            return false;
        }

        private MessageItem GetPendingInvitationMessage()
        {
            MessageItem msg = new MessageItem(_store, 0);

            if ((msg.Type == MessageType.TextMessage) && msg.SenderUserId.Equals(_userId))
            {
                if (msg.GetDeliveryStatus() != MessageDeliveryStatus.Delivered)
                    return msg;
            }

            throw new MeshException("Pending invitation message not found.");
        }

        private MessageRecipient[] GetMessageRecipients()
        {
            MessageRecipient[] msgRcpt;

            if (_type == MeshNetworkType.Private)
            {
                msgRcpt = new MessageRecipient[] { new MessageRecipient(_otherPeer.PeerUserId) };
            }
            else
            {
                _peersLock.EnterReadLock();
                try
                {
                    if (_peers.Count > 1)
                    {
                        msgRcpt = new MessageRecipient[_peers.Count - 1];
                        int i = 0;

                        foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                        {
                            if (!peer.Value.IsSelfPeer)
                                msgRcpt[i++] = new MessageRecipient(peer.Value.PeerUserId);
                        }
                    }
                    else
                    {
                        msgRcpt = new MessageRecipient[] { };
                    }
                }
                finally
                {
                    _peersLock.ExitReadLock();
                }
            }

            return msgRcpt;
        }

        private void SendSecureChannelFailedInfoMessage(SecureChannelException ex)
        {
            //send info message
            string peerUserId = "";

            if (ex.PeerUserId != null)
                peerUserId = "<" + ex.PeerUserId.ToString() + "> ";

            string message = "Secure channel with peer '" + peerUserId + "[" + ex.PeerEP.ToString() + "]' encountered '" + ex.Code.ToString() + "' exception.";

            if (ex.InnerException != null)
                message += " Inner exception: " + ex.InnerException.ToString();

            MessageItem msg = new MessageItem(message);
            msg.WriteTo(_store);

            RaiseEventMessageReceived(_selfPeer, msg);
        }

        #endregion

        #region public

        public void GoOnline()
        {
            if (_status != MeshNetworkStatus.Online)
            {
                //start ping timer
                _pingTimer.Change(PING_TIMER_INTERVAL, PING_TIMER_INTERVAL);

                //start dht announce
                _dhtAnnounceTimer.Change(DHT_ANNOUNCE_TIMER_INITIAL_INTERVAL, DHT_ANNOUNCE_TIMER_INTERVAL);
                _dhtAnnounceTimerIsRunning = true;

                _status = MeshNetworkStatus.Online;

                //async to avoid blocking UI thread
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    //update self status
                    _selfPeer.RaiseEventStateChanged();

                    //connectivity status update
                    UpdateConnectivityStatus();
                });
            }
        }

        public void GoOffline()
        {
            if (_status != MeshNetworkStatus.Offline)
            {
                //stop ping timer
                _pingTimer.Change(Timeout.Infinite, Timeout.Infinite);

                //stop dht announce
                _dhtAnnounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _dhtAnnounceTimerIsRunning = false;

                _status = MeshNetworkStatus.Offline;

                //async to avoid blocking UI thread
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    //disconnect all peers
                    if (_type == MeshNetworkType.Private)
                    {
                        _selfPeer.Disconnect();
                        _otherPeer.Disconnect();
                    }
                    else
                    {
                        _peersLock.EnterReadLock();
                        try
                        {
                            foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                                peer.Value.Disconnect();
                        }
                        finally
                        {
                            _peersLock.ExitReadLock();
                        }
                    }

                    //update self status
                    _selfPeer.RaiseEventStateChanged();

                    //connectivity status update
                    UpdateConnectivityStatus();

                    if (!_localNetworkOnly)
                    {
                        lock (_ipv4TcpRelayPeers)
                        {
                            _ipv4TcpRelayPeers.Clear();
                        }

                        lock (_ipv6TcpRelayPeers)
                        {
                            _ipv6TcpRelayPeers.Clear();
                        }

                        //unregister network from tcp relays
                        _connectionManager.TcpRelayClientUnregisterHostedNetwork(_networkId);
                    }
                });
            }
        }

        public Peer[] GetPeers()
        {
            if (_type == MeshNetworkType.Private)
            {
                return new Peer[] { _selfPeer, _otherPeer };
            }
            else
            {
                Peer[] peers;

                _peersLock.EnterReadLock();
                try
                {
                    peers = new Peer[_peers.Count];
                    _peers.Values.CopyTo(peers, 0);
                }
                finally
                {
                    _peersLock.ExitReadLock();
                }

                return peers;
            }
        }

        public void SendTypingNotification()
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                SendMessageBroadcast(new MeshNetworkPacket(MeshNetworkPacketType.MessageTypingNotification));
            });
        }

        public void SendTextMessage(string message)
        {
            if (message.Length > MAX_MESSAGE_SIZE)
                throw new IOException("MeshNetwork message data size cannot exceed " + MAX_MESSAGE_SIZE + " bytes.");

            MessageRecipient[] msgRcpt = GetMessageRecipients();

            MessageItem msg = new MessageItem(DateTime.UtcNow, _userId, msgRcpt, MessageType.TextMessage, message, null, null, 0, null);
            msg.WriteTo(_store);

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                SendMessageBroadcast(msg.GetMeshNetworkPacket());

                RaiseEventMessageReceived(_selfPeer, msg);
            });
        }

        public void SendInlineImage(string message, string filePath, byte[] imageThumbnail)
        {
            if (message.Length > MAX_MESSAGE_SIZE)
                throw new IOException("MeshNetwork message data size cannot exceed " + MAX_MESSAGE_SIZE + " bytes.");

            MessageRecipient[] msgRcpt = GetMessageRecipients();

            MessageItem msg = new MessageItem(DateTime.UtcNow, _userId, msgRcpt, MessageType.InlineImage, message, imageThumbnail, Path.GetFileName(filePath), (new FileInfo(filePath)).Length, filePath);
            msg.WriteTo(_store);

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                SendMessageBroadcast(msg.GetMeshNetworkPacket());

                RaiseEventMessageReceived(_selfPeer, msg);
            });
        }

        public void SendFileAttachment(string message, string filePath)
        {
            if (message.Length > MAX_MESSAGE_SIZE)
                throw new IOException("MeshNetwork message data size cannot exceed " + MAX_MESSAGE_SIZE + " bytes.");

            MessageRecipient[] msgRcpt = GetMessageRecipients();

            MessageItem msg = new MessageItem(DateTime.UtcNow, _userId, msgRcpt, MessageType.FileAttachment, message, null, Path.GetFileName(filePath), (new FileInfo(filePath)).Length, filePath);
            msg.WriteTo(_store);

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                SendMessageBroadcast(msg.GetMeshNetworkPacket());

                RaiseEventMessageReceived(_selfPeer, msg);
            });
        }

        public MessageItem[] GetLatestMessages(int index, int count)
        {
            return MessageItem.GetLatestMessageItems(_store, index, count);
        }

        public int GetMessageCount()
        {
            return _store.GetMessageCount();
        }

        public int DhtGetTotalIPv4Peers()
        {
            lock (_dhtIPv4Peers)
            {
                return _dhtIPv4Peers.Count;
            }
        }

        public EndPoint[] DhtGetIPv4Peers()
        {
            lock (_dhtIPv4Peers)
            {
                return _dhtIPv4Peers.ToArray();
            }
        }

        public int DhtGetTotalIPv6Peers()
        {
            lock (_dhtIPv6Peers)
            {
                return _dhtIPv6Peers.Count;
            }
        }

        public EndPoint[] DhtGetIPv6Peers()
        {
            lock (_dhtIPv6Peers)
            {
                return _dhtIPv6Peers.ToArray();
            }
        }

        public int DhtGetTotalLanPeers()
        {
            lock (_dhtLanPeers)
            {
                return _dhtLanPeers.Count;
            }
        }

        public EndPoint[] DhtGetLanPeers()
        {
            lock (_dhtLanPeers)
            {
                return _dhtLanPeers.ToArray();
            }
        }

        public int DhtGetTotalTorPeers()
        {
            lock (_dhtTorPeers)
            {
                return _dhtTorPeers.Count;
            }
        }

        public EndPoint[] DhtGetTorPeers()
        {
            lock (_dhtTorPeers)
            {
                return _dhtTorPeers.ToArray();
            }
        }

        public TimeSpan DhtNextUpdateIn()
        {
            return _dhtLastUpdated.AddMilliseconds(DHT_ANNOUNCE_TIMER_INTERVAL) - DateTime.UtcNow;
        }

        public int TcpRelayGetTotalIPv4Peers()
        {
            lock (_ipv4TcpRelayPeers)
            {
                return _ipv4TcpRelayPeers.Count;
            }
        }

        public EndPoint[] TcpRelayGetIPv4Peers()
        {
            lock (_ipv4TcpRelayPeers)
            {
                return _ipv4TcpRelayPeers.ToArray();
            }
        }

        public int TcpRelayGetTotalIPv6Peers()
        {
            lock (_ipv6TcpRelayPeers)
            {
                return _ipv6TcpRelayPeers.Count;
            }
        }

        public EndPoint[] TcpRelayGetIPv6Peers()
        {
            lock (_ipv6TcpRelayPeers)
            {
                return _ipv6TcpRelayPeers.ToArray();
            }
        }

        public void DeleteNetwork()
        {
            //dispose
            this.Dispose();

            //delete message store index and data
            string messageStoreFolder = Path.Combine(_connectionManager.Node.ProfileFolder, "messages");

            try
            {
                File.Delete(Path.Combine(messageStoreFolder, _messageStoreId + ".index"));
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);
            }

            try
            {
                File.Delete(Path.Combine(messageStoreFolder, _messageStoreId + ".data"));
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);
            }

            //unregister network from tcp relays
            _connectionManager.TcpRelayClientUnregisterHostedNetwork(_networkId);

            //remove object from mesh node
            _connectionManager.Node.DeleteMeshNetwork(this);
        }

        public void WriteTo(BinaryWriter bW)
        {
            bW.Write((byte)1); //version

            bW.Write((byte)_type);
            _userId.WriteTo(bW.BaseStream);

            if (_type == MeshNetworkType.Group)
                bW.WriteShortString(_networkName);

            bW.WriteShortString(_sharedSecret);
            bW.Write((byte)_status);

            //
            _networkId.WriteTo(bW.BaseStream);
            _networkSecret.WriteTo(bW.BaseStream);

            //
            bW.WriteShortString(_messageStoreId);
            bW.WriteBuffer(_messageStoreKey);

            //
            bW.Write(_localNetworkOnlyDateModified);
            bW.Write(_localNetworkOnly);

            //
            bW.Write(_groupDisplayImageDateModified);
            bW.WriteBuffer(_groupDisplayImage);

            //
            bW.Write(_groupLockNetworkDateModified);
            bW.Write(_groupLockNetwork);

            //
            bW.Write(_mute);

            //known peers
            if (_type == MeshNetworkType.Private)
            {
                _otherPeer.GetPeerInfo().WriteTo(bW);
            }
            else
            {
                _peersLock.EnterReadLock();
                try
                {
                    bW.Write(Convert.ToByte(_peers.Count - 1)); //not counting self peer

                    foreach (KeyValuePair<BinaryNumber, Peer> peer in _peers)
                    {
                        if (!peer.Value.IsSelfPeer)
                            peer.Value.GetPeerInfo().WriteTo(bW);
                    }
                }
                finally
                {
                    _peersLock.ExitReadLock();
                }
            }
        }

        public override string ToString()
        {
            return this.NetworkName;
        }

        #endregion

        #region properties

        public MeshNode Node
        { get { return _connectionManager.Node; } }

        public MeshNetworkType Type
        { get { return _type; } }

        public BinaryNumber NetworkId
        { get { return _networkId; } }

        public MeshNetworkStatus Status
        { get { return _status; } }

        public string NetworkName
        {
            get
            {
                if (_type == MeshNetworkType.Private)
                {
                    if (_otherPeer.ProfileDisplayName != null)
                        return _otherPeer.ProfileDisplayName;

                    return _otherPeer.PeerUserId.ToString();
                }
                else
                {
                    return _networkName;
                }
            }
        }

        public string SharedSecret
        {
            get { return _sharedSecret; }
            set
            {
                BinaryNumber newNetworkId;
                BinaryNumber newNetworkSecret;

                switch (_type)
                {
                    case MeshNetworkType.Private:
                        newNetworkId = GetPrivateNetworkId(_userId, _otherPeer.PeerUserId, value);
                        newNetworkSecret = GetPrivateNetworkSecret(_userId, _otherPeer.PeerUserId, value);
                        break;

                    default:
                        newNetworkId = GetGroupNetworkId(_networkName, value);
                        newNetworkSecret = GetGroupNetworkSecret(_networkName, value);
                        break;
                }

                try
                {
                    _connectionManager.Node.MeshNetworkChanged(this, newNetworkId);

                    _sharedSecret = value;
                    _networkId = newNetworkId;
                    _networkSecret = newNetworkSecret;
                }
                catch (ArgumentException)
                {
                    throw new MeshException("Unable to change shared secret/password. Mesh network with same network id already exists.");
                }
            }
        }

        public Peer SelfPeer
        { get { return _selfPeer; } }

        public Peer OtherPeer
        { get { return _otherPeer; } }

        public bool LocalNetworkOnly
        {
            get { return _localNetworkOnly; }
            set
            {
                _localNetworkOnlyDateModified = DateTime.UtcNow;
                _localNetworkOnly = value;

                if (_localNetworkOnly)
                {
                    lock (_dhtIPv4Peers)
                    {
                        _dhtIPv4Peers.Clear();
                    }

                    lock (_dhtIPv6Peers)
                    {
                        _dhtIPv6Peers.Clear();
                    }

                    lock (_ipv4TcpRelayPeers)
                    {
                        _ipv4TcpRelayPeers.Clear();
                    }

                    lock (_ipv6TcpRelayPeers)
                    {
                        _ipv6TcpRelayPeers.Clear();
                    }

                    //unregister from tcp relay
                    _connectionManager.TcpRelayClientUnregisterHostedNetwork(_networkId);
                }

                //notify UI
                string infoText;

                if (_localNetworkOnly)
                    infoText = "Mesh group network was updated to work only on local LAN networks by " + _selfPeer.ProfileDisplayName;
                else
                    infoText = "Mesh group network was updated to work on Internet and local LAN networks by " + _selfPeer.ProfileDisplayName;

                MessageItem msg = new MessageItem(DateTime.UtcNow, _userId, null, MessageType.Info, infoText, null, null, 0, null);
                msg.WriteTo(_store);

                RaiseEventMessageReceived(_selfPeer, msg);

                //notify peers
                SendMessageBroadcast(new MeshNetworkPacketLocalNetworkOnly(_localNetworkOnlyDateModified, _localNetworkOnly));
            }
        }

        public byte[] GroupDisplayImage
        {
            get { return _groupDisplayImage; }
            set
            {
                if (_type != MeshNetworkType.Group)
                    throw new InvalidOperationException("Cannot set group display image for non group network.");

                _groupDisplayImageDateModified = DateTime.UtcNow;
                _groupDisplayImage = value;

                //notify UI
                RaiseEventGroupImageChanged(_selfPeer);

                //notify peers
                SendMessageBroadcast(new MeshNetworkPacketGroupDisplayImage(_groupDisplayImageDateModified, _groupDisplayImage));
            }
        }

        public bool GroupLockNetwork
        {
            get { return _groupLockNetwork; }
            set
            {
                if (_type != MeshNetworkType.Group)
                    throw new InvalidOperationException("Cannot set group lock network for non group network.");

                _groupLockNetworkDateModified = DateTime.UtcNow;
                _groupLockNetwork = value;

                //notify UI

                string infoText;

                if (_groupLockNetwork)
                    infoText = "Mesh group network was locked by " + _selfPeer.ProfileDisplayName;
                else
                    infoText = "Mesh group network was unlocked by " + _selfPeer.ProfileDisplayName;

                MessageItem msg = new MessageItem(DateTime.UtcNow, _userId, null, MessageType.Info, infoText, null, null, 0, null);
                msg.WriteTo(_store);

                RaiseEventMessageReceived(_selfPeer, msg);

                //notify peers
                SendMessageBroadcast(new MeshNetworkPacketGroupLockNetwork(_groupLockNetworkDateModified, _groupLockNetwork));
            }
        }

        public bool Mute
        {
            get { return _mute; }
            set { _mute = value; }
        }

        public bool IsIPv4DhtRunning
        { get { return _dhtAnnounceTimerIsRunning && !_localNetworkOnly; } }

        public bool IsIPv6DhtRunning
        { get { return _dhtAnnounceTimerIsRunning && !_localNetworkOnly; } }

        public bool IsLanDhtRunning
        { get { return _dhtAnnounceTimerIsRunning && (_connectionManager.Node.Type == MeshNodeType.P2P); } }

        public bool IsTorDhtRunning
        { get { return _dhtAnnounceTimerIsRunning && (_connectionManager.Node.Type == MeshNodeType.Tor); } }

        public bool IsTcpRelayClientRunning
        { get { return !_localNetworkOnly; } }

        #endregion

        public enum PeerConnectivityStatus
        {
            NoNetwork = 0,
            PartialMeshNetwork = 1,
            FullMeshNetwork = 2
        }

        public class Peer : IDisposable
        {
            #region events

            public event EventHandler StateChanged;
            public event EventHandler ProfileChanged;
            public event EventHandler ConnectivityStatusChanged;

            #endregion

            #region variables

            readonly MeshNetwork _network;
            readonly BinaryNumber _peerUserId;

            readonly bool _isSelfPeer;
            bool _isOnline = false;

            PeerConnectivityStatus _connectivityStatus = PeerConnectivityStatus.NoNetwork;
            List<MeshNetworkPeerInfo> _connectedPeerList;
            List<MeshNetworkPeerInfo> _disconnectedPeerList;

            MeshNetworkPacketProfile _profile;
            MeshNetworkPacketProfileDisplayImage _profileImage;

            readonly List<Session> _sessions = new List<Session>(1);
            readonly ReaderWriterLockSlim _sessionsLock = new ReaderWriterLockSlim();

            BinaryNumber _maskedPeerUserId;

            #endregion

            #region constructor

            internal Peer(MeshNetwork network, BinaryNumber peerUserId, string peerDisplayName)
            {
                _network = network;
                _peerUserId = peerUserId;

                _isSelfPeer = _network._userId.Equals(_peerUserId);

                if (!_isSelfPeer)
                    _profile = new MeshNetworkPacketProfile(DateTime.MinValue, peerDisplayName, MeshProfileStatus.None, null);
            }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                Dispose(true);
            }

            bool _isDisposing = false;
            bool _disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    _isDisposing = true;

                    _sessionsLock.EnterWriteLock();
                    try
                    {
                        foreach (Session session in _sessions)
                            session.Dispose();

                        _sessions.Clear();
                    }
                    finally
                    {
                        _sessionsLock.ExitWriteLock();
                    }

                    _sessionsLock.Dispose();
                }

                _disposed = true;
            }

            #endregion

            #region private event functions

            internal void RaiseEventStateChanged()
            {
                _network._connectionManager.Node.SynchronizationContext.Send(delegate (object state)
                {
                    StateChanged?.Invoke(this, EventArgs.Empty);
                }, null);

                //send info message
                string message;

                if (this.IsOnline)
                    message = this.ProfileDisplayName + " is online";
                else
                    message = this.ProfileDisplayName + " is offline";

                MessageItem msg = new MessageItem(message);
                msg.WriteTo(_network._store);

                _network.RaiseEventMessageReceived(this, msg);
            }

            internal void RaiseEventProfileChanged()
            {
                _network._connectionManager.Node.SynchronizationContext.Send(delegate (object state)
                {
                    ProfileChanged?.Invoke(this, EventArgs.Empty);
                }, null);
            }

            private void RaiseEventConnectivityStatusChanged()
            {
                _network._connectionManager.Node.SynchronizationContext.Send(delegate (object state)
                {
                    ConnectivityStatusChanged?.Invoke(this, EventArgs.Empty);
                }, null);
            }

            #endregion

            #region internal/private

            internal void SendMessage(byte[] data, int offset, int count)
            {
                _sessionsLock.EnterReadLock();
                try
                {
                    foreach (Session session in _sessions)
                    {
                        session.SendMessage(data, offset, count);
                    }
                }
                finally
                {
                    _sessionsLock.ExitReadLock();
                }
            }

            internal void AddSession(SecureChannelStream channel, Connection connection)
            {
                Session session;

                _sessionsLock.EnterWriteLock();
                try
                {
                    session = new Session(this, channel, connection);
                    _sessions.Add(session);
                }
                finally
                {
                    _sessionsLock.ExitWriteLock();
                }

                if (!_isOnline)
                {
                    _isOnline = true;
                    RaiseEventStateChanged(); //notify UI that peer is online
                }

                //send profile, image & settings to this session
                MeshNode node = _network._connectionManager.Node;
                session.SendMessage(new MeshNetworkPacketProfile(node.ProfileDateModified, node.ProfileDisplayName, node.ProfileStatus, node.ProfileStatusMessage));
                session.SendMessage(new MeshNetworkPacketProfileDisplayImage(node.ProfileDisplayImageDateModified, node.ProfileDisplayImage));
                session.SendMessage(new MeshNetworkPacketLocalNetworkOnly(_network._localNetworkOnlyDateModified, _network._localNetworkOnly));

                //peer exchange
                _network.UpdateConnectivityStatus();
                _network.DoPeerExchange();

                switch (_network.Type)
                {
                    case MeshNetworkType.Private:
                        ReSendUndeliveredMessages(session); //feature only for private chat. since, group chat can have multiple offline users, sending undelivered messages will create partial & confusing conversation for the one who comes online later.
                        break;

                    case MeshNetworkType.Group:
                        session.SendMessage(new MeshNetworkPacketGroupDisplayImage(_network._groupDisplayImageDateModified, _network._groupDisplayImage)); //group image feature
                        session.SendMessage(new MeshNetworkPacketGroupLockNetwork(_network._groupLockNetworkDateModified, _network._groupLockNetwork)); //group lock setting
                        break;
                }
            }

            private void RemoveSession(Session session)
            {
                if (!_isDisposing)
                {
                    //remove this session from peer
                    _sessionsLock.EnterWriteLock();
                    try
                    {
                        _sessions.Remove(session);

                        _isOnline = (_sessions.Count > 0);
                    }
                    finally
                    {
                        _sessionsLock.ExitWriteLock();
                    }

                    if (!_isOnline)
                    {
                        _connectivityStatus = PeerConnectivityStatus.NoNetwork;

                        RaiseEventStateChanged(); //notify UI that peer is offline
                    }

                    //peer exchange
                    _network.UpdateConnectivityStatus();
                    _network.DoPeerExchange();
                }
            }

            internal void Disconnect()
            {
                _sessionsLock.EnterReadLock();
                try
                {
                    foreach (Session session in _sessions)
                        session.Disconnect();
                }
                finally
                {
                    _sessionsLock.ExitReadLock();
                }
            }

            internal bool IsConnectedVia(EndPoint peerEP)
            {
                _sessionsLock.EnterReadLock();
                try
                {
                    foreach (Session session in _sessions)
                    {
                        if (session.RemotePeerEP.Equals(peerEP))
                            return true;
                    }
                }
                finally
                {
                    _sessionsLock.ExitReadLock();
                }

                return false;
            }

            internal MeshNetworkPeerInfo GetPeerInfo()
            {
                List<EndPoint> peerEPList = new List<EndPoint>();

                _sessionsLock.EnterReadLock();
                try
                {
                    foreach (Session session in _sessions)
                        peerEPList.Add(session.RemotePeerEP);
                }
                finally
                {
                    _sessionsLock.ExitReadLock();
                }

                return new MeshNetworkPeerInfo(_peerUserId, this.ProfileDisplayName, peerEPList.ToArray());
            }

            internal List<MeshNetworkPeerInfo> GetConnectedPeerList()
            {
                List<MeshNetworkPeerInfo> connectedPeerList = new List<MeshNetworkPeerInfo>();

                if (_isSelfPeer)
                {
                    //add connected peer info from this mesh network for self

                    if (_network._type == MeshNetworkType.Private)
                    {
                        connectedPeerList.Add(this.GetPeerInfo());

                        if (_network._otherPeer._isOnline)
                            connectedPeerList.Add(_network._otherPeer.GetPeerInfo());
                    }
                    else
                    {
                        _network._peersLock.EnterReadLock();
                        try
                        {
                            foreach (KeyValuePair<BinaryNumber, Peer> peer in _network._peers)
                            {
                                if (peer.Value.IsOnline)
                                    connectedPeerList.Add(peer.Value.GetPeerInfo());
                            }
                        }
                        finally
                        {
                            _network._peersLock.ExitReadLock();
                        }
                    }
                }

                //note: self peer may have sessions from another device too and from this network

                //add peer info from sessions
                _sessionsLock.EnterReadLock();
                try
                {
                    foreach (Session session in _sessions)
                    {
                        foreach (MeshNetworkPeerInfo peerInfo in session.GetConnectedPeerList())
                        {
                            if (!connectedPeerList.Contains(peerInfo))
                                connectedPeerList.Add(peerInfo);
                        }
                    }
                }
                finally
                {
                    _sessionsLock.ExitReadLock();
                }

                //keep internal copy for later connectivity status check call and UI
                _connectedPeerList = connectedPeerList;

                return connectedPeerList;
            }

            internal void UpdateConnectivityStatus(List<MeshNetworkPeerInfo> uniquePeerInfoList)
            {
                PeerConnectivityStatus oldStatus = _connectivityStatus;

                List<MeshNetworkPeerInfo> connectedPeerList = _connectedPeerList;
                List<MeshNetworkPeerInfo> disconnectedPeerList = new List<MeshNetworkPeerInfo>();

                if (connectedPeerList != null)
                {
                    foreach (MeshNetworkPeerInfo checkEP in connectedPeerList)
                    {
                        if (!uniquePeerInfoList.Contains(checkEP))
                            disconnectedPeerList.Add(checkEP);
                    }

                    //remove self from the disconnected list
                    if (_disconnectedPeerList != null)
                        _disconnectedPeerList.Remove(new MeshNetworkPeerInfo(_peerUserId, new IPEndPoint(IPAddress.Any, 0))); //new object with just peer userId would be enough to remove it from list due to PeerInfo.Equals()
                }

                if (disconnectedPeerList.Count > 0)
                    _connectivityStatus = PeerConnectivityStatus.PartialMeshNetwork;
                else if ((connectedPeerList != null) && (connectedPeerList.Count > 0))
                    _connectivityStatus = PeerConnectivityStatus.FullMeshNetwork;
                else
                    _connectivityStatus = PeerConnectivityStatus.NoNetwork;

                //keep a copy for UI
                _disconnectedPeerList = disconnectedPeerList;

                //notify UI
                RaiseEventConnectivityStatusChanged();
            }

            private void ReSendUndeliveredMessages(Session session)
            {
                List<MessageItem> undeliveredMessages = new List<MessageItem>(10);
                BinaryNumber selfUserId = _network._selfPeer._peerUserId;

                for (int i = _network._store.GetMessageCount() - 1; i > -1; i--)
                {
                    MessageItem msg = new MessageItem(_network._store, i);

                    if ((msg.Type == MessageType.TextMessage) && msg.SenderUserId.Equals(selfUserId))
                    {
                        if (msg.GetDeliveryStatus() == MessageDeliveryStatus.Undelivered)
                            undeliveredMessages.Add(msg);
                        else
                            break;
                    }
                }

                for (int i = undeliveredMessages.Count - 1; i > -1; i--)
                {
                    session.SendMessage(undeliveredMessages[i].GetMeshNetworkPacket());
                }
            }

            private void DoSendGroupImage(Session session)
            {
                session.SendMessage(new MeshNetworkPacketGroupDisplayImage(_network._groupDisplayImageDateModified, _network._groupDisplayImage));
            }

            #endregion

            #region public

            public FileTransfer ReceiveFileAttachment(int messageNumber, string fileName)
            {
                Session[] sessions;

                _sessionsLock.EnterReadLock();
                try
                {
                    sessions = _sessions.ToArray();
                }
                finally
                {
                    _sessionsLock.ExitReadLock();
                }

                return new FileTransfer(messageNumber, Path.Combine(_network.Node.DownloadFolder, fileName), sessions);
            }

            public override string ToString()
            {
                return this.ProfileDisplayName;
            }

            #endregion

            #region properties

            public BinaryNumber PeerUserId
            { get { return _peerUserId; } }

            public BinaryNumber MaskedPeerUserId
            {
                get
                {
                    if (_maskedPeerUserId == null)
                        _maskedPeerUserId = GetMaskedUserId(_peerUserId);

                    return _maskedPeerUserId;
                }
            }

            public MeshNetwork Network
            { get { return _network; } }

            public string ProfileDisplayName
            {
                get
                {
                    if (_isSelfPeer)
                        return _network._connectionManager.Node.ProfileDisplayName;
                    else
                        return _profile.ProfileDisplayName;
                }
            }

            public byte[] ProfileDisplayImage
            {
                get
                {
                    if (_isSelfPeer)
                        return _network._connectionManager.Node.ProfileDisplayImage;
                    else if (_profileImage != null)
                        return _profileImage.ProfileDisplayImage;
                    else
                        return null;
                }
            }

            public MeshProfileStatus ProfileStatus
            {
                get
                {
                    if (_isSelfPeer)
                        return _network._connectionManager.Node.ProfileStatus;
                    else
                        return _profile.ProfileStatus;
                }
            }

            public string ProfileStatusMessage
            {
                get
                {
                    if (_isSelfPeer)
                        return _network._connectionManager.Node.ProfileStatusMessage;
                    else
                        return _profile.ProfileStatusMessage;
                }
            }

            public bool IsOnline
            {
                get
                {
                    if (_isSelfPeer)
                        return (_network._status == MeshNetworkStatus.Online);

                    return _isOnline;
                }
            }

            public bool IsSelfPeer
            { get { return _isSelfPeer; } }

            public PeerConnectivityStatus ConnectivityStatus
            { get { return _connectivityStatus; } }

            public MeshNetworkPeerInfo[] ConnectedWith
            {
                get
                {
                    List<MeshNetworkPeerInfo> connectedPeerList = _connectedPeerList;

                    if (connectedPeerList == null)
                        return new MeshNetworkPeerInfo[] { };

                    return connectedPeerList.ToArray();
                }
            }

            public MeshNetworkPeerInfo[] NotConnectedWith
            {
                get
                {
                    List<MeshNetworkPeerInfo> disconnectedPeerList = _disconnectedPeerList;

                    if (disconnectedPeerList == null)
                        return new MeshNetworkPeerInfo[] { };

                    return disconnectedPeerList.ToArray();
                }
            }

            public SecureChannelCipherSuite CipherSuite
            {
                get
                {
                    _sessionsLock.EnterReadLock();
                    try
                    {
                        if (_sessions.Count > 0)
                            return _sessions[0].CipherSuite;
                        else
                            return SecureChannelCipherSuite.None;
                    }
                    finally
                    {
                        _sessionsLock.ExitReadLock();
                    }
                }
            }

            #endregion

            public enum FileTransferStatus
            {
                Unknown = 0,
                Starting = 1,
                Downloading = 2,
                Complete = 3,
                Canceled = 4,
                Failed = 5,
                Error = 6
            }

            public class FileTransfer
            {
                #region variables

                Thread _thread;

                string _filePath;
                FileTransferStatus _status;
                long _bytesReceived;
                long _fileSize;
                DateTime _startedOn;

                #endregion

                #region constructor

                internal FileTransfer(int messageNumber, string filePath, Session[] sessions)
                {
                    _filePath = filePath;

                    _thread = new Thread(delegate (object state)
                    {
                        Stream dS = null;

                        try
                        {
                            using (FileStream fS = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                _bytesReceived = fS.Length;
                                fS.Position = _bytesReceived;

                                _status = FileTransferStatus.Starting;

                                //find session for file transfer
                                foreach (Session session in sessions)
                                {
                                    dS = session.ReceiveFile(messageNumber, _bytesReceived);
                                    if (dS != null)
                                        break;
                                }

                                if (dS == null)
                                {
                                    //file transfer declined by remote peer
                                    _status = FileTransferStatus.Failed;
                                    return;
                                }

                                //start file transfer
                                _status = FileTransferStatus.Downloading;
                                _startedOn = DateTime.UtcNow;
                                byte[] buffer = new byte[32 * 1024];
                                int bytesRead;

                                //read file size
                                dS.ReadBytes(buffer, 0, 8);
                                _fileSize = BitConverter.ToInt64(buffer, 0);

                                //read file data
                                while (true)
                                {
                                    bytesRead = dS.Read(buffer, 0, buffer.Length);
                                    if (bytesRead < 1)
                                        break;

                                    fS.Write(buffer, 0, bytesRead);

                                    _bytesReceived += bytesRead;
                                }

                                if (_bytesReceived >= _fileSize)
                                    _status = FileTransferStatus.Complete;
                                else
                                    _status = FileTransferStatus.Failed;
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            _status = FileTransferStatus.Canceled;
                        }
                        catch (Exception ex)
                        {
                            _status = FileTransferStatus.Error;
                            Debug.Write(this.GetType().Name, ex);
                        }
                        finally
                        {
                            if (dS != null)
                                dS.Dispose();
                        }
                    });

                    _thread.IsBackground = true;
                    _thread.Start();
                }

                #endregion

                #region public

                public void Cancel()
                {
                    _thread.Abort();
                }

                #endregion

                #region properties

                public string FilePath
                { get { return _filePath; } }

                public FileTransferStatus Status
                { get { return _status; } }

                public long BytesReceived
                { get { return _bytesReceived; } }

                public long FileSize
                { get { return _fileSize; } }

                public DateTime StartedOn
                { get { return _startedOn; } }

                public int ProgressPercentage
                {
                    get
                    {
                        if (_fileSize < 1)
                            return 100;

                        return (int)((_bytesReceived * 100) / _fileSize);
                    }
                }

                public double DownloadSpeed
                {
                    get
                    {
                        long secondsElapsed = (long)(DateTime.UtcNow - _startedOn).TotalSeconds;
                        if (secondsElapsed < 1)
                            return 0.0;

                        return _bytesReceived / secondsElapsed;
                    }
                }

                #endregion
            }

            internal class Session : IDisposable
            {
                #region variables

                readonly Peer _peer;
                readonly SecureChannelStream _channel;
                readonly Connection _connection;

                readonly Thread _readThread;

                MeshNetworkPacketPeerExchange _peerExchange; //saved info for getting connected peers for connectivity status

                readonly Dictionary<ushort, DataStream> _dataStreams = new Dictionary<ushort, DataStream>();
                ushort _lastPort = 0;

                #endregion

                #region constructor

                public Session(Peer peer, SecureChannelStream channel, Connection connection)
                {
                    _peer = peer;
                    _channel = channel;
                    _connection = connection;

                    //client will use odd port & server will use even port to avoid conflicts
                    if (_channel is SecureChannelClientStream)
                        _lastPort = 1;

                    //start read thread
                    _readThread = new Thread(ReadMessageAsync);
                    _readThread.IsBackground = true;
                    _readThread.Start();
                }

                #endregion

                #region IDisposable

                public void Dispose()
                {
                    Dispose(true);
                }

                bool _isDisposing = false;
                bool _disposed = false;

                protected virtual void Dispose(bool disposing)
                {
                    lock (this)
                    {
                        if (_disposed)
                            return;

                        if (disposing)
                        {
                            _isDisposing = true;

                            //close all data streams
                            lock (_dataStreams)
                            {
                                foreach (KeyValuePair<ushort, DataStream> dataStream in _dataStreams)
                                    dataStream.Value.Dispose();

                                _dataStreams.Clear();
                            }

                            //close base secure channel
                            _channel?.Dispose();

                            //remove session
                            try
                            {
                                _peer.RemoveSession(this);
                            }
                            catch
                            { }
                        }

                        _disposed = true;
                    }
                }

                #endregion

                #region private

                private void WriteDataPacket(ushort port, byte[] data, int offset, int count)
                {
                    if ((port == 0) && (count > ushort.MaxValue))
                        throw new ArgumentOutOfRangeException("Data count cannot exceed " + ushort.MaxValue + " for port 0.");

                    int packetCount = ushort.MaxValue;

                    while (true)
                    {
                        if (count < packetCount)
                            packetCount = count;

                        lock (_channel)
                        {
                            _channel.Write(BitConverter.GetBytes(port), 0, 2); //write port
                            _channel.Write(BitConverter.GetBytes(Convert.ToUInt16(packetCount)), 0, 2); //write data length
                            _channel.Write(data, offset, packetCount);//write data

                            offset += packetCount;
                            count -= packetCount;

                            if (count < 1)
                            {
                                _channel.Flush(); //flush base stream
                                break;
                            }
                        }
                    }
                }

                private void ReadMessageAsync(object state)
                {
                    try
                    {
                        BinaryReader bR = new BinaryReader(_channel);
                        ushort port;
                        OffsetStream dataStream = new OffsetStream(_channel, 0, 0, true, false);
                        BinaryReader dataReader = new BinaryReader(dataStream);

                        while (true)
                        {
                            port = bR.ReadUInt16();
                            dataStream.Reset(0, bR.ReadUInt16(), 0);

                            if (port == 0)
                            {
                                MeshNetworkPacket packet = MeshNetworkPacket.Parse(dataReader);

                                switch (packet.Type)
                                {
                                    case MeshNetworkPacketType.PingRequest:
                                        SendMessage(new MeshNetworkPacket(MeshNetworkPacketType.PingResponse));
                                        break;

                                    case MeshNetworkPacketType.PingResponse:
                                        //do nothing
                                        break;

                                    case MeshNetworkPacketType.PeerExchange:
                                        MeshNetworkPacketPeerExchange peerExchange = packet as MeshNetworkPacketPeerExchange;

                                        _peerExchange = peerExchange;
                                        _peer._network.UpdateConnectivityStatus();

                                        foreach (MeshNetworkPeerInfo peerInfo in peerExchange.Peers)
                                        {
                                            foreach (IPEndPoint peerEP in peerInfo.PeerEPs)
                                                _peer._network.BeginMakeConnection(peerEP, _connection);
                                        }

                                        break;

                                    case MeshNetworkPacketType.LocalNetworkOnly:
                                        MeshNetworkPacketLocalNetworkOnly localOnly = packet as MeshNetworkPacketLocalNetworkOnly;

                                        if (localOnly.LocalNetworkOnlyDateModified > _peer._network._localNetworkOnlyDateModified)
                                        {
                                            _peer._network._localNetworkOnlyDateModified = localOnly.LocalNetworkOnlyDateModified;

                                            if (_peer._network._localNetworkOnly != localOnly.LocalNetworkOnly)
                                            {
                                                _peer._network._localNetworkOnly = localOnly.LocalNetworkOnly;

                                                if (_peer._network._localNetworkOnly)
                                                {
                                                    lock (_peer._network._dhtIPv4Peers)
                                                    {
                                                        _peer._network._dhtIPv4Peers.Clear();
                                                    }

                                                    lock (_peer._network._dhtIPv6Peers)
                                                    {
                                                        _peer._network._dhtIPv6Peers.Clear();
                                                    }

                                                    lock (_peer._network._ipv4TcpRelayPeers)
                                                    {
                                                        _peer._network._ipv4TcpRelayPeers.Clear();
                                                    }

                                                    lock (_peer._network._ipv6TcpRelayPeers)
                                                    {
                                                        _peer._network._ipv6TcpRelayPeers.Clear();
                                                    }

                                                    //unregister from tcp relay
                                                    _peer._network._connectionManager.TcpRelayClientUnregisterHostedNetwork(_peer._network._networkId);
                                                }

                                                string infoText;

                                                if (_peer._network._localNetworkOnly)
                                                    infoText = "Mesh group network was updated to work only on local LAN networks by " + _peer.ProfileDisplayName;
                                                else
                                                    infoText = "Mesh group network was updated to work on Internet and local LAN networks by " + _peer.ProfileDisplayName;

                                                MessageItem msg = new MessageItem(DateTime.UtcNow, _peer._peerUserId, null, MessageType.Info, infoText, null, null, 0, null);
                                                msg.WriteTo(_peer._network._store);

                                                _peer._network.RaiseEventMessageReceived(_peer, msg);
                                            }
                                        }

                                        break;

                                    case MeshNetworkPacketType.Profile:
                                        MeshNetworkPacketProfile profile = packet as MeshNetworkPacketProfile;

                                        if (_peer._isSelfPeer)
                                        {
                                            MeshNode node = _peer._network._connectionManager.Node;

                                            if (profile.ProfileDateModified > node.ProfileDateModified)
                                            {
                                                node.UpdateProfileWithoutTriggerUpdate(profile.ProfileDateModified, profile.ProfileDisplayName, profile.ProfileStatus, profile.ProfileStatusMessage);

                                                _peer.RaiseEventProfileChanged();
                                            }
                                        }
                                        else
                                        {
                                            if (profile.ProfileDateModified > _peer._profile.ProfileDateModified)
                                            {
                                                _peer._profile = profile;

                                                _peer.RaiseEventProfileChanged();
                                            }
                                        }

                                        break;

                                    case MeshNetworkPacketType.ProfileDisplayImage:
                                        MeshNetworkPacketProfileDisplayImage profileImage = packet as MeshNetworkPacketProfileDisplayImage;

                                        if (_peer._isSelfPeer)
                                        {
                                            MeshNode node = _peer._network._connectionManager.Node;

                                            if (profileImage.ProfileDisplayImageDateModified > node.ProfileDisplayImageDateModified)
                                            {
                                                node.UpdateProfileDisplayImageWithoutTriggerUpdate(profileImage.ProfileDisplayImageDateModified, profileImage.ProfileDisplayImage);

                                                _peer.RaiseEventProfileChanged();
                                            }
                                        }
                                        else
                                        {
                                            if ((_peer._profileImage == null) || (profileImage.ProfileDisplayImageDateModified > _peer._profileImage.ProfileDisplayImageDateModified))
                                            {
                                                _peer._profileImage = profileImage;

                                                _peer.RaiseEventProfileChanged();
                                            }
                                        }

                                        break;

                                    case MeshNetworkPacketType.GroupDisplayImage:
                                        MeshNetworkPacketGroupDisplayImage groupImage = packet as MeshNetworkPacketGroupDisplayImage;

                                        if (groupImage.GroupDisplayImageDateModified > _peer._network._groupDisplayImageDateModified)
                                        {
                                            _peer._network._groupDisplayImage = groupImage.GroupDisplayImage;
                                            _peer._network._groupDisplayImageDateModified = groupImage.GroupDisplayImageDateModified;

                                            _peer._network.RaiseEventGroupImageChanged(_peer);
                                        }

                                        break;

                                    case MeshNetworkPacketType.GroupLockNetwork:
                                        MeshNetworkPacketGroupLockNetwork groupLock = packet as MeshNetworkPacketGroupLockNetwork;

                                        if (groupLock.GroupLockNetworkDateModified > _peer._network._groupLockNetworkDateModified)
                                        {
                                            _peer._network._groupLockNetworkDateModified = groupLock.GroupLockNetworkDateModified;

                                            if (_peer._network._groupLockNetwork != groupLock.GroupLockNetwork)
                                            {
                                                _peer._network._groupLockNetwork = groupLock.GroupLockNetwork;

                                                string infoText;

                                                if (_peer._network._groupLockNetwork)
                                                    infoText = "Mesh group network was locked by " + _peer.ProfileDisplayName;
                                                else
                                                    infoText = "Mesh group network was unlocked by " + _peer.ProfileDisplayName;

                                                MessageItem msg = new MessageItem(DateTime.UtcNow, _peer._peerUserId, null, MessageType.Info, infoText, null, null, 0, null);
                                                msg.WriteTo(_peer._network._store);

                                                _peer._network.RaiseEventMessageReceived(_peer, msg);
                                            }
                                        }
                                        break;

                                    case MeshNetworkPacketType.MessageTypingNotification:
                                        _peer._network.RaiseEventPeerTyping(_peer);
                                        break;

                                    case MeshNetworkPacketType.Message:
                                        {
                                            MeshNetworkPacketMessage message = packet as MeshNetworkPacketMessage;

                                            MessageItem msg = new MessageItem(_peer._peerUserId, message);
                                            msg.WriteTo(_peer._network._store);

                                            _peer._network.RaiseEventMessageReceived(_peer, msg);

                                            //send delivery notification
                                            SendMessage(new MeshNetworkPacketMessageDeliveryNotification(message.MessageNumber));
                                        }
                                        break;

                                    case MeshNetworkPacketType.MessageDeliveryNotification:
                                        {
                                            MeshNetworkPacketMessageDeliveryNotification notification = packet as MeshNetworkPacketMessageDeliveryNotification;

                                            MessageItem msg;

                                            lock (_peer._network._store) //lock to avoid race condition in a group chat. this will prevent message data from getting overwritten.
                                            {
                                                //read existing message from store
                                                msg = new MessageItem(_peer._network._store, notification.MessageNumber);

                                                foreach (MessageRecipient rcpt in msg.Recipients)
                                                {
                                                    if (rcpt.UserId.Equals(_peer._peerUserId))
                                                    {
                                                        rcpt.SetDeliveredStatus();
                                                        break;
                                                    }
                                                }

                                                //update message to store
                                                msg.WriteTo(_peer._network._store);
                                            }

                                            _peer._network.RaiseEventMessageDeliveryNotification(_peer, msg);
                                        }
                                        break;

                                    case MeshNetworkPacketType.FileRequest:
                                        {
                                            MeshNetworkPacketFileRequest fileRequest = packet as MeshNetworkPacketFileRequest;

                                            ThreadPool.QueueUserWorkItem(delegate (object state2)
                                            {
                                                try
                                                {
                                                    //open data port
                                                    using (DataStream dS = OpenDataStream(fileRequest.DataPort))
                                                    {
                                                        //read existing message from store
                                                        MessageItem msg = new MessageItem(_peer._network._store, fileRequest.MessageNumber);

                                                        if (msg.Type == MessageType.FileAttachment)
                                                        {
                                                            //open local file stream
                                                            using (FileStream fS = new FileStream(msg.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                                            {
                                                                //write file size
                                                                dS.Write(BitConverter.GetBytes(fS.Length));

                                                                //set file position to allow pause/resume transfer
                                                                fS.Position = fileRequest.FileOffset;

                                                                //write file data
                                                                fS.CopyTo(dS);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Debug.Write(this.GetType().Name, ex);
                                                }
                                            });
                                        }
                                        break;

                                    default:
                                        //do nothing
                                        break;
                                }
                            }
                            else
                            {
                                DataStream stream = null;

                                try
                                {
                                    lock (_dataStreams)
                                    {
                                        stream = _dataStreams[port];
                                    }

                                    stream.FeedReadBuffer(dataStream, 30000);
                                }
                                catch
                                {
                                    if (stream != null)
                                        stream.Dispose();
                                }
                            }

                            //discard any unread data
                            if (dataStream.Length > dataStream.Position)
                                dataStream.CopyTo(Stream.Null, 1024, Convert.ToInt32(dataStream.Length - dataStream.Position));
                        }
                    }
                    catch (SecureChannelException ex)
                    {
                        _peer._network.SendSecureChannelFailedInfoMessage(ex);
                        Dispose();
                    }
                    catch (EndOfStreamException)
                    {
                        //gracefull secure channel disconnection done
                        Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(this.GetType().Name, ex);

                        Dispose();

                        //try reconnection due to unexpected channel closure (mostly read timed out exception)
                        _peer._network.BeginMakeConnection(_channel.RemotePeerEP);
                    }
                }

                private DataStream OpenDataStream(ushort port = 0)
                {
                    lock (_dataStreams)
                    {
                        if (port == 0)
                        {
                            do
                            {
                                _lastPort += 2;

                                if (_lastPort > (ushort.MaxValue - 3))
                                {
                                    if (_channel is SecureChannelClientStream)
                                        _lastPort = 1;
                                    else
                                        _lastPort = 0;

                                    continue;
                                }
                            }
                            while (_dataStreams.ContainsKey(_lastPort));

                            port = _lastPort;
                        }
                        else if (_dataStreams.ContainsKey(port))
                        {
                            throw new InvalidOperationException("Data port already in use.");
                        }

                        DataStream stream = new DataStream(this, port);
                        _dataStreams.Add(port, stream);

                        return stream;
                    }
                }

                #endregion

                #region public

                public void SendMessage(byte[] data, int offset, int count)
                {
                    WriteDataPacket(0, data, offset, count);
                }

                public void SendMessage(MeshNetworkPacket message)
                {
                    using (MemoryStream mS = new MemoryStream())
                    {
                        message.WriteTo(new BinaryWriter(mS));

                        byte[] buffer = mS.ToArray();
                        WriteDataPacket(0, buffer, 0, buffer.Length);
                    }
                }

                public Stream ReceiveFile(int messageNumber, long fileOffset)
                {
                    //open data port
                    DataStream dS = OpenDataStream();

                    //send file request
                    SendMessage(new MeshNetworkPacketFileRequest(messageNumber, fileOffset, dS.Port));

                    //peek first byte for EOF test
                    int firstByte = dS.PeekByte();
                    if (firstByte < 0)
                    {
                        dS.Dispose();
                        return null; //remote peer disconnected data stream; request failed
                    }

                    return dS;
                }

                public void Disconnect()
                {
                    try
                    {
                        _channel.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(this.GetType().Name, ex);
                    }
                }

                public ICollection<MeshNetworkPeerInfo> GetConnectedPeerList()
                {
                    MeshNetworkPacketPeerExchange peerExchange = _peerExchange;
                    if (peerExchange == null)
                        return new MeshNetworkPeerInfo[] { };

                    return peerExchange.Peers;
                }

                #endregion

                #region properties

                public SecureChannelCipherSuite CipherSuite
                { get { return _channel.SelectedCipher; } }

                public EndPoint RemotePeerEP
                { get { return _channel.RemotePeerEP; } }

                #endregion

                private class DataStream : Stream
                {
                    #region variables

                    const int DATA_READ_TIMEOUT = 60000;
                    const int DATA_WRITE_TIMEOUT = 30000; //dummy

                    readonly Session _session;
                    readonly ushort _port;

                    readonly byte[] _readBuffer = new byte[DATA_STREAM_BUFFER_SIZE];
                    volatile int _readBufferPosition;
                    volatile int _readBufferCount;

                    int _readTimeout = DATA_READ_TIMEOUT;
                    int _writeTimeout = DATA_WRITE_TIMEOUT;

                    #endregion

                    #region constructor

                    public DataStream(Session session, ushort port)
                    {
                        _session = session;
                        _port = port;
                    }

                    #endregion

                    #region IDisposable

                    volatile bool _disposed = false;

                    protected override void Dispose(bool disposing)
                    {
                        lock (this)
                        {
                            if (_disposed)
                                return;

                            if (disposing)
                            {
                                try
                                {
                                    _session.WriteDataPacket(_port, new byte[] { }, 0, 0);
                                }
                                catch (Exception ex)
                                {
                                    Debug.Write(this.GetType().Name, ex);
                                }

                                if (!_session._isDisposing)
                                {
                                    lock (_session._dataStreams)
                                    {
                                        _session._dataStreams.Remove(_port);
                                    }
                                }

                                Monitor.PulseAll(this);
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
                        get { return true; }
                    }

                    public override int ReadTimeout
                    {
                        get { return _readTimeout; }
                        set { _readTimeout = value; }
                    }

                    public override int WriteTimeout
                    {
                        get { return _writeTimeout; }
                        set { _writeTimeout = value; }
                    }

                    public override void Flush()
                    {
                        //do nothing
                    }

                    public override long Length
                    {
                        get { throw new NotSupportedException("DataStream stream does not support seeking."); }
                    }

                    public override long Position
                    {
                        get
                        {
                            throw new NotSupportedException("DataStream stream does not support seeking.");
                        }
                        set
                        {
                            throw new NotSupportedException("DataStream stream does not support seeking.");
                        }
                    }

                    public override long Seek(long offset, SeekOrigin origin)
                    {
                        throw new NotSupportedException("DataStream stream does not support seeking.");
                    }

                    public override void SetLength(long value)
                    {
                        throw new NotSupportedException("DataStream stream does not support seeking.");
                    }

                    public int PeekByte()
                    {
                        byte[] buffer = new byte[1];

                        int bytesRead = Read(buffer, 0, 1);
                        if (bytesRead < 1)
                            return -1;

                        //reset read buffer offset & count
                        _readBufferPosition -= bytesRead;
                        _readBufferCount += bytesRead;

                        return buffer[0];
                    }

                    public override int Read(byte[] buffer, int offset, int count)
                    {
                        if (count < 1)
                            throw new ArgumentOutOfRangeException("Count cannot be less than 1.");

                        lock (this)
                        {
                            if (_readBufferCount < 1)
                            {
                                if (_disposed)
                                    return 0;

                                if (!Monitor.Wait(this, _readTimeout))
                                    throw new IOException("Read timed out.");

                                if (_readBufferCount < 1)
                                    return 0;
                            }

                            int bytesToCopy = count;

                            if (bytesToCopy > _readBufferCount)
                                bytesToCopy = _readBufferCount;

                            Buffer.BlockCopy(_readBuffer, _readBufferPosition, buffer, offset, bytesToCopy);

                            _readBufferPosition += bytesToCopy;
                            _readBufferCount -= bytesToCopy;

                            if (_readBufferCount < 1)
                                Monitor.Pulse(this);

                            return bytesToCopy;
                        }
                    }

                    public override void Write(byte[] buffer, int offset, int count)
                    {
                        if (_disposed)
                            throw new ObjectDisposedException("DataStream");

                        if (count > 0)
                            _session.WriteDataPacket(_port, buffer, offset, count);
                    }

                    #endregion

                    #region private

                    public void FeedReadBuffer(Stream s, int timeout)
                    {
                        int count = Convert.ToInt32(s.Length - s.Position);

                        if (count < 1)
                        {
                            Dispose();
                            return;
                        }

                        int readCount = _readBuffer.Length;

                        while (count > 0)
                        {
                            lock (this)
                            {
                                if (_disposed)
                                    throw new ObjectDisposedException("DataStream");

                                if (_readBufferCount > 0)
                                {
                                    if (!Monitor.Wait(this, timeout))
                                        throw new IOException("DataStream FeedReadBuffer timed out.");

                                    if (_readBufferCount > 0)
                                        throw new IOException("DataStream FeedReadBuffer failed. Buffer not empty.");
                                }

                                if (count < readCount)
                                    readCount = count;

                                s.ReadBytes(_readBuffer, 0, readCount);
                                _readBufferPosition = 0;
                                _readBufferCount = readCount;
                                count -= readCount;

                                Monitor.Pulse(this);
                            }
                        }
                    }

                    #endregion

                    #region properties

                    public ushort Port
                    { get { return _port; } }

                    #endregion
                }
            }
        }
    }
}
