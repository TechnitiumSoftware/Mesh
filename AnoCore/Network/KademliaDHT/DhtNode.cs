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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

/*
 * Kademlia based Distributed Hash Table (DHT) Implementation For Bit Chat
 * =======================================================================
 *
 * FEATURES IMPLEMENTED
 * --------------------
 * 1. Routing table with K-Bucket: K=8, bucket refresh, contact health check, additional "replacement" list of upto K contacts.
 * 2. RPC: TCP based protocol with PING & FIND_NODE implemented. FIND_PEER & ANNOUNCE_PEER implemented similar to BitTorrent DHT implementation.
 * 3. Peer data eviction after 15 mins of receiving announcement.
 * 4. Parallel lookup: FIND_NODE lookup with alpha=3 implemented.
 * 
 * FEATURES NOT IMPLEMENTED
 * ------------------------
 * 1. Node data republishing. Each peer MUST announce itself within 15 mins to all nodes closer to bit chat networkID.
 * 
 * REFERENCE
 * ---------
 * 1. https://pdos.csail.mit.edu/~petar/papers/maymounkov-kademlia-lncs.pdf
 * 2. http://www.bittorrent.org/beps/bep_0005.html
*/

namespace BitChatCore.Network.KademliaDHT
{
    class DhtNode : IDisposable
    {
        #region variables

        public const int KADEMLIA_K = 8;
        internal const int KADEMLIA_B = 5;
        const int QUERY_TIMEOUT = 5000;
        const int HEALTH_CHECK_TIMER_INITIAL_INTERVAL = 30 * 1000; //30 sec
        const int HEALTH_CHECK_TIMER_INTERVAL = 15 * 60 * 1000; //15 min
        const int KADEMLIA_ALPHA = 3;

        readonly IDhtNodeManager _manager;

        CurrentNode _currentNode;
        KBucket _routingTable;

        Timer _healthTimer;

        #endregion

        #region constructor

        public DhtNode(IPEndPoint localNodeEP, IDhtNodeManager manager)
        {
            _manager = manager;

            //init routing table
            _currentNode = new CurrentNode(localNodeEP);
            _routingTable = new KBucket(_currentNode);

            //start health timer
            _healthTimer = new Timer(HealthTimerCallback, null, HEALTH_CHECK_TIMER_INITIAL_INTERVAL, Timeout.Infinite);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_healthTimer != null)
                    {
                        _healthTimer.Dispose();
                        _healthTimer = null;
                    }
                }

                _disposed = true;
            }
        }

        #endregion

        #region private

        private void HealthTimerCallback(object state)
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
                NodeContact[] initialContacts = _routingTable.GetKClosestContacts(_currentNode.NodeID);

                if (initialContacts.Length > 0)
                    QueryFindNode(initialContacts, _currentNode.NodeID); //query manager auto add contacts that respond
            }
            catch
            { }
            finally
            {
                if (_healthTimer != null)
                    _healthTimer.Change(HEALTH_CHECK_TIMER_INTERVAL, Timeout.Infinite);
            }
        }

        private DhtRpcPacket ProcessQuery(DhtRpcPacket query, IPAddress remoteNodeIP)
        {
            AddNode(new IPEndPoint(remoteNodeIP, query.SourceNodePort));

            switch (query.Type)
            {
                case DhtRpcType.PING:
                    return DhtRpcPacket.CreatePingPacket(_currentNode);

                case DhtRpcType.FIND_NODE:
                    return DhtRpcPacket.CreateFindNodePacketResponse(_currentNode, query.NetworkID, _routingTable.GetKClosestContacts(query.NetworkID));

                case DhtRpcType.FIND_PEERS:
                    PeerEndPoint[] peers = _currentNode.GetPeers(query.NetworkID);
                    if (peers.Length == 0)
                        return DhtRpcPacket.CreateFindPeersPacketResponse(_currentNode, query.NetworkID, _routingTable.GetKClosestContacts(query.NetworkID), peers);
                    else
                        return DhtRpcPacket.CreateFindPeersPacketResponse(_currentNode, query.NetworkID, new NodeContact[] { }, peers);

                case DhtRpcType.ANNOUNCE_PEER:
                    _currentNode.StorePeer(query.NetworkID, new PeerEndPoint(remoteNodeIP, query.ServicePort));
                    return DhtRpcPacket.CreateAnnouncePeerPacketResponse(_currentNode, query.NetworkID, _currentNode.GetPeers(query.NetworkID));

                default:
                    throw new Exception("Invalid DHT-RPC type.");
            }
        }

        private DhtRpcPacket Query(DhtRpcPacket query, NodeContact contact)
        {
            if (_currentNode.NodeEP.AddressFamily != contact.NodeEP.AddressFamily)
                return null;

            Stream s = null;

            try
            {
                s = _manager.GetConnectionStream(contact.NodeEP);

                //set timeout
                s.WriteTimeout = QUERY_TIMEOUT;
                s.ReadTimeout = QUERY_TIMEOUT;

                //send query
                query.WriteTo(s);
                s.Flush();

                //read response
                DhtRpcPacket response = new DhtRpcPacket(s);

                //auto add contact or update last seen time
                {
                    NodeContact bucketContact = _routingTable.FindContact(contact.NodeID);

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
            catch
            {
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

        private object QueryFind(NodeContact[] initialContacts, BinaryNumber nodeID, DhtRpcType queryType)
        {
            if (initialContacts.Length < 1)
                return null;

            List<NodeContact> seenContacts = new List<NodeContact>(initialContacts);
            List<NodeContact> learnedNotQueriedContacts = new List<NodeContact>(initialContacts);
            List<NodeContact> respondedContacts = new List<NodeContact>();
            List<PeerEndPoint> receivedPeers = null;
            int alpha = KADEMLIA_ALPHA;
            bool finalRound = false;
            bool checkTerminationCondition = false;

            if (queryType == DhtRpcType.FIND_PEERS)
                receivedPeers = new List<PeerEndPoint>();

            NodeContact previousClosestSeenContact = KBucket.SelectClosestContacts(seenContacts, nodeID, 1)[0];

            while (true)
            {
                NodeContact[] alphaContacts;

                //pick alpha contacts to query from learned contacts
                lock (learnedNotQueriedContacts)
                {
                    alphaContacts = KBucket.SelectClosestContacts(learnedNotQueriedContacts, nodeID, alpha);

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
                        //query each alpha contact async
                        foreach (NodeContact alphaContact in alphaContacts)
                        {
                            Thread t = new Thread(delegate (object state)
                            {
                                DhtRpcPacket response;

                                if (queryType == DhtRpcType.FIND_NODE)
                                    response = Query(DhtRpcPacket.CreateFindNodePacketQuery(_currentNode, nodeID), alphaContact);
                                else
                                    response = Query(DhtRpcPacket.CreateFindPeersPacketQuery(_currentNode, nodeID), alphaContact);

                                if ((response == null) || (response.Type != queryType))
                                {
                                    //time out or error
                                    //ignore contact by removing from seen contacts list
                                    lock (seenContacts)
                                    {
                                        seenContacts.Remove(alphaContact);
                                    }

                                    return;
                                }

                                //got reply!
                                if ((queryType == DhtRpcType.FIND_PEERS) && (response.Peers.Length > 0))
                                {
                                    lock (receivedPeers)
                                    {
                                        foreach (PeerEndPoint peer in response.Peers)
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

                                //no pulse for final round to wait for all k contacts to respond. this allows any failed node contact to be removed from seen contacts list during the wait.
                                lock (lockObj)
                                {
                                    Monitor.Pulse(lockObj);
                                }
                            });

                            t.IsBackground = true;
                            t.Start();
                        }

                        //wait for any of the node contact to return new contacts
                        if (Monitor.Wait(lockObj, QUERY_TIMEOUT))
                        {
                            //got reply or final round!

                            NodeContact currentClosestSeenContact;

                            lock (seenContacts)
                            {
                                currentClosestSeenContact = KBucket.SelectClosestContacts(seenContacts, nodeID, 1)[0];
                            }

                            BinaryNumber previousDistance = nodeID ^ previousClosestSeenContact.NodeID;
                            BinaryNumber currentDistance = nodeID ^ currentClosestSeenContact.NodeID;

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
                        kClosestSeenContacts = KBucket.SelectClosestContacts(seenContacts, nodeID, KADEMLIA_K);
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
                            return KBucket.SelectClosestContacts(respondedContacts, nodeID, KADEMLIA_K);
                    }
                }
            }
        }

        internal NodeContact[] QueryFindNode(NodeContact[] initialContacts, BinaryNumber nodeID)
        {
            object contacts = QueryFind(initialContacts, nodeID, DhtRpcType.FIND_NODE);

            if (contacts == null)
                return null;

            return contacts as NodeContact[];
        }

        private PeerEndPoint[] QueryFindPeers(NodeContact[] initialContacts, BinaryNumber networkID)
        {
            object peers = QueryFind(initialContacts, networkID, DhtRpcType.FIND_PEERS);

            if (peers == null)
                return null;

            return peers as PeerEndPoint[];
        }

        private PeerEndPoint[] QueryAnnounce(NodeContact[] initialContacts, BinaryNumber networkID, ushort servicePort)
        {
            NodeContact[] contacts = QueryFindNode(initialContacts, networkID);

            if ((contacts == null) || (contacts.Length == 0))
                return null;

            List<PeerEndPoint> peers = new List<PeerEndPoint>();

            lock (peers)
            {
                foreach (NodeContact contact in contacts)
                {
                    Thread t = new Thread(delegate (object state)
                    {
                        DhtRpcPacket response = Query(DhtRpcPacket.CreateAnnouncePeerPacketQuery(_currentNode, networkID, servicePort), contact);
                        if ((response != null) && (response.Type == DhtRpcType.ANNOUNCE_PEER) && (response.Peers.Length > 0))
                        {
                            lock (peers)
                            {
                                foreach (PeerEndPoint peer in response.Peers)
                                {
                                    if (!peers.Contains(peer))
                                        peers.Add(peer);
                                }

                                Monitor.Pulse(peers);
                            }
                        }
                    });

                    t.IsBackground = true;
                    t.Start();
                }

                if (Monitor.Wait(peers, QUERY_TIMEOUT))
                    return peers.ToArray();

                return null;
            }
        }

        #endregion

        #region public

        public bool UpdateLocalNodeEP(IPEndPoint localNodeEP)
        {
            if ((localNodeEP != null) && (_currentNode.NodeEP.AddressFamily == localNodeEP.AddressFamily) && !_currentNode.NodeEP.Equals(localNodeEP))
            {
                NodeContact[] existingContacts = _routingTable.GetAllContacts(true);

                _currentNode = new CurrentNode(localNodeEP);
                _routingTable = new KBucket(_currentNode);

                AddNode(existingContacts);
                return true;
            }

            return false;
        }

        public void AcceptConnection(Stream s, IPAddress remoteNodeIP)
        {
            //set timeout
            s.WriteTimeout = QUERY_TIMEOUT;
            s.ReadTimeout = QUERY_TIMEOUT;

            while (true)
            {
                DhtRpcPacket response = ProcessQuery(new DhtRpcPacket(s), remoteNodeIP);
                if (response == null)
                    break;

                response.WriteTo(s);
                s.Flush();
            }
        }

        public void AddNode(IEnumerable<NodeContact> contacts)
        {
            foreach (NodeContact contact in contacts)
                AddNode(contact);
        }

        public void AddNode(IEnumerable<IPEndPoint> nodeEPs)
        {
            foreach (IPEndPoint nodeEP in nodeEPs)
                AddNode(new NodeContact(nodeEP));
        }

        public void AddNode(IPEndPoint nodeEP)
        {
            AddNode(new NodeContact(nodeEP));
        }

        public void AddNode(NodeContact contact)
        {
            if (!NetUtilities.IsPrivateIP(contact.NodeEP.Address) && (contact.NodeEP.AddressFamily == _currentNode.NodeEP.AddressFamily))
            {
                if (_routingTable.AddContact(contact))
                {
                    ThreadPool.QueueUserWorkItem(delegate (object state)
                    {
                        Query(DhtRpcPacket.CreatePingPacket(_currentNode), contact);
                    });
                }
            }
        }

        public IPEndPoint[] FindPeers(BinaryNumber networkID)
        {
            NodeContact[] initialContacts = _routingTable.GetKClosestContacts(networkID);

            if (initialContacts.Length < 1)
                return null;

            return QueryFindPeers(initialContacts, networkID);
        }

        public IPEndPoint[] Announce(BinaryNumber networkID, int servicePort)
        {
            NodeContact[] initialContacts = _routingTable.GetKClosestContacts(networkID);

            if (initialContacts.Length < 1)
                return null;

            return QueryAnnounce(initialContacts, networkID, Convert.ToUInt16(servicePort));
        }

        public IPEndPoint[] GetAllNodeEPs(bool includeStaleContacts)
        {
            NodeContact[] contacts = _routingTable.GetAllContacts(includeStaleContacts);
            IPEndPoint[] nodeEPs = new IPEndPoint[contacts.Length];

            for (int i = 0; i < contacts.Length; i++)
                nodeEPs[i] = contacts[i].NodeEP;

            return nodeEPs;
        }

        public IPEndPoint[] GetKClosestRandomNodeEPs()
        {
            NodeContact[] contacts = _routingTable.GetKClosestContacts(BinaryNumber.GenerateRandomNumber160());
            IPEndPoint[] nodeEPs = new IPEndPoint[contacts.Length];

            for (int i = 0; i < contacts.Length; i++)
                nodeEPs[i] = contacts[i].NodeEP;

            return nodeEPs;
        }

        public void ForceHealthCheck()
        {
            if (_healthTimer != null)
                _healthTimer.Change(1000, Timeout.Infinite);
        }

        #endregion

        #region properties

        public BinaryNumber LocalNodeID
        { get { return _currentNode.NodeID; } }

        public IPEndPoint LocalNodeEP
        { get { return _currentNode.NodeEP; } }

        public int TotalNodes
        { get { return _routingTable.TotalContacts; } }

        #endregion

        class CurrentNode : NodeContact
        {
            #region variables

            const int MAX_PEERS_TO_RETURN = 30;

            readonly Dictionary<BinaryNumber, List<PeerEndPoint>> _data = new Dictionary<BinaryNumber, List<PeerEndPoint>>();

            #endregion

            #region constructor

            public CurrentNode(IPEndPoint localNodeEP)
                : base(localNodeEP)
            {
                _currentNode = true;
            }

            #endregion

            #region public

            public void StorePeer(BinaryNumber networkID, PeerEndPoint peerEP)
            {
                lock (_data)
                {
                    List<PeerEndPoint> peerList;

                    if (_data.ContainsKey(networkID))
                    {
                        peerList = _data[networkID];
                    }
                    else
                    {
                        peerList = new List<PeerEndPoint>();
                        _data.Add(networkID, peerList);
                    }

                    foreach (PeerEndPoint peer in peerList)
                    {
                        if (peer.Equals(peerEP))
                        {
                            peer.UpdateDateAdded();
                            return;
                        }
                    }

                    peerList.Add(peerEP);
                }
            }

            public PeerEndPoint[] GetPeers(BinaryNumber networkID)
            {
                lock (_data)
                {
                    if (_data.ContainsKey(networkID))
                    {
                        List<PeerEndPoint> peers = _data[networkID];

                        if (peers.Count > MAX_PEERS_TO_RETURN)
                        {
                            List<PeerEndPoint> finalPeers = new List<PeerEndPoint>(peers);
                            Random rnd = new Random(DateTime.UtcNow.Millisecond);

                            while (finalPeers.Count > MAX_PEERS_TO_RETURN)
                            {
                                finalPeers.RemoveAt(rnd.Next(finalPeers.Count - 1));
                            }

                            return finalPeers.ToArray();
                        }
                        else
                        {
                            return _data[networkID].ToArray();
                        }
                    }
                    else
                    {
                        return new PeerEndPoint[] { };
                    }
                }
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
        }
    }
}
