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
using System.Text;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

namespace MeshCore.Network.DHT
{
    public enum PeerEndPointType : byte
    {
        IPEndPoint = 1,
        TorAddress = 2
    }

    public class PeerEndPoint
    {
        #region variables

        const int PEER_EXPIRY_TIME_SECONDS = 900; //15 min expiry

        BinaryNumber _meshId;
        PeerEndPointType _type;
        IPEndPoint _ep;
        string _torAddress;

        DateTime _dateAdded = DateTime.UtcNow;

        #endregion

        #region constructor

        public PeerEndPoint(BinaryNumber meshId, IPEndPoint ep)
        {
            _meshId = meshId;
            _type = PeerEndPointType.IPEndPoint;
            _ep = ep;
        }

        public PeerEndPoint(BinaryNumber meshId, int servicePort)
        {
            _meshId = meshId;
            _type = PeerEndPointType.IPEndPoint;
            _ep = new IPEndPoint(IPAddress.Any, servicePort);
        }

        public PeerEndPoint(BinaryNumber meshId, string torAddress)
        {
            _meshId = meshId;
            _type = PeerEndPointType.TorAddress;
            _torAddress = torAddress;
        }

        public PeerEndPoint(Stream s)
        {
            _meshId = new BinaryNumber(s);

            int type = s.ReadByte();
            if (type < 0)
                throw new EndOfStreamException();

            _type = (PeerEndPointType)type;

            switch (_type)
            {
                case PeerEndPointType.IPEndPoint:
                    _ep = IPEndPointParser.Parse(s);
                    break;

                case PeerEndPointType.TorAddress:
                    {
                        int len = s.ReadByte();
                        if (len < 0)
                            throw new EndOfStreamException();

                        byte[] buffer = new byte[len];
                        OffsetStream.StreamRead(s, buffer, 0, buffer.Length);

                        _torAddress = Encoding.UTF8.GetString(buffer);
                    }
                    break;

                default:
                    throw new IOException("Invalid peer end point type.");
            }
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

        public void WriteTo(Stream s)
        {
            _meshId.WriteTo(s);

            s.WriteByte((byte)_type);

            switch (_type)
            {
                case PeerEndPointType.IPEndPoint:
                    IPEndPointParser.WriteTo(_ep, s);
                    break;

                case PeerEndPointType.TorAddress:
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(_torAddress);

                        s.WriteByte(Convert.ToByte(buffer.Length));
                        s.Write(buffer, 0, buffer.Length);
                    }
                    break;
            }
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

            if (_meshId != other._meshId)
                return false;

            if (_type != other._type)
                return false;

            switch (_type)
            {
                case PeerEndPointType.IPEndPoint:
                    if (!_ep.Equals(other._ep))
                        return false;

                    break;

                case PeerEndPointType.TorAddress:
                    if (_torAddress != other._torAddress)
                        return false;

                    break;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _meshId.GetHashCode();
        }

        #endregion

        #region properties

        public BinaryNumber MeshId
        { get { return _meshId; } }

        public PeerEndPointType Type
        { get { return _type; } }

        public IPEndPoint IPEndPoint
        { get { return _ep; } }

        public string TorAddress
        { get { return _torAddress; } }

        public DateTime DateAdded
        { get { return _dateAdded; } }

        #endregion
    }
}
