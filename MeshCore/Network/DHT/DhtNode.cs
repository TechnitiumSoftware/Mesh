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
using System.Net.Sockets;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

/*
 * Kademlia based Distributed Hash Table (DHT) Implementation For Mesh
 * ===================================================================
 *
 * FEATURES IMPLEMENTED
 * --------------------
 * 1. Routing table with K-Bucket: K=8, bucket refresh, contact health check, additional "replacement" list of upto K contacts.
 * 2. RPC: TCP based protocol with PING & FIND_NODE implemented. FIND_PEER & ANNOUNCE_PEER implemented similar to BitTorrent DHT implementation.
 * 3. Peer data eviction after 15 mins of receiving announcement.
 * 4. Parallel lookup: FIND_NODE lookup with alpha=3 implemented.
 * 5. Each node has internal random node ID for managing k-bucket and a different perceived node ID computed based on node's IPEndPoint to limit Sybil attack.
 * 
 * FEATURES NOT IMPLEMENTED
 * ------------------------
 * 1. Node data republishing. Each peer MUST announce itself within 15 mins to all nodes closer to Mesh networkId. This is not feasible due to node having different internal node ID and external perceived node ID.
 * 
 * REFERENCE
 * ---------
 * 1. https://pdos.csail.mit.edu/~petar/papers/maymounkov-kademlia-lncs.pdf
 * 2. http://www.bittorrent.org/beps/bep_0005.html
*/

namespace MeshCore.Network.DHT
{
    class DhtNode : IDisposable
    {
        #region variables

        internal const int KADEMLIA_K = 8;
        internal const int KADEMLIA_B = 5;
        const int HEALTH_CHECK_TIMER_INITIAL_INTERVAL = 30 * 1000; //30 sec
        const int HEALTH_CHECK_TIMER_INTERVAL = 15 * 60 * 1000; //15 min
        const int KADEMLIA_ALPHA = 3;

        readonly IDhtConnectionManager _manager;

        readonly CurrentNode _currentNode;
        readonly KBucket _routingTable;

        readonly Timer _healthTimer;

        int _queryTimeout = 5000;

        #endregion

        #region constructor

        public DhtNode(IDhtConnectionManager manager, EndPoint nodeEP)
        {
            _manager = manager;

            switch (nodeEP.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                case AddressFamily.InterNetworkV6:
                    if (IPAddress.IsLoopback((nodeEP as IPEndPoint).Address))
                        _currentNode = new CurrentNode(BinaryNumber.GenerateRandomNumber256(), nodeEP);
                    else
                        _currentNode = new CurrentNode(nodeEP);

                    break;

                case AddressFamily.Unspecified:
                    _currentNode = new CurrentNode(nodeEP);
                    break;

                default:
                    throw new NotSupportedException();
            }

            //init routing table
            _routingTable = new KBucket(_currentNode);

            //start health timer
            _healthTimer = new Timer(delegate (object state)
            {
                try
                {
                    //remove expired data
                    _currentNode.RemoveExpiredPeers();

                    //check contact health
                    _routingTable.CheckContactHealth(this);

                    //refresh buckets
                    _routingTable.RefreshBucket(this);

                    //find closest contacts for current node id
                    NodeContact[] initialContacts = _routingTable.GetKClosestContacts(_currentNode.NodeId, true);

                    if (initialContacts.Length > 0)
                        QueryFindNode(initialContacts, _currentNode.NodeId); //query manager auto add contacts that respond
                }
                catch (Exception ex)
                {
                    Debug.Write(this.GetType().Name, ex);
                }
            }, null, HEALTH_CHECK_TIMER_INITIAL_INTERVAL, HEALTH_CHECK_TIMER_INTERVAL);
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
                if (_healthTimer != null)
                    _healthTimer.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region private

        private DhtRpcPacket ProcessQuery(DhtRpcPacket query, EndPoint remoteNodeEP)
        {
            //in case of remote node querying via Tor, remoteNodeEP.Address will be loopback IP. Use the onion address from the query as remote end point

            if (!_currentNode.NodeEP.Equals(remoteNodeEP))
            {
                switch (remoteNodeEP.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                    case AddressFamily.InterNetworkV6:
                        IPAddress remoteNodeAddress = (remoteNodeEP as IPEndPoint).Address;

                        if (IPAddress.IsLoopback(remoteNodeAddress) && (query.SourceNodeEP.AddressFamily == AddressFamily.Unspecified) && (query.SourceNodeEP as DomainEndPoint).Address.EndsWith(".onion", StringComparison.OrdinalIgnoreCase))
                            AddNode(query.SourceNodeEP); //use the tor hidden end point claimed by remote node
                        else
                            AddNode(new IPEndPoint(remoteNodeAddress, query.SourceNodeEP.GetPort())); //use remote node end point as seen by connection manager and use port from query

                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            Debug.Write(this.GetType().Name, "query received from: " + remoteNodeEP.ToString() + "; type: " + query.Type.ToString());

            //process query
            switch (query.Type)
            {
                case DhtRpcType.PING:
                    return DhtRpcPacket.CreatePingPacket(_currentNode);

                case DhtRpcType.FIND_NODE:
                    return DhtRpcPacket.CreateFindNodePacketResponse(_currentNode, query.NetworkId, _routingTable.GetKClosestContacts(query.NetworkId, false));

                case DhtRpcType.FIND_PEERS:
                    EndPoint[] peers = _currentNode.GetPeers(query.NetworkId);
                    if (peers.Length == 0)
                        return DhtRpcPacket.CreateFindPeersPacketResponse(_currentNode, query.NetworkId, _routingTable.GetKClosestContacts(query.NetworkId, false), peers);
                    else
                        return DhtRpcPacket.CreateFindPeersPacketResponse(_currentNode, query.NetworkId, new NodeContact[] { }, peers);

                case DhtRpcType.ANNOUNCE_PEER:
                    if ((query.Peers != null) && (query.Peers.Length > 0))
                    {
                        EndPoint peerEP;

                        if (query.Peers[0].AddressFamily == AddressFamily.Unspecified)
                            peerEP = query.Peers[0];
                        else
                            peerEP = new IPEndPoint((remoteNodeEP as IPEndPoint).Address, query.Peers[0].GetPort());

                        _currentNode.StorePeer(query.NetworkId, peerEP);
                    }

                    return DhtRpcPacket.CreateAnnouncePeerPacketResponse(_currentNode, query.NetworkId, _currentNode.GetPeers(query.NetworkId));

                default:
                    throw new Exception("Invalid DHT-RPC type.");
            }
        }

        private DhtRpcPacket Query(DhtRpcPacket query, NodeContact contact)
        {
            Stream s = null;

            try
            {
                if (contact.IsCurrentNode)
                    return ProcessQuery(query, contact.NodeEP);

                if (_currentNode.NodeEP.AddressFamily != contact.NodeEP.AddressFamily)
                {
                    contact.IncrementRpcFailCount();
                    return null;
                }

                s = _manager.GetConnection(contact.NodeEP);

                //set timeout
                s.WriteTimeout = _queryTimeout;
                s.ReadTimeout = _queryTimeout;

                //send query
                query.WriteTo(new BinaryWriter(s));
                s.Flush();

                Debug.Write(this.GetType().Name, "query sent to: " + contact.ToString());

                //read response
                DhtRpcPacket response = new DhtRpcPacket(new BinaryReader(s));

                Debug.Write(this.GetType().Name, "response received from: " + contact.ToString());

                //auto add contact or update last seen time
                {
                    NodeContact bucketContact = _routingTable.FindContact(contact.NodeId);

                    if (bucketContact == null)
                    {
                        contact.UpdateLastSeenTime();
                        _routingTable.AddContact(contact);
                    }
                    else
                    {
                        bucketContact.UpdateLastSeenTime();
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);

                contact.IncrementRpcFailCount();
                return null;
            }
            finally
            {
                if (s != null)
                    s.Dispose();
            }
        }

        internal bool Ping(NodeContact contact)
        {
            DhtRpcPacket response = Query(DhtRpcPacket.CreatePingPacket(_currentNode), contact);
            return (response != null);
        }

        private object QueryFind(NodeContact[] initialContacts, BinaryNumber nodeId, DhtRpcType queryType)
        {
            if (initialContacts.Length < 1)
                return null;

            List<NodeContact> seenContacts = new List<NodeContact>(initialContacts);
            List<NodeContact> learnedNotQueriedContacts = new List<NodeContact>(initialContacts);
            List<NodeContact> respondedContacts = new List<NodeContact>();
            List<EndPoint> receivedPeers = null;
            int alpha = KADEMLIA_ALPHA;
            bool finalRound = false;
            bool checkTerminationCondition = false;

            if (queryType == DhtRpcType.FIND_PEERS)
                receivedPeers = new List<EndPoint>();

            NodeContact previousClosestSeenContact = KBucket.SelectClosestContacts(seenContacts, nodeId, 1)[0];

            while (true)
            {
                NodeContact[] alphaContacts;

                //pick alpha contacts to query from learned contacts
                lock (learnedNotQueriedContacts)
                {
                    alphaContacts = KBucket.SelectClosestContacts(learnedNotQueriedContacts, nodeId, alpha);

                    //remove selected alpha contacts from learned not queries contacts list
                    foreach (NodeContact alphaContact in alphaContacts)
                        learnedNotQueriedContacts.Remove(alphaContact);
                }

                if (alphaContacts.Length < 1)
                {
                    checkTerminationCondition = true;
                }
                else
                {
                    object lockObj = new object();

                    lock (lockObj)
                    {
                        int respondedAlphaContacts = 0;

                        //query each alpha contact async
                        foreach (NodeContact alphaContact in alphaContacts)
                        {
                            ThreadPool.QueueUserWorkItem(delegate (object state)
                            {
                                DhtRpcPacket response;

                                if (queryType == DhtRpcType.FIND_NODE)
                                    response = Query(DhtRpcPacket.CreateFindNodePacketQuery(_currentNode, nodeId), alphaContact);
                                else
                                    response = Query(DhtRpcPacket.CreateFindPeersPacketQuery(_currentNode, nodeId), alphaContact);

                                if ((response == null) || (response.Type != queryType))
                                {
                                    //time out or error
                                    //ignore contact by removing from seen contacts list
                                    lock (seenContacts)
                                    {
                                        seenContacts.Remove(alphaContact);
                                    }
                                }
                                else
                                {
                                    //got reply!
                                    if ((queryType == DhtRpcType.FIND_PEERS) && (response.Peers.Length > 0))
                                    {
                                        lock (receivedPeers)
                                        {
                                            foreach (EndPoint peer in response.Peers)
                                            {
                                                if (!receivedPeers.Contains(peer))
                                                    receivedPeers.Add(peer);
                                            }
                                        }
                                    }

                                    //add alpha contact to responded contacts list
                                    lock (respondedContacts)
                                    {
                                        if (!respondedContacts.Contains(alphaContact))
                                            respondedContacts.Add(alphaContact);
                                    }

                                    //add received contacts to learned contacts list
                                    lock (seenContacts)
                                    {
                                        lock (learnedNotQueriedContacts)
                                        {
                                            foreach (NodeContact contact in response.Contacts)
                                            {
                                                if (!seenContacts.Contains(contact))
                                                {
                                                    seenContacts.Add(contact);
                                                    learnedNotQueriedContacts.Add(contact);
                                                }
                                            }
                                        }
                                    }
                                }

                                //wait for all alpha contacts to respond and signal only when all contacts responded
                                lock (lockObj)
                                {
                                    respondedAlphaContacts++;

                                    if (respondedAlphaContacts == alphaContacts.Length)
                                        Monitor.Pulse(lockObj);
                                }
                            });
                        }

                        //wait for any of the node contact to return new contacts
                        if (Monitor.Wait(lockObj, _queryTimeout))
                        {
                            //got reply or final round!

                            NodeContact currentClosestSeenContact;

                            lock (seenContacts)
                            {
                                currentClosestSeenContact = KBucket.SelectClosestContacts(seenContacts, nodeId, 1)[0];
                            }

                            BinaryNumber previousDistance = nodeId ^ previousClosestSeenContact.NodeId;
                            BinaryNumber currentDistance = nodeId ^ currentClosestSeenContact.NodeId;

                            if (previousDistance <= currentDistance)
                            {
                                //current round failed to return a node contact any closer than the closest already seen
                                if (finalRound)
                                {
                                    //final round over, check for termination condition
                                    checkTerminationCondition = true;
                                }
                                else
                                {
                                    //resend query to k closest node not already queried
                                    finalRound = true;
                                    alpha = KADEMLIA_K;
                                }
                            }
                            else
                            {
                                //current closest seen contact is closer than previous closest seen contact
                                previousClosestSeenContact = currentClosestSeenContact;
                                finalRound = false;
                                alpha = KADEMLIA_ALPHA;
                            }
                        }
                    }
                }

                if (checkTerminationCondition)
                {
                    checkTerminationCondition = false; //reset

                    if (queryType == DhtRpcType.FIND_PEERS)
                    {
                        //check only in final round to get most peers
                        lock (receivedPeers)
                        {
                            if (receivedPeers.Count > 0)
                                return receivedPeers.ToArray();

                            return null;
                        }
                    }

                    //lookup terminates when k closest seen contacts have responded
                    NodeContact[] kClosestSeenContacts;

                    lock (seenContacts)
                    {
                        kClosestSeenContacts = KBucket.SelectClosestContacts(seenContacts, nodeId, KADEMLIA_K);
                    }

                    lock (respondedContacts)
                    {
                        bool success = true;

                        foreach (NodeContact contact in kClosestSeenContacts)
                        {
                            if (!respondedContacts.Contains(contact))
                            {
                                success = false;
                                break;
                            }
                        }

                        if (success)
                            return kClosestSeenContacts;

                        if (alphaContacts.Length < 1)
                            return KBucket.SelectClosestContacts(respondedContacts, nodeId, KADEMLIA_K);
                    }
                }
            }
        }

        internal NodeContact[] QueryFindNode(NodeContact[] initialContacts, BinaryNumber nodeId)
        {
            object contacts = QueryFind(initialContacts, nodeId, DhtRpcType.FIND_NODE);

            if (contacts == null)
                return null;

            return contacts as NodeContact[];
        }

        private EndPoint[] QueryFindPeers(NodeContact[] initialContacts, BinaryNumber networkId)
        {
            object peers = QueryFind(initialContacts, networkId, DhtRpcType.FIND_PEERS);

            if (peers == null)
                return null;

            return peers as EndPoint[];
        }

        private EndPoint[] QueryAnnounce(NodeContact[] initialContacts, BinaryNumber networkId, EndPoint serviceEP)
        {
            NodeContact[] contacts = QueryFindNode(initialContacts, networkId);

            if ((contacts == null) || (contacts.Length == 0))
                return null;

            List<EndPoint> peers = new List<EndPoint>();

            lock (peers)
            {
                int respondedContacts = 0;

                foreach (NodeContact contact in contacts)
                {
                    ThreadPool.QueueUserWorkItem(delegate (object state)
                    {
                        DhtRpcPacket response = Query(DhtRpcPacket.CreateAnnouncePeerPacketQuery(_currentNode, networkId, serviceEP), contact);
                        if ((response != null) && (response.Type == DhtRpcType.ANNOUNCE_PEER) && (response.Peers.Length > 0))
                        {
                            lock (peers)
                            {
                                foreach (EndPoint peer in response.Peers)
                                {
                                    if (!peers.Contains(peer))
                                        peers.Add(peer);
                                }

                                respondedContacts++;

                                if (respondedContacts == contacts.Length)
                                    Monitor.Pulse(peers);
                            }
                        }
                    });
                }

                if (Monitor.Wait(peers, _queryTimeout))
                    return peers.ToArray();

                return null;
            }
        }

        #endregion

        #region public

        public void AcceptConnection(Stream s, EndPoint remoteNodeEP)
        {
            //set timeout
            s.WriteTimeout = _queryTimeout;
            s.ReadTimeout = _queryTimeout;

            BinaryReader bR = new BinaryReader(s);
            BinaryWriter bW = new BinaryWriter(s);

            DhtRpcPacket response = ProcessQuery(new DhtRpcPacket(bR), remoteNodeEP);
            if (response == null)
                return;

            response.WriteTo(bW);
            s.Flush();
        }

        public void AddNode(IEnumerable<NodeContact> contacts)
        {
            foreach (NodeContact contact in contacts)
                AddNode(contact);
        }

        public void AddNode(IEnumerable<EndPoint> nodeEPs)
        {
            foreach (EndPoint nodeEP in nodeEPs)
                AddNode(new NodeContact(nodeEP));
        }

        public void AddNode(EndPoint nodeEP)
        {
            AddNode(new NodeContact(nodeEP));
        }

        public void AddNode(NodeContact contact)
        {
            if (contact.NodeEP.AddressFamily != _currentNode.NodeEP.AddressFamily)
                return;

            if (_routingTable.AddContact(contact))
            {
                Debug.Write(this.GetType().Name, "node contact added: " + contact.ToString());

                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    Query(DhtRpcPacket.CreatePingPacket(_currentNode), contact);
                });
            }
        }

        public EndPoint[] FindPeers(BinaryNumber networkId)
        {
            NodeContact[] initialContacts = _routingTable.GetKClosestContacts(networkId, true);

            if (initialContacts.Length < 1)
                return null;

            return QueryFindPeers(initialContacts, networkId);
        }

        public EndPoint[] Announce(BinaryNumber networkId, EndPoint serviceEP)
        {
            NodeContact[] initialContacts = _routingTable.GetKClosestContacts(networkId, true);

            if (initialContacts.Length < 1)
                return null;

            return QueryAnnounce(initialContacts, networkId, serviceEP);
        }

        public EndPoint[] GetAllNodeEPs(bool includeStaleContacts, bool includeSelfContact)
        {
            NodeContact[] contacts = _routingTable.GetAllContacts(includeStaleContacts, includeSelfContact);
            EndPoint[] nodeEPs = new EndPoint[contacts.Length];

            for (int i = 0; i < contacts.Length; i++)
                nodeEPs[i] = contacts[i].NodeEP;

            return nodeEPs;
        }

        public EndPoint[] GetKRandomNodeEPs(bool includeSelfContact)
        {
            NodeContact[] contacts = _routingTable.GetKClosestContacts(BinaryNumber.GenerateRandomNumber256(), includeSelfContact);
            EndPoint[] nodeEPs = new EndPoint[contacts.Length];

            for (int i = 0; i < contacts.Length; i++)
                nodeEPs[i] = contacts[i].NodeEP;

            return nodeEPs;
        }

        public void ForceHealthCheck()
        {
            if (_healthTimer != null)
                _healthTimer.Change(1000, HEALTH_CHECK_TIMER_INTERVAL);
        }

        #endregion

        #region properties

        public BinaryNumber LocalNodeID
        { get { return _currentNode.NodeId; } }

        public EndPoint LocalNodeEP
        { get { return _currentNode.NodeEP; } }

        public int TotalNodes
        { get { return _routingTable.TotalContacts; } }

        public int QueryTimeout
        {
            get { return _queryTimeout; }
            set { _queryTimeout = value; }
        }

        #endregion

        class CurrentNode : NodeContact
        {
            #region variables

            const int MAX_PEERS_TO_RETURN = 30;

            readonly Dictionary<BinaryNumber, List<PeerEndPoint>> _data = new Dictionary<BinaryNumber, List<PeerEndPoint>>();

            #endregion

            #region constructor

            public CurrentNode(EndPoint nodeEP)
                : base(nodeEP)
            {
                _isCurrentNode = true;
            }

            public CurrentNode(BinaryNumber nodeId, EndPoint nodeEP)
                : base(nodeId, nodeEP)
            {
                _isCurrentNode = true;
            }

            #endregion

            #region public

            public void StorePeer(BinaryNumber networkId, EndPoint peerEP)
            {
                switch (peerEP.AddressFamily)
                {
                    case AddressFamily.Unspecified:
                        if (!(peerEP as DomainEndPoint).Address.EndsWith(".onion", StringComparison.OrdinalIgnoreCase)) //allow only .onion domain end points
                            return;

                        break;

                    case AddressFamily.InterNetwork:
                        if ((peerEP as IPEndPoint).Address.Equals(IPAddress.Any)) //avoid storing [0.0.0.0]
                            return;

                        if (NodeEP.AddressFamily != AddressFamily.InterNetwork) //avoid storing cross family peer end points
                            return;

                        break;

                    case AddressFamily.InterNetworkV6:
                        if ((peerEP as IPEndPoint).Address.Equals(IPAddress.IPv6Any)) //avoid storing [::]
                            return;

                        if (NodeEP.AddressFamily != AddressFamily.InterNetworkV6) //avoid storing cross family peer end points
                            return;

                        break;
                }

                List<PeerEndPoint> peerList;

                lock (_data)
                {
                    if (_data.ContainsKey(networkId))
                    {
                        peerList = _data[networkId];
                    }
                    else
                    {
                        peerList = new List<PeerEndPoint>();
                        _data.Add(networkId, peerList);
                    }
                }

                lock (peerList)
                {
                    foreach (PeerEndPoint peer in peerList)
                    {
                        if (peer.EndPoint.Equals(peerEP))
                        {
                            peer.UpdateDateAdded();
                            return;
                        }
                    }

                    peerList.Add(new PeerEndPoint(peerEP));
                }
            }

            public EndPoint[] GetPeers(BinaryNumber networkId)
            {
                List<PeerEndPoint> peers;

                lock (_data)
                {
                    if (_data.ContainsKey(networkId))
                    {
                        peers = _data[networkId];
                    }
                    else
                    {
                        return new EndPoint[] { };
                    }
                }

                List<PeerEndPoint> finalPeers;

                lock (peers)
                {
                    if (peers.Count > MAX_PEERS_TO_RETURN)
                    {
                        finalPeers = new List<PeerEndPoint>(peers);
                        Random rnd = new Random(DateTime.UtcNow.Millisecond);

                        while (finalPeers.Count > MAX_PEERS_TO_RETURN)
                        {
                            finalPeers.RemoveAt(rnd.Next(finalPeers.Count - 1));
                        }
                    }
                    else
                    {
                        finalPeers = _data[networkId];
                    }
                }

                EndPoint[] finalEPs = new EndPoint[finalPeers.Count];
                int i = 0;

                foreach (PeerEndPoint peer in finalPeers)
                    finalEPs[i++] = peer.EndPoint;

                return finalEPs;
            }

            public void RemoveExpiredPeers()
            {
                lock (_data)
                {
                    List<PeerEndPoint> expiredPeers = new List<PeerEndPoint>();

                    foreach (List<PeerEndPoint> peerList in _data.Values)
                    {
                        foreach (PeerEndPoint peer in peerList)
                        {
                            if (peer.HasExpired())
                                expiredPeers.Add(peer);
                        }

                        foreach (PeerEndPoint expiredPeer in expiredPeers)
                        {
                            peerList.Remove(expiredPeer);
                        }

                        expiredPeers.Clear();
                    }
                }
            }

            #endregion

            class PeerEndPoint
            {
                #region variables

                const int PEER_EXPIRY_TIME_SECONDS = 900; //15 min expiry

                EndPoint _endPoint;

                DateTime _dateAdded = DateTime.UtcNow;

                #endregion

                #region constructor

                public PeerEndPoint(EndPoint endPoint)
                {
                    _endPoint = endPoint;

                    if (_endPoint.AddressFamily == AddressFamily.InterNetworkV6)
                        (_endPoint as IPEndPoint).Address.ScopeId = 0;
                }

                #endregion

                #region public

                public bool HasExpired()
                {
                    return (DateTime.UtcNow - _dateAdded).TotalSeconds > PEER_EXPIRY_TIME_SECONDS;
                }

                public void UpdateDateAdded()
                {
                    _dateAdded = DateTime.UtcNow;
                }

                public override string ToString()
                {
                    return _endPoint.ToString();
                }

                #endregion

                #region properties

                public EndPoint EndPoint
                { get { return _endPoint; } }

                #endregion
            }
        }
    }
}
