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
using System.Threading;
using TechnitiumLibrary.IO;

namespace BitChatCore.Network.KademliaDHT
{
    class KBucket
    {
        #region variables

        const int BUCKET_STALE_TIMEOUT_SECONDS = 900; //15 mins timeout before declaring node stale

        readonly BinaryNumber _bucketID;
        readonly int _bucketDepth;
        DateTime _lastChanged;

        readonly KBucket _parentBucket;

        volatile NodeContact[] _contacts;
        volatile int _contactCount;

        volatile KBucket _leftBucket;
        volatile KBucket _rightBucket;

        #endregion

        #region constructor

        public KBucket(NodeContact currentNode)
        {
            _bucketDepth = 0;

            _contacts = new NodeContact[DhtNode.KADEMLIA_K * 2];

            _contacts[0] = currentNode;
            _contactCount = 1;
            _lastChanged = DateTime.UtcNow;
        }

        private KBucket(KBucket parentBucket, bool left)
        {
            _bucketDepth = parentBucket._bucketDepth + 1;

            _parentBucket = parentBucket;

            _contacts = new NodeContact[DhtNode.KADEMLIA_K * 2];

            if (parentBucket._bucketID == null)
            {
                _bucketID = new BinaryNumber(new byte[20]);

                if (left)
                    _bucketID.Number[0] = 0x80;
            }
            else
            {
                if (left)
                {
                    _bucketID = new BinaryNumber(new byte[20]);
                    _bucketID.Number[0] = 0x80;

                    _bucketID = parentBucket._bucketID | (_bucketID >> (_bucketDepth - 1));
                }
                else
                {
                    _bucketID = parentBucket._bucketID;
                }
            }

            _lastChanged = DateTime.UtcNow;
        }

        #endregion

        #region static

        public static NodeContact[] SelectClosestContacts(ICollection<NodeContact> contacts, BinaryNumber nodeID, int count)
        {
            if (contacts.Count < count)
                count = contacts.Count;

            NodeContact[] closestContacts = new NodeContact[count];
            BinaryNumber[] closestContactDistances = new BinaryNumber[count];

            foreach (NodeContact contact in contacts)
            {
                BinaryNumber distance = nodeID ^ contact.NodeID;

                for (int i = 0; i < count; i++)
                {
                    if ((closestContactDistances[i] == null) || (distance < closestContactDistances[i]))
                    {
                        //demote existing values
                        for (int j = count - 1; j > i; j--)
                        {
                            closestContactDistances[j] = closestContactDistances[j - 1];
                            closestContacts[j] = closestContacts[j - 1];
                        }

                        //place current on top
                        closestContactDistances[i] = distance;
                        closestContacts[i] = contact;
                        break;
                    }
                }
            }

            return closestContacts;
        }

        private static void SplitBucket(KBucket bucket, NodeContact newContact)
        {
            if (bucket._contacts == null)
                return;

            KBucket leftBucket = new KBucket(bucket, true);
            KBucket rightBucket = new KBucket(bucket, false);

            foreach (NodeContact contact in bucket._contacts)
            {
                if (contact != null)
                {
                    if ((leftBucket._bucketID & contact.NodeID) == leftBucket._bucketID)
                        leftBucket._contacts[leftBucket._contactCount++] = contact;
                    else
                        rightBucket._contacts[rightBucket._contactCount++] = contact;
                }
            }

            KBucket selectedBucket;

            if ((leftBucket._bucketID & newContact.NodeID) == leftBucket._bucketID)
                selectedBucket = leftBucket;
            else
                selectedBucket = rightBucket;

            if (selectedBucket._contactCount == selectedBucket._contacts.Length)
            {
                SplitBucket(selectedBucket, newContact);

                selectedBucket._contactCount++;
            }
            else
            {
                selectedBucket._contacts[selectedBucket._contactCount++] = newContact;
            }

            bucket._contacts = null;
            bucket._leftBucket = leftBucket;
            bucket._rightBucket = rightBucket;
        }

        #endregion

        #region private

        private List<KBucket> GetAllLeafKBuckets()
        {
            List<KBucket> allLeafKBuckets = new List<KBucket>();

            KBucket currentBucket = this;

            while (true)
            {
                NodeContact[] contacts = currentBucket._contacts;
                KBucket leftBucket = currentBucket._leftBucket;
                KBucket rightBucket = currentBucket._rightBucket;

                if (contacts != null)
                {
                    allLeafKBuckets.Add(currentBucket);

                    while (true)
                    {
                        if (currentBucket == this)
                            return allLeafKBuckets;

                        if (currentBucket == currentBucket._parentBucket._leftBucket)
                        {
                            currentBucket = currentBucket._parentBucket._rightBucket;
                            break;
                        }
                        else
                        {
                            currentBucket = currentBucket._parentBucket;
                        }
                    }
                }
                else
                {
                    currentBucket = leftBucket;
                }
            }
        }

        #endregion

        #region public

        public bool AddContact(NodeContact contact)
        {
            KBucket currentBucket = this;

            while (true)
            {
                NodeContact[] contacts;
                KBucket leftBucket;
                KBucket rightBucket;

                lock (currentBucket)
                {
                    contacts = currentBucket._contacts;
                    leftBucket = currentBucket._leftBucket;
                    rightBucket = currentBucket._rightBucket;

                    if (contacts != null)
                    {
                        #region add contact in this bucket

                        //search if contact already exists
                        for (int i = 0; i < contacts.Length; i++)
                        {
                            if (contact.Equals(contacts[i]))
                                return false; //contact already exists
                        }

                        //try add contact
                        for (int i = 0; i < contacts.Length; i++)
                        {
                            if (contacts[i] == null)
                            {
                                contacts[i] = contact;
                                currentBucket._lastChanged = DateTime.UtcNow;

                                KBucket bucket = currentBucket;
                                do
                                {
                                    Interlocked.Increment(ref bucket._contactCount);
                                    bucket = bucket._parentBucket;
                                }
                                while (bucket != null);

                                return true;
                            }
                        }

                        //k-bucket is full so contact was not added

                        //if current contact is not stale then find and replace with any existing stale contact
                        if (!contact.IsStale())
                        {
                            for (int i = 0; i < contacts.Length; i++)
                            {
                                if (contacts[i].IsStale())
                                {
                                    contacts[i] = contact;
                                    currentBucket._lastChanged = DateTime.UtcNow;

                                    KBucket bucket = currentBucket;
                                    do
                                    {
                                        Interlocked.Increment(ref bucket._contactCount);
                                        bucket = bucket._parentBucket;
                                    }
                                    while (bucket != null);

                                    return true;
                                }
                            }
                        }

                        //no stale contact in this k-bucket to replace!
                        if (contacts[0].IsCurrentNode || (currentBucket._bucketDepth < (DhtNode.KADEMLIA_B - 1)))
                        {
                            //split current bucket and add contact!
                            SplitBucket(currentBucket, contact);

                            KBucket bucket = currentBucket;
                            do
                            {
                                Interlocked.Increment(ref bucket._contactCount);
                                bucket = bucket._parentBucket;
                            }
                            while (bucket != null);

                            return true;
                        }

                        //k-bucket is full!
                        return false;

                        #endregion
                    }
                }

                if ((leftBucket._bucketID & contact.NodeID) == leftBucket._bucketID)
                    currentBucket = leftBucket;
                else
                    currentBucket = rightBucket;
            }
        }

        public bool RemoveContact(NodeContact contact)
        {
            KBucket currentBucket = this;

            while (true)
            {
                NodeContact[] contacts;
                KBucket leftBucket;
                KBucket rightBucket;

                lock (currentBucket)
                {
                    contacts = currentBucket._contacts;
                    leftBucket = currentBucket._leftBucket;
                    rightBucket = currentBucket._rightBucket;

                    if (contacts != null)
                    {
                        #region remove contact from this bucket

                        if (currentBucket._contactCount <= DhtNode.KADEMLIA_K)
                            return false; //k-bucket is not full and replacement cache is empty

                        for (int i = 0; i < contacts.Length; i++)
                        {
                            if (contact.Equals(contacts[i]))
                            {
                                if (contacts[i].IsStale())
                                {
                                    //remove stale contact
                                    contacts[i] = null;
                                    currentBucket._lastChanged = DateTime.UtcNow;

                                    KBucket bucket = currentBucket;
                                    do
                                    {
                                        Interlocked.Decrement(ref bucket._contactCount);
                                        bucket = bucket._parentBucket;
                                    }
                                    while (bucket != null);

                                    return true;
                                }

                                break;
                            }
                        }

                        return false; //contact was not found or was not stale

                        #endregion
                    }
                }

                if ((leftBucket._bucketID & contact.NodeID) == leftBucket._bucketID)
                    currentBucket = leftBucket;
                else
                    currentBucket = rightBucket;
            }
        }

        public NodeContact[] GetKClosestContacts(BinaryNumber nodeID)
        {
            KBucket currentBucket = this;

            while (true)
            {
                NodeContact[] contacts = currentBucket._contacts;
                KBucket leftBucket = currentBucket._leftBucket;
                KBucket rightBucket = currentBucket._rightBucket;

                if (contacts != null)
                {
                    #region find closest contacts from this bucket

                    KBucket closestBucket = currentBucket;
                    NodeContact[] closestContacts = null;

                    if (closestBucket._contactCount >= DhtNode.KADEMLIA_K)
                    {
                        closestContacts = closestBucket.GetAllContacts(false);

                        if (closestContacts.Length > DhtNode.KADEMLIA_K)
                            return SelectClosestContacts(closestContacts, nodeID, DhtNode.KADEMLIA_K);

                        if (closestContacts.Length == DhtNode.KADEMLIA_K)
                            return closestContacts;

                        if (closestBucket._parentBucket == null)
                            return closestContacts;
                    }

                    while (closestBucket._parentBucket != null)
                    {
                        KBucket parentBucket = closestBucket._parentBucket;

                        closestContacts = parentBucket.GetAllContacts(false);

                        if (closestContacts.Length > DhtNode.KADEMLIA_K)
                            return SelectClosestContacts(closestContacts, nodeID, DhtNode.KADEMLIA_K);

                        if (closestContacts.Length == DhtNode.KADEMLIA_K)
                            return closestContacts;

                        closestBucket = parentBucket;
                    }

                    if (closestContacts == null)
                        closestContacts = closestBucket.GetAllContacts(false);

                    return closestContacts;

                    #endregion
                }

                if ((leftBucket._bucketID & nodeID) == leftBucket._bucketID)
                    currentBucket = leftBucket;
                else
                    currentBucket = rightBucket;
            }
        }

        public NodeContact[] GetAllContacts(bool includeStaleContacts)
        {
            List<NodeContact> allContacts = new List<NodeContact>();
            List<KBucket> allLeafKBuckets = GetAllLeafKBuckets();

            foreach (KBucket kBucket in allLeafKBuckets)
            {
                NodeContact[] contacts = kBucket._contacts;

                if (contacts != null)
                {
                    foreach (NodeContact contact in contacts)
                    {
                        if (contact != null)
                        {
                            if ((includeStaleContacts || !contact.IsStale()) && !contact.IsCurrentNode)
                                allContacts.Add(contact);
                        }
                    }
                }
            }

            return allContacts.ToArray();
        }

        public NodeContact FindContact(BinaryNumber nodeID)
        {
            KBucket currentBucket = this;

            while (true)
            {
                NodeContact[] contacts = currentBucket._contacts;
                KBucket leftBucket = currentBucket._leftBucket;
                KBucket rightBucket = currentBucket._rightBucket;

                if (contacts != null)
                {
                    foreach (NodeContact contact in contacts)
                    {
                        if ((contact != null) && contact.NodeID.Equals(nodeID))
                            return contact;
                    }

                    return null; //contact not found
                }

                if ((leftBucket._bucketID & nodeID) == leftBucket._bucketID)
                    currentBucket = leftBucket;
                else
                    currentBucket = rightBucket;
            }
        }

        internal void CheckContactHealth(DhtNode dhtNode)
        {
            List<KBucket> allLeafKBuckets = GetAllLeafKBuckets();

            foreach (KBucket kBucket in allLeafKBuckets)
            {
                NodeContact[] contacts = kBucket._contacts;

                if (contacts != null)
                {
                    foreach (NodeContact contact in contacts)
                    {
                        if ((contact != null) && contact.IsStale())
                        {
                            ThreadPool.QueueUserWorkItem(delegate (object state)
                            {
                                if (!dhtNode.Ping(contact))
                                {
                                    //remove stale node contact
                                    kBucket.RemoveContact(contact);
                                }
                            });
                        }
                    }
                }
            }
        }

        internal void RefreshBucket(DhtNode dhtNode)
        {
            List<KBucket> allLeafKBuckets = GetAllLeafKBuckets();

            foreach (KBucket kBucket in allLeafKBuckets)
            {
                if (kBucket._contacts != null)
                {
                    if ((DateTime.UtcNow - kBucket._lastChanged).TotalSeconds > BUCKET_STALE_TIMEOUT_SECONDS)
                    {
                        ThreadPool.QueueUserWorkItem(delegate (object state)
                        {
                            //get random node ID in the bucket range
                            BinaryNumber randomNodeID = BinaryNumber.GenerateRandomNumber160();

                            if (kBucket._bucketID != null)
                                randomNodeID = (randomNodeID >> kBucket._bucketDepth) | kBucket._bucketID;

                            //find closest contacts for current node id
                            NodeContact[] initialContacts = kBucket.GetKClosestContacts(randomNodeID);

                            if (initialContacts.Length > 0)
                                dhtNode.QueryFindNode(initialContacts, randomNodeID);
                        });
                    }
                }
            }
        }

        #endregion

        #region properties

        public int TotalContacts
        { get { return _contactCount; } }

        #endregion
    }
}
