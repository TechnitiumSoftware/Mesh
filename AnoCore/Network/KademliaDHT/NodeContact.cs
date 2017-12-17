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
using TechnitiumLibrary.Net;

namespace BitChatCore.Network.KademliaDHT
{
    class NodeContact : IWriteStream, IComparable<NodeContact>
    {
        #region variables

        const int NODE_RPC_FAIL_LIMIT = 5; //max failed RPC count before declaring node stale
        const int NODE_STALE_TIMEOUT_SECONDS = 900; //15mins timeout before declaring node stale

        static readonly byte[] NODE_ID_SALT = new byte[] { 0xF4, 0xC7, 0x56, 0x9A, 0xA3, 0xAD, 0xC9, 0xA7, 0x13, 0x0E, 0xCA, 0x56, 0x56, 0xA3, 0x52, 0x8F, 0xFE, 0x6E, 0x9C, 0x72 };

        readonly IPEndPoint _nodeEP;
        readonly BinaryNumber _nodeID;

        protected bool _currentNode;
        DateTime _lastSeen;
        int _failRpcCount = 0;

        #endregion

        #region constructor

        public NodeContact(Stream s)
        {
            _nodeEP = IPEndPointParser.Parse(s);
            _nodeID = GetNodeID(_nodeEP);
        }

        public NodeContact(IPEndPoint nodeEP)
        {
            _nodeEP = nodeEP;
            _nodeID = GetNodeID(_nodeEP);
        }

        #endregion

        #region static

        public static BinaryNumber GetNodeID(IPEndPoint nodeEP)
        {
            using (HMAC hmac = new HMACSHA1(NODE_ID_SALT))
            {
                using (MemoryStream mS = new MemoryStream(20))
                {
                    IPEndPointParser.WriteTo(nodeEP, mS);
                    mS.Position = 0;

                    return new BinaryNumber(hmac.ComputeHash(mS));
                }
            }
        }

        #endregion

        #region public

        public bool IsStale()
        {
            if (_currentNode)
                return false;
            else
                return ((_failRpcCount > NODE_RPC_FAIL_LIMIT) || ((DateTime.UtcNow - _lastSeen).TotalSeconds > NODE_STALE_TIMEOUT_SECONDS));
        }

        public void UpdateLastSeenTime()
        {
            _lastSeen = DateTime.UtcNow;
            _failRpcCount = 0;
        }

        public void IncrementRpcFailCount()
        {
            _failRpcCount++;
        }

        public void WriteTo(Stream s)
        {
            IPEndPointParser.WriteTo(_nodeEP, s);
        }

        public override bool Equals(object obj)
        {
            NodeContact contact = obj as NodeContact;

            if (contact == null)
                return false;

            return Equals(_nodeID, contact._nodeID);
        }

        public override int GetHashCode()
        {
            return _nodeID.GetHashCode();
        }

        public int CompareTo(NodeContact other)
        {
            return _lastSeen.CompareTo(other._lastSeen);
        }

        public override string ToString()
        {
            return _nodeEP.ToString();
        }

        #endregion

        #region properties

        public BinaryNumber NodeID
        { get { return _nodeID; } }

        public IPEndPoint NodeEP
        { get { return _nodeEP; } }

        public bool IsCurrentNode
        { get { return _currentNode; } }

        public DateTime LastSeen
        { get { return _lastSeen; } }

        #endregion
    }
}
