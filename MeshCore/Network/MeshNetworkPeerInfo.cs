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
using System.IO;
using System.Net;
using System.Text;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

namespace MeshCore.Network
{
    public class MeshNetworkPeerInfo
    {
        #region variables

        readonly BinaryNumber _peerUserId;
        readonly string _peerName; //optional
        readonly EndPoint[] _peerEPs;

        #endregion

        #region constructor

        public MeshNetworkPeerInfo(BinaryNumber peerUserId, IPEndPoint peerEP)
        {
            _peerUserId = peerUserId;
            _peerEPs = new IPEndPoint[] { peerEP };
        }

        public MeshNetworkPeerInfo(BinaryNumber peerUserId, string peerName, EndPoint[] peerEPs)
        {
            _peerUserId = peerUserId;
            _peerName = peerName;
            _peerEPs = peerEPs;
        }

        public MeshNetworkPeerInfo(BinaryReader bR)
        {
            _peerUserId = new BinaryNumber(bR.BaseStream);

            _peerName = Encoding.UTF8.GetString(bR.ReadBytes(bR.ReadByte()));
            if (_peerName == "")
                _peerName = null;

            {
                _peerEPs = new EndPoint[bR.ReadByte()];

                for (int i = 0; i < _peerEPs.Length; i++)
                    _peerEPs[i] = EndPointExtension.Parse(bR);
            }
        }

        #endregion

        #region public

        public void WriteTo(BinaryWriter bW)
        {
            _peerUserId.WriteTo(bW.BaseStream);

            if (_peerName == null)
                bW.Write((byte)0);
            else
                bW.WriteShortString(_peerName);

            bW.Write(Convert.ToByte(_peerEPs.Length));

            foreach (EndPoint peerEP in _peerEPs)
                peerEP.WriteTo(bW);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            MeshNetworkPeerInfo objPeerInfo = obj as MeshNetworkPeerInfo;

            return _peerUserId.Equals(objPeerInfo._peerUserId);
        }

        public override int GetHashCode()
        {
            return _peerUserId.GetHashCode();
        }

        public override string ToString()
        {
            if (_peerName == null)
                return "[" + _peerUserId.ToString() + "]";

            return _peerName + " [" + _peerUserId.ToString() + "]";
        }

        #endregion

        #region properties

        public BinaryNumber PeerUserId
        { get { return _peerUserId; } }

        public string PeerDisplayName
        {
            get
            {
                if (_peerName == null)
                    return _peerUserId.ToString();

                return _peerName;
            }
        }

        public EndPoint[] PeerEPs
        { get { return _peerEPs; } }

        #endregion
    }
}
