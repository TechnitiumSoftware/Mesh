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

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

namespace MeshCore.Network.DHT
{
    class NodeContact
    {
        #region variables

        const int NODE_RPC_FAIL_LIMIT = 5; //max failed RPC count before declaring node stale
        const int NODE_STALE_TIMEOUT_SECONDS = 900; //15mins timeout before declaring node stale

        static readonly byte[] NODE_ID_SALT = new byte[] { 0xF4, 0xC7, 0x56, 0x9A, 0xA3, 0xAD, 0xC9, 0xA7, 0x13, 0x0E, 0xCA, 0x56, 0x56, 0xA3, 0x52, 0x8F, 0xFE, 0x6E, 0x9C, 0x72 };

        readonly EndPoint _nodeEP;
        readonly BinaryNumber _nodeId;

        protected bool _isCurrentNode;
        DateTime _lastSeen;
        int _successfulRpcCount = 0;
        int _failRpcCount = 0;

        #endregion

        #region constructor

        public NodeContact(BinaryReader bR)
        {
            _nodeEP = EndPointExtension.Parse(bR);
            _nodeId = GetNodeId(_nodeEP);
        }

        public NodeContact(EndPoint nodeEP)
        {
            _nodeEP = nodeEP;
            _nodeId = GetNodeId(_nodeEP);
        }

        protected NodeContact(BinaryNumber nodeId, EndPoint nodeEP)
        {
            _nodeId = nodeId;
            _nodeEP = nodeEP;
        }

        #endregion

        #region static

        private static BinaryNumber GetNodeId(EndPoint nodeEP)
        {
            using (HMAC hmac = new HMACSHA256(NODE_ID_SALT))
            {
                using (MemoryStream mS = new MemoryStream(32))
                {
                    nodeEP.WriteTo(new BinaryWriter(mS));
                    mS.Position = 0;

                    return new BinaryNumber(hmac.ComputeHash(mS));
                }
            }
        }

        #endregion

        #region public

        public bool IsStale()
        {
            if (_isCurrentNode)
                return false;
            else
                return ((_failRpcCount > NODE_RPC_FAIL_LIMIT) || ((DateTime.UtcNow - _lastSeen).TotalSeconds > NODE_STALE_TIMEOUT_SECONDS));
        }

        public void UpdateLastSeenTime()
        {
            _lastSeen = DateTime.UtcNow;
            _successfulRpcCount++;
            _failRpcCount = 0;
        }

        public void IncrementRpcFailCount()
        {
            _failRpcCount++;
        }

        public void WriteTo(BinaryWriter bW)
        {
            _nodeEP.WriteTo(bW);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            NodeContact contact = obj as NodeContact;
            if (contact == null)
                return false;

            if (_nodeEP.Equals(contact._nodeEP))
                return true;

            return _nodeId.Equals(contact._nodeId);
        }

        public override int GetHashCode()
        {
            return _nodeId.GetHashCode();
        }

        public override string ToString()
        {
            return _nodeEP.ToString();
        }

        #endregion

        #region properties

        public EndPoint NodeEP
        { get { return _nodeEP; } }

        public BinaryNumber NodeId
        { get { return _nodeId; } }

        public bool IsCurrentNode
        { get { return _isCurrentNode; } }

        public DateTime LastSeen
        { get { return _lastSeen; } }

        public int SuccessfulRpcCount
        { get { return _successfulRpcCount; } }

        #endregion
    }
}
