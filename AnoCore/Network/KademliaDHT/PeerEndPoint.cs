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
using TechnitiumLibrary.Net;

namespace BitChatCore.Network.KademliaDHT
{
    class PeerEndPoint : IPEndPoint
    {
        #region variables

        const int PEER_EXPIRY_TIME_SECONDS = 900; //15 min expiry

        DateTime _dateAdded = DateTime.UtcNow;

        #endregion

        #region constructor

        public PeerEndPoint(IPAddress address, int port)
            : base(address, port)
        { }

        public PeerEndPoint(Stream s)
            : base(0, 0)
        {
            IPEndPoint ep = IPEndPointParser.Parse(s);

            this.Address = ep.Address;
            this.Port = ep.Port;
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
            IPEndPointParser.WriteTo(this, s);
        }

        #endregion

        #region properties

        public DateTime DateAdded
        { get { return _dateAdded; } }

        #endregion
    }
}
