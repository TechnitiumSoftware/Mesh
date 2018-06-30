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
using System.Net.Sockets;
using TechnitiumLibrary.Net;

namespace MeshCore.Network.DHT
{
    public class PeerEndPoint
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

        public PeerEndPoint(BinaryReader bR)
        {
            _endPoint = EndPointExtension.Parse(bR);
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

        public void WriteTo(BinaryWriter bW)
        {
            _endPoint.WriteTo(bW);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            PeerEndPoint other = obj as PeerEndPoint;
            if (other == null)
                return false;

            return _endPoint.Equals(other._endPoint);
        }

        public override int GetHashCode()
        {
            return _endPoint.GetHashCode();
        }

        public override string ToString()
        {
            return _endPoint.ToString();
        }

        #endregion

        #region properties

        public EndPoint EndPoint
        { get { return _endPoint; } }

        public int EndPointPort
        {
            get
            {
                if (_endPoint.AddressFamily == AddressFamily.Unspecified)
                    return (_endPoint as DomainEndPoint).Port;

                return (_endPoint as IPEndPoint).Port;
            }
        }

        public DateTime DateAdded
        { get { return _dateAdded; } }

        #endregion
    }
}
