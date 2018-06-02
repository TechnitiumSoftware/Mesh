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
using TechnitiumLibrary.IO;

namespace MeshCore.Message
{
    public enum MessageRecipientStatus : byte
    {
        Undelivered = 0,
        Delivered = 1
    }

    public class MessageRecipient
    {
        #region variables

        readonly BinaryNumber _userId;

        MessageRecipientStatus _status = MessageRecipientStatus.Undelivered;
        DateTime _deliveredOn;

        #endregion

        #region constructor

        public MessageRecipient(BinaryNumber userId)
        {
            _userId = userId;
        }

        public MessageRecipient(BinaryReader bR)
        {
            switch (bR.ReadByte()) //version
            {
                case 1:
                    _userId = new BinaryNumber(bR.BaseStream);
                    _status = (MessageRecipientStatus)bR.ReadByte();
                    _deliveredOn = bR.ReadDate();
                    break;

                default:
                    throw new InvalidDataException("Cannot decode data format: version not supported.");
            }
        }

        #endregion

        #region public

        public void SetDeliveredStatus()
        {
            _status = MessageRecipientStatus.Delivered;
            _deliveredOn = DateTime.UtcNow;
        }

        public void WriteTo(BinaryWriter bW)
        {
            bW.Write((byte)1); //version
            _userId.WriteTo(bW.BaseStream);
            bW.Write((byte)_status);
            bW.Write(_deliveredOn);
        }

        #endregion

        #region properties

        public BinaryNumber UserId
        { get { return _userId; } }

        public MessageRecipientStatus Status
        { get { return _status; } }

        public DateTime DeliveredOn
        { get { return _deliveredOn; } }

        #endregion
    }
}
