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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;
using TechnitiumLibrary.Net.Proxy;

namespace MeshCore.Network.DHT
{
    public enum DhtNetworkType
    {
        IPv4Internet = 1,
        IPv6Internet = 2,
        LocalNetwork = 3,
        TorNetwork = 4
    }

    public class DhtManager : IDisposable
    {
        #region variables

        const int WRITE_BUFFERED_STREAM_SIZE = 128;

        const int SOCKET_CONNECTION_TIMEOUT = 2000;
        const int SOCKET_SEND_TIMEOUT = 5000;
        const int SOCKET_RECV_TIMEOUT = 5000;

        const int LOCAL_DISCOVERY_ANNOUNCE_PORT = 41988;
        const int ANNOUNCEMENT_INTERVAL = 60000;
        const int ANNOUNCEMENT_RETRY_INTERVAL = 2000;
        const int ANNOUNCEMENT_RETRY_COUNT = 3;

        const int BUFFER_MAX_SIZE = 32;

        const string IPV6_MULTICAST_IP = "FF12::1";

        readonly int _localServicePort;

        const int NETWORK_WATCHER_INTERVAL = 30000;
        readonly Timer _networkWatcher;
        readonly List<NetworkInfo> _networks = new List<NetworkInfo>();
        readonly List<LocalNetworkDhtManager> _localNetworkDhtManagers = new List<LocalNetworkDhtManager>();

        readonly DhtNode _ipv4InternetDhtNode;
        readonly DhtNode _ipv6InternetDhtNode;
        readonly DhtNode _torInternetDhtNode;

        const string DHT_BOOTSTRAP_URL = "https://go.technitium.com/?id=32";
        const int BOOTSTRAP_RETRY_TIMER_INITIAL_INTERVAL = 5000;
        const int BOOTSTRAP_RETRY_TIMER_INTERVAL = 60000;
        readonly Timer _bootstrapRetryTimer;

        #endregion

        #region constructor

        public DhtManager(int localServicePort, IDhtConnectionManager connectionManager, NetProxy proxy, IEnumerable<EndPoint> ipv4BootstrapNodes, IEnumerable<EndPoint> ipv6BootstrapNodes, IEnumerable<EndPoint> torBootstrapNodes, string torOnionAddress, bool enableTorMode)
        {
            _localServicePort = localServicePort;

            //init internet dht nodes
            _ipv4InternetDhtNode = new DhtNode(connectionManager, new IPEndPoint(IPAddress.Any, localServicePort));
            _ipv6InternetDhtNode = new DhtNode(connectionManager, new IPEndPoint(IPAddress.IPv6Any, localServicePort));

            //add known bootstrap nodes
            _ipv4InternetDhtNode.AddNode(ipv4BootstrapNodes);
            _ipv6InternetDhtNode.AddNode(ipv6BootstrapNodes);

            if (enableTorMode)
            {
                //init tor dht node
                _torInternetDhtNode = new DhtNode(connectionManager, new DomainEndPoint(torOnionAddress, localServicePort));

                //add known bootstrap nodes
                _torInternetDhtNode.AddNode(torBootstrapNodes);

                //set higher timeout value for internet and tor DHT nodes since they will be using tor network
                _ipv4InternetDhtNode.QueryTimeout = 10000;
                _ipv6InternetDhtNode.QueryTimeout = 10000;
                _torInternetDhtNode.QueryTimeout = 10000;
            }
            else
            {
                //start network watcher
                _networkWatcher = new Timer(NetworkWatcherAsync, null, 1000, NETWORK_WATCHER_INTERVAL);
            }

            //add bootstrap nodes via web
            _bootstrapRetryTimer = new Timer(delegate (object state)
            {
                try
                {
                    using (WebClientEx wC = new WebClientEx())
                    {
                        wC.Proxy = proxy;
                        wC.Timeout = 10000;

                        using (BinaryReader bR = new BinaryReader(new MemoryStream(wC.DownloadData(DHT_BOOTSTRAP_URL))))
                        {
                            int count = bR.ReadByte();
                            for (int i = 0; i < count; i++)
                                AddNode(EndPointExtension.Parse(bR));
                        }
                    }

                    //bootstrap success, stop retry timer
                    _bootstrapRetryTimer.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);
                }
            }, null, BOOTSTRAP_RETRY_TIMER_INITIAL_INTERVAL, BOOTSTRAP_RETRY_TIMER_INTERVAL);
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
                if (_bootstrapRetryTimer != null)
                    _bootstrapRetryTimer.Dispose();

                if (_networkWatcher != null)
                    _networkWatcher.Dispose();

                lock (_localNetworkDhtManagers)
                {
                    foreach (LocalNetworkDhtManager localNetworkDhtManager in _localNetworkDhtManagers)
                        localNetworkDhtManager.Dispose();

                    _localNetworkDhtManagers.Clear();
                }

                if (_ipv4InternetDhtNode != null)
                    _ipv4InternetDhtNode.Dispose();

                if (_ipv6InternetDhtNode != null)
                    _ipv6InternetDhtNode.Dispose();

                if (_torInternetDhtNode != null)
                    _torInternetDhtNode.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region private

        private void NetworkWatcherAsync(object state)
        {
            try
            {
                bool networkChanged = false;
                List<NetworkInfo> newNetworks = new List<NetworkInfo>();

                {
                    List<NetworkInfo> currentNetworks = NetUtilities.GetNetworkInfo();

                    networkChanged = (currentNetworks.Count != _networks.Count);

                    foreach (NetworkInfo currentNetwork in currentNetworks)
                    {
                        if (!_networks.Contains(currentNetwork))
                        {
                            networkChanged = true;
                            newNetworks.Add(currentNetwork);
                        }
                    }

                    _networks.Clear();
                    _networks.AddRange(currentNetworks);
                }

                if (networkChanged)
                {
                    lock (_localNetworkDhtManagers)
                    {
                        //remove local network dht manager with offline networks
                        {
                            List<LocalNetworkDhtManager> localNetworkDhtManagersToRemove = new List<LocalNetworkDhtManager>();

                            foreach (LocalNetworkDhtManager localNetworkDhtManager in _localNetworkDhtManagers)
                            {
                                if (!_networks.Contains(localNetworkDhtManager.Network))
                                    localNetworkDhtManagersToRemove.Add(localNetworkDhtManager);
                            }

                            foreach (LocalNetworkDhtManager localNetworkDhtManager in localNetworkDhtManagersToRemove)
                            {
                                localNetworkDhtManager.Dispose();
                                _localNetworkDhtManagers.Remove(localNetworkDhtManager);
                            }
                        }

                        //add local network dht managers for new online networks
                        if (newNetworks.Count > 0)
                        {
                            foreach (NetworkInfo network in newNetworks)
                            {
                                if (IPAddress.IsLoopback(network.LocalIP))
                                    continue; //skip loopback networks

                                if (network.LocalIP.AddressFamily == AddressFamily.InterNetworkV6)
                                    continue; //skip ipv6 private networks for saving resources

                                if (!NetUtilities.IsPrivateIP(network.LocalIP))
                                    continue; //skip public networks

                                _localNetworkDhtManagers.Add(new LocalNetworkDhtManager(network));

                                Debug.Write(this.GetType().Name, "local network dht manager created: " + network.LocalIP.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);
            }
        }

        private ICollection<EndPoint> RemoveSelfEndPoint(EndPoint[] peers, EndPoint selfEndPoint)
        {
            switch (selfEndPoint.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                case AddressFamily.InterNetworkV6:
                    selfEndPoint = new IPEndPoint((selfEndPoint as IPEndPoint).Address, _localServicePort);
                    break;

                case AddressFamily.Unspecified:
                    selfEndPoint = new DomainEndPoint((selfEndPoint as DomainEndPoint).Address, _localServicePort);
                    break;

                default:
                    return peers;
            }

            List<EndPoint> newList = new List<EndPoint>();

            foreach (EndPoint peer in peers)
            {
                if (!selfEndPoint.Equals(peer))
                    newList.Add(peer);
            }

            return newList;
        }

        private void AnnounceAsync(BinaryNumber networkId, bool localNetworkOnly, EndPoint serviceEP, Action<DhtNetworkType, ICollection<EndPoint>> callback)
        {
            if (!localNetworkOnly)
            {
                {
                    ThreadPool.QueueUserWorkItem(delegate (object state)
                    {
                        try
                        {
                            EndPoint[] peers;

                            if (serviceEP == null)
                                peers = _ipv4InternetDhtNode.FindPeers(networkId);
                            else
                                peers = _ipv4InternetDhtNode.Announce(networkId, serviceEP);

                            if ((callback != null) && (peers != null) && (peers.Length > 0))
                            {
                                ICollection<EndPoint> finalPeers = RemoveSelfEndPoint(peers, _ipv4InternetDhtNode.LocalNodeEP);

                                if (_torInternetDhtNode != null)
                                    finalPeers = RemoveSelfEndPoint(peers, _torInternetDhtNode.LocalNodeEP);

                                foreach (EndPoint peer in finalPeers)
                                    AddNode(peer);

                                callback(DhtNetworkType.IPv4Internet, finalPeers);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(this.GetType().Name, ex);
                        }
                    });
                }

                {
                    ThreadPool.QueueUserWorkItem(delegate (object state)
                    {
                        try
                        {
                            EndPoint[] peers;

                            if (serviceEP == null)
                                peers = _ipv6InternetDhtNode.FindPeers(networkId);
                            else
                                peers = _ipv6InternetDhtNode.Announce(networkId, serviceEP);

                            if ((callback != null) && (peers != null) && (peers.Length > 0))
                            {
                                ICollection<EndPoint> finalPeers = RemoveSelfEndPoint(peers, _ipv6InternetDhtNode.LocalNodeEP);

                                if (_torInternetDhtNode != null)
                                    finalPeers = RemoveSelfEndPoint(peers, _torInternetDhtNode.LocalNodeEP);

                                foreach (EndPoint peer in finalPeers)
                                    AddNode(peer);

                                callback(DhtNetworkType.IPv6Internet, finalPeers);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(this.GetType().Name, ex);
                        }
                    });
                }
            }

            if (_torInternetDhtNode != null)
            {
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    try
                    {
                        EndPoint[] peers;

                        if (serviceEP == null)
                            peers = _torInternetDhtNode.FindPeers(networkId);
                        else
                            peers = _torInternetDhtNode.Announce(networkId, serviceEP);

                        if ((callback != null) && (peers != null) && (peers.Length > 0))
                            callback(DhtNetworkType.TorNetwork, RemoveSelfEndPoint(peers, _torInternetDhtNode.LocalNodeEP));
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(this.GetType().Name, ex);
                    }
                });
            }

            lock (_localNetworkDhtManagers)
            {
                foreach (LocalNetworkDhtManager localNetworkDhtManager in _localNetworkDhtManagers)
                {
                    ThreadPool.QueueUserWorkItem(delegate (object state)
                    {
                        try
                        {
                            EndPoint[] peers;

                            if (serviceEP == null)
                                peers = localNetworkDhtManager.DhtNode.FindPeers(networkId);
                            else
                                peers = localNetworkDhtManager.DhtNode.Announce(networkId, serviceEP);

                            if ((callback != null) && (peers != null) && (peers.Length > 0))
                                callback(DhtNetworkType.LocalNetwork, RemoveSelfEndPoint(peers, localNetworkDhtManager.DhtNode.LocalNodeEP));
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(this.GetType().Name, ex);
                        }
                    });
                }
            }
        }

        #endregion

        #region public

        public void AddNode(EndPoint nodeEP)
        {
            switch (nodeEP.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    if (!NetUtilities.IsPrivateIPv4((nodeEP as IPEndPoint).Address))
                        _ipv4InternetDhtNode.AddNode(nodeEP);

                    break;

                case AddressFamily.InterNetworkV6:
                    if (NetUtilities.IsPublicIPv6((nodeEP as IPEndPoint).Address))
                        _ipv6InternetDhtNode.AddNode(nodeEP);

                    break;

                case AddressFamily.Unspecified:
                    _torInternetDhtNode?.AddNode(nodeEP);
                    break;
            }
        }

        public void AcceptInternetDhtConnection(Stream s, EndPoint remoteNodeEP)
        {
            switch (remoteNodeEP.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    _ipv4InternetDhtNode.AcceptConnection(s, remoteNodeEP);
                    break;

                case AddressFamily.InterNetworkV6:
                    _ipv6InternetDhtNode.AcceptConnection(s, remoteNodeEP);
                    break;

                case AddressFamily.Unspecified:
                    _torInternetDhtNode?.AcceptConnection(s, remoteNodeEP);
                    break;

                default:
                    throw new NotSupportedException("AddressFamily not supported.");
            }
        }

        public void FindPeersAsync(BinaryNumber networkId, bool localNetworkOnly, Action<DhtNetworkType, ICollection<EndPoint>> callback)
        {
            AnnounceAsync(networkId, localNetworkOnly, null, callback);
        }

        public void AnnounceAsync(BinaryNumber networkId, bool localNetworkOnly, Action<DhtNetworkType, ICollection<EndPoint>> callback)
        {
            EndPoint serviceEP;

            if (_torInternetDhtNode == null)
                serviceEP = _ipv4InternetDhtNode.LocalNodeEP;
            else
                serviceEP = _torInternetDhtNode.LocalNodeEP;

            AnnounceAsync(networkId, localNetworkOnly, serviceEP, callback);
        }

        public EndPoint[] GetIPv4DhtNodes()
        {
            return _ipv4InternetDhtNode.GetAllNodeEPs(false, true);
        }

        public EndPoint[] GetIPv6DhtNodes()
        {
            return _ipv6InternetDhtNode.GetAllNodeEPs(false, true);
        }

        public EndPoint[] GetTorDhtNodes()
        {
            if (_torInternetDhtNode == null)
                return new EndPoint[] { };

            return _torInternetDhtNode.GetAllNodeEPs(false, true);
        }

        public EndPoint[] GetLanDhtNodes()
        {
            List<EndPoint> nodeEPs = new List<EndPoint>();

            lock (_localNetworkDhtManagers)
            {
                foreach (LocalNetworkDhtManager localDht in _localNetworkDhtManagers)
                    nodeEPs.AddRange(localDht.DhtNode.GetAllNodeEPs(false, true));
            }

            return nodeEPs.ToArray();
        }

        public EndPoint[] GetIPv4KRandomNodeEPs()
        {
            return _ipv4InternetDhtNode.GetKRandomNodeEPs(false);
        }

        public EndPoint[] GetIPv6KRandomNodeEPs()
        {
            return _ipv6InternetDhtNode.GetKRandomNodeEPs(false);
        }

        #endregion

        #region properties

        public BinaryNumber IPv4DhtNodeId
        { get { return _ipv4InternetDhtNode.LocalNodeID; } }

        public BinaryNumber IPv6DhtNodeId
        { get { return _ipv6InternetDhtNode.LocalNodeID; } }

        public BinaryNumber TorDhtNodeId
        {
            get
            {
                if (_torInternetDhtNode == null)
                    return null;

                return _torInternetDhtNode.LocalNodeID;
            }
        }

        public int IPv4DhtTotalNodes
        { get { return _ipv4InternetDhtNode.TotalNodes; } }

        public int IPv6DhtTotalNodes
        { get { return _ipv6InternetDhtNode.TotalNodes; } }

        public int TorDhtTotalNodes
        {
            get
            {
                if (_torInternetDhtNode == null)
                    return 0;

                return _torInternetDhtNode.TotalNodes;
            }
        }

        public int LanDhtTotalNodes
        {
            get
            {
                int totalNodes = 0;

                lock (_localNetworkDhtManagers)
                {
                    foreach (LocalNetworkDhtManager localDht in _localNetworkDhtManagers)
                        totalNodes += localDht.DhtNode.TotalNodes;
                }

                return totalNodes;
            }
        }

        #endregion

        class LocalNetworkDhtManager : IDhtConnectionManager, IDisposable
        {
            #region variables

            readonly NetworkInfo _network;

            readonly Socket _udpListener;
            readonly Thread _udpListenerThread;

            readonly Socket _tcpListener;
            readonly Thread _tcpListenerThread;

            readonly IPEndPoint _dhtEndPoint;
            readonly DhtNode _dhtNode;

            readonly Timer _announceTimer;

            #endregion

            #region constructor

            public LocalNetworkDhtManager(NetworkInfo network)
            {
                _network = network;

                //start udp & tcp listeners
                switch (_network.LocalIP.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        _udpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        _tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        break;

                    case AddressFamily.InterNetworkV6:
                        _udpListener = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                        _tcpListener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                        if ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 6))
                        {
                            //windows vista & above
                            _udpListener.DualMode = true;
                            _tcpListener.DualMode = true;
                        }
                        break;

                    default:
                        throw new NotSupportedException("Address family not supported.");
                }

                _udpListener.EnableBroadcast = true;
                _udpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                _udpListener.Bind(new IPEndPoint(_network.LocalIP, LOCAL_DISCOVERY_ANNOUNCE_PORT));

                _tcpListener.Bind(new IPEndPoint(_network.LocalIP, 0));
                _tcpListener.Listen(10);

                _dhtEndPoint = _tcpListener.LocalEndPoint as IPEndPoint;

                //init dht node
                _dhtNode = new DhtNode(this, _dhtEndPoint);

                if (_udpListener.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    NetworkInterface nic = _network.Interface;
                    if ((nic.OperationalStatus == OperationalStatus.Up) && (nic.Supports(NetworkInterfaceComponent.IPv6)) && nic.SupportsMulticast)
                    {
                        try
                        {
                            _udpListener.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(IPAddress.Parse(IPV6_MULTICAST_IP), nic.GetIPProperties().GetIPv6Properties().Index));
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(this.GetType().Name, ex);
                        }
                    }
                }

                //start reading packets
                _udpListenerThread = new Thread(ReceiveUdpPacketAsync);
                _udpListenerThread.IsBackground = true;
                _udpListenerThread.Start();

                //start accepting connections
                _tcpListenerThread = new Thread(AcceptTcpConnectionAsync);
                _tcpListenerThread.IsBackground = true;
                _tcpListenerThread.Start();

                //announce async
                _announceTimer = new Timer(AnnounceAsync, null, 1000, ANNOUNCEMENT_INTERVAL);
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
                    if (_udpListenerThread != null)
                        _udpListenerThread.Abort();

                    if (_tcpListenerThread != null)
                        _tcpListenerThread.Abort();

                    if (_udpListener != null)
                        _udpListener.Dispose();

                    if (_tcpListener != null)
                        _tcpListener.Dispose();

                    if (_dhtNode != null)
                        _dhtNode.Dispose();

                    if (_announceTimer != null)
                        _announceTimer.Dispose();
                }

                _disposed = true;
            }

            #endregion

            #region private

            private void ReceiveUdpPacketAsync(object parameter)
            {
                EndPoint remoteEP;
                byte[] buffer = new byte[BUFFER_MAX_SIZE];
                int bytesRecv;

                if (_udpListener.AddressFamily == AddressFamily.InterNetwork)
                    remoteEP = new IPEndPoint(IPAddress.Any, 0);
                else
                    remoteEP = new IPEndPoint(IPAddress.IPv6Any, 0);

                #region this code ignores ICMP port unreachable responses which creates SocketException in ReceiveFrom()

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    const uint IOC_IN = 0x80000000;
                    const uint IOC_VENDOR = 0x18000000;
                    const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                    _udpListener.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                }

                #endregion

                try
                {
                    while (true)
                    {
                        try
                        {
                            //receive message from remote
                            bytesRecv = _udpListener.ReceiveFrom(buffer, ref remoteEP);
                        }
                        catch (SocketException ex)
                        {
                            switch (ex.SocketErrorCode)
                            {
                                case SocketError.ConnectionReset:
                                case SocketError.HostUnreachable:
                                case SocketError.MessageSize:
                                case SocketError.NetworkReset:
                                    bytesRecv = 0;
                                    break;

                                default:
                                    throw;
                            }
                        }

                        if (bytesRecv > 0)
                        {
                            IPAddress remoteNodeIP = (remoteEP as IPEndPoint).Address;

                            if (remoteNodeIP.IsIPv4MappedToIPv6)
                                remoteNodeIP = remoteNodeIP.MapToIPv4();

                            if (remoteNodeIP.AddressFamily == AddressFamily.InterNetworkV6)
                                remoteNodeIP.ScopeId = 0;

                            try
                            {
                                DhtNodeDiscoveryPacket packet = new DhtNodeDiscoveryPacket(new MemoryStream(buffer, false));

                                IPEndPoint remoteNodeEP = new IPEndPoint(remoteNodeIP, packet.DhtPort);

                                if (!remoteNodeEP.Equals(_dhtEndPoint))
                                {
                                    Debug.Write(this.GetType().Name, "dht node discovered: " + _dhtEndPoint.ToString());

                                    //add node
                                    _dhtNode.AddNode(remoteNodeEP);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Write(this.GetType().Name, ex);
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    //stopping
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);
                }
            }

            private void AcceptTcpConnectionAsync(object parameter)
            {
                try
                {
                    while (true)
                    {
                        Socket socket = _tcpListener.Accept();

                        socket.NoDelay = true;
                        socket.SendTimeout = SOCKET_SEND_TIMEOUT;
                        socket.ReceiveTimeout = SOCKET_RECV_TIMEOUT;

                        ThreadPool.QueueUserWorkItem(delegate (object state)
                        {
                            try
                            {
                                using (Stream s = new WriteBufferedStream(new NetworkStream(socket, true), WRITE_BUFFERED_STREAM_SIZE))
                                {
                                    IPEndPoint remoteNodeEP = socket.RemoteEndPoint as IPEndPoint;

                                    if (remoteNodeEP.Address.IsIPv4MappedToIPv6)
                                        remoteNodeEP = new IPEndPoint(remoteNodeEP.Address.MapToIPv4(), remoteNodeEP.Port);

                                    _dhtNode.AcceptConnection(s, remoteNodeEP);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Write(this.GetType().Name, ex);

                                socket.Dispose();
                            }
                        });
                    }
                }
                catch (ThreadAbortException)
                {
                    //stopping
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);
                }
            }

            private void Broadcast(byte[] buffer, int offset, int count)
            {
                if (_network.LocalIP.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPAddress broadcastIP = _network.BroadcastIP;

                    if (_udpListener.AddressFamily == AddressFamily.InterNetworkV6)
                        broadcastIP = broadcastIP.MapToIPv6();

                    try
                    {
                        _udpListener.SendTo(buffer, offset, count, SocketFlags.None, new IPEndPoint(broadcastIP, LOCAL_DISCOVERY_ANNOUNCE_PORT));
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(this.GetType().Name, ex);
                    }
                }
                else
                {
                    try
                    {
                        _udpListener.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastInterface, _network.Interface.GetIPProperties().GetIPv6Properties().Index);
                        _udpListener.SendTo(buffer, offset, count, SocketFlags.None, new IPEndPoint(IPAddress.Parse(IPV6_MULTICAST_IP), LOCAL_DISCOVERY_ANNOUNCE_PORT));
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(this.GetType().Name, ex);
                    }
                }
            }

            private void AnnounceAsync(object state)
            {
                try
                {
                    if (_dhtNode.TotalNodes < 2)
                    {
                        byte[] announcement = (new DhtNodeDiscoveryPacket((ushort)_dhtEndPoint.Port)).ToArray();

                        for (int i = 0; i < ANNOUNCEMENT_RETRY_COUNT; i++)
                        {
                            Broadcast(announcement, 0, announcement.Length);

                            if (i < ANNOUNCEMENT_RETRY_COUNT - 1)
                                Thread.Sleep(ANNOUNCEMENT_RETRY_INTERVAL);
                        }
                    }
                    else
                    {
                        //stop announcement since one or more nodes were found
                        _announceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);
                }
            }

            #endregion

            #region IDhtConnectionManager support

            Stream IDhtConnectionManager.GetConnection(EndPoint remoteNodeEP)
            {
                Socket socket = null;

                try
                {
                    socket = new Socket(remoteNodeEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    IAsyncResult result = socket.BeginConnect(remoteNodeEP, null, null);
                    if (!result.AsyncWaitHandle.WaitOne(SOCKET_CONNECTION_TIMEOUT))
                        throw new SocketException((int)SocketError.TimedOut);

                    if (!socket.Connected)
                        throw new SocketException((int)SocketError.ConnectionRefused);

                    socket.NoDelay = true;
                    socket.SendTimeout = SOCKET_SEND_TIMEOUT;
                    socket.ReceiveTimeout = SOCKET_RECV_TIMEOUT;

                    return new WriteBufferedStream(new NetworkStream(socket, true), WRITE_BUFFERED_STREAM_SIZE);
                }
                catch
                {
                    if (socket != null)
                        socket.Dispose();

                    throw;
                }
            }

            #endregion

            #region properties

            public NetworkInfo Network
            { get { return _network; } }

            public DhtNode DhtNode
            { get { return _dhtNode; } }

            #endregion
        }

        class DhtNodeDiscoveryPacket
        {
            #region variables

            readonly ushort _dhtPort;

            #endregion

            #region constructor

            public DhtNodeDiscoveryPacket(ushort dhtPort)
            {
                _dhtPort = dhtPort;
            }

            public DhtNodeDiscoveryPacket(Stream s)
            {
                switch (s.ReadByte()) //version
                {
                    case 1:
                        byte[] buffer = new byte[2];
                        s.ReadBytes(buffer, 0, 2);
                        _dhtPort = BitConverter.ToUInt16(buffer, 0);
                        break;

                    case -1:
                        throw new EndOfStreamException();

                    default:
                        throw new IOException("DHT node discovery packet version not supported.");
                }
            }

            #endregion

            #region public

            public byte[] ToArray()
            {
                byte[] buffer = new byte[3];

                buffer[0] = 1; //version
                Buffer.BlockCopy(BitConverter.GetBytes(_dhtPort), 0, buffer, 1, 2); //service port

                return buffer;
            }

            #endregion

            #region properties

            public ushort DhtPort
            { get { return _dhtPort; } }

            #endregion
        }
    }
}
