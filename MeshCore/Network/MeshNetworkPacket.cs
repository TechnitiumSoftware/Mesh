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
using System.Collections.Generic;
using System.IO;
using System.Text;
using TechnitiumLibrary.IO;

/*  Mesh Network Packet
* 
*  +----------------+---------------//----------------+
*  |  type (8 bits) |     optional packet data        |
*  +----------------+---------------//----------------+
*  
*/

namespace MeshCore.Network
{
    public enum MeshNetworkPacketType : byte
    {
        PingRequest = 1,
        PingResponse = 2,
        PeerExchange = 3,
        LocalNetworkOnly = 4,
        Profile = 5,
        ProfileDisplayImage = 6,
        GroupDisplayImage = 7,
        GroupLockNetwork = 8,
        MessageTypingNotification = 9,
        Message = 10,
        MessageDeliveryNotification = 11,
        FileRequest = 12
    }

    public class MeshNetworkPacket
    {
        #region variables

        readonly MeshNetworkPacketType _type;

        #endregion

        #region constructor

        public MeshNetworkPacket(MeshNetworkPacketType type)
        {
            _type = type;
        }

        #endregion

        #region static

        public static MeshNetworkPacket Parse(BinaryReader bR)
        {
            MeshNetworkPacketType type = (MeshNetworkPacketType)bR.ReadByte();

            switch (type)
            {
                case MeshNetworkPacketType.PingRequest:
                case MeshNetworkPacketType.PingResponse:
                case MeshNetworkPacketType.MessageTypingNotification:
                    return new MeshNetworkPacket(type);

                case MeshNetworkPacketType.PeerExchange:
                    return new MeshNetworkPacketPeerExchange(bR);

                case MeshNetworkPacketType.LocalNetworkOnly:
                    return new MeshNetworkPacketLocalNetworkOnly(bR);

                case MeshNetworkPacketType.Profile:
                    return new MeshNetworkPacketProfile(bR);

                case MeshNetworkPacketType.ProfileDisplayImage:
                    return new MeshNetworkPacketProfileDisplayImage(bR);

                case MeshNetworkPacketType.GroupDisplayImage:
                    return new MeshNetworkPacketGroupDisplayImage(bR);

                case MeshNetworkPacketType.GroupLockNetwork:
                    return new MeshNetworkPacketGroupLockNetwork(bR);

                case MeshNetworkPacketType.Message:
                    return new MeshNetworkPacketMessage(bR);

                case MeshNetworkPacketType.MessageDeliveryNotification:
                    return new MeshNetworkPacketMessageDeliveryNotification(bR);

                case MeshNetworkPacketType.FileRequest:
                    return new MeshNetworkPacketFileRequest(bR);

                default:
                    throw new InvalidOperationException("Invalid message type: " + type);
            }
        }

        #endregion

        #region public

        public virtual void WriteTo(BinaryWriter bW)
        {
            bW.Write((byte)_type);
        }

        #endregion

        #region properties

        public MeshNetworkPacketType Type
        { get { return _type; } }

        #endregion
    }

    public class MeshNetworkPacketPeerExchange : MeshNetworkPacket
    {
        #region variables

        readonly ICollection<MeshNetworkPeerInfo> _peers;

        #endregion

        #region constructor

        public MeshNetworkPacketPeerExchange(ICollection<MeshNetworkPeerInfo> peers)
            : base(MeshNetworkPacketType.PeerExchange)
        {
            _peers = peers;
        }

        public MeshNetworkPacketPeerExchange(BinaryReader bR)
            : base(MeshNetworkPacketType.PeerExchange)
        {
            int count = bR.ReadByte();
            _peers = new List<MeshNetworkPeerInfo>(count);

            for (int i = 0; i < count; i++)
                _peers.Add(new MeshNetworkPeerInfo(bR));
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(Convert.ToByte(_peers.Count));
            foreach (MeshNetworkPeerInfo peer in _peers)
                peer.WriteTo(bW);
        }

        #endregion

        #region properties

        public ICollection<MeshNetworkPeerInfo> Peers
        { get { return _peers; } }

        #endregion
    }

    public class MeshNetworkPacketLocalNetworkOnly : MeshNetworkPacket
    {
        #region variables

        readonly DateTime _localNetworkOnlyDateModified;
        readonly bool _localNetworkOnly;

        #endregion

        #region constructor

        public MeshNetworkPacketLocalNetworkOnly(DateTime localNetworkOnlyDateModified, bool localNetworkOnly)
            : base(MeshNetworkPacketType.LocalNetworkOnly)
        {
            _localNetworkOnlyDateModified = localNetworkOnlyDateModified;
            _localNetworkOnly = localNetworkOnly;
        }

        public MeshNetworkPacketLocalNetworkOnly(BinaryReader bR)
            : base(MeshNetworkPacketType.LocalNetworkOnly)
        {
            _localNetworkOnlyDateModified = bR.ReadDate();
            _localNetworkOnly = bR.ReadBoolean();
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_localNetworkOnlyDateModified);
            bW.Write(_localNetworkOnly);
        }

        #endregion

        #region properties

        public DateTime LocalNetworkOnlyDateModified
        { get { return _localNetworkOnlyDateModified; } }

        public bool LocalNetworkOnly
        { get { return _localNetworkOnly; } }

        #endregion
    }

    public class MeshNetworkPacketProfile : MeshNetworkPacket
    {
        #region variables

        readonly DateTime _profileDateModified;
        readonly string _profileDisplayName;
        readonly MeshProfileStatus _profileStatus;
        readonly string _profileStatusMessage;

        #endregion

        #region constructor

        public MeshNetworkPacketProfile(DateTime profileDateModified, string profileDisplayName, MeshProfileStatus profileStatus, string profileStatusMessage)
            : base(MeshNetworkPacketType.Profile)
        {
            _profileDateModified = profileDateModified;
            _profileDisplayName = profileDisplayName;
            _profileStatus = profileStatus;
            _profileStatusMessage = profileStatusMessage;
        }

        public MeshNetworkPacketProfile(BinaryReader bR)
            : base(MeshNetworkPacketType.Profile)
        {
            _profileDateModified = bR.ReadDate();
            _profileDisplayName = Encoding.UTF8.GetString(bR.ReadBytes(bR.ReadByte()));
            _profileStatus = (MeshProfileStatus)bR.ReadByte();
            _profileStatusMessage = Encoding.UTF8.GetString(bR.ReadBytes(bR.ReadByte()));
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_profileDateModified);

            {
                byte[] buffer = Encoding.UTF8.GetBytes(_profileDisplayName);

                bW.Write(Convert.ToByte(buffer.Length));
                bW.Write(buffer, 0, buffer.Length);
            }

            bW.Write((byte)_profileStatus);

            {
                byte[] buffer = Encoding.UTF8.GetBytes(_profileStatusMessage);

                bW.Write(Convert.ToByte(buffer.Length));
                bW.Write(buffer, 0, buffer.Length);
            }
        }

        #endregion

        #region properties

        public DateTime ProfileDateModified
        { get { return _profileDateModified; } }

        public string ProfileDisplayName
        { get { return _profileDisplayName; } }

        public MeshProfileStatus ProfileStatus
        { get { return _profileStatus; } }

        public string ProfileStatusMessage
        { get { return _profileStatusMessage; } }

        #endregion
    }

    public class MeshNetworkPacketProfileDisplayImage : MeshNetworkPacket
    {
        #region variables

        readonly DateTime _profileDisplayImageDateModified;
        readonly byte[] _profileDisplayImage;

        #endregion

        #region constructor

        public MeshNetworkPacketProfileDisplayImage(DateTime profileDisplayImageDateModified, byte[] profileDisplayImage = null)
            : base(MeshNetworkPacketType.ProfileDisplayImage)
        {
            _profileDisplayImageDateModified = profileDisplayImageDateModified;
            _profileDisplayImage = profileDisplayImage;
        }

        public MeshNetworkPacketProfileDisplayImage(BinaryReader bR)
            : base(MeshNetworkPacketType.ProfileDisplayImage)
        {
            _profileDisplayImageDateModified = bR.ReadDate();

            ushort len = bR.ReadUInt16();
            if (len > 0)
                _profileDisplayImage = bR.ReadBytes(len);
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_profileDisplayImageDateModified);

            if ((_profileDisplayImage == null) || (_profileDisplayImage.Length == 0))
            {
                bW.Write((ushort)0);
            }
            else
            {
                bW.Write(Convert.ToUInt16(_profileDisplayImage.Length));
                bW.Write(_profileDisplayImage, 0, _profileDisplayImage.Length);
            }
        }

        #endregion

        #region properties

        public DateTime ProfileDisplayImageDateModified
        { get { return _profileDisplayImageDateModified; } }

        public byte[] ProfileDisplayImage
        { get { return _profileDisplayImage; } }

        #endregion
    }

    public class MeshNetworkPacketGroupDisplayImage : MeshNetworkPacket
    {
        #region variables

        readonly DateTime _groupDisplayImageDateModified;
        readonly byte[] _groupDisplayImage;

        #endregion

        #region constructor

        public MeshNetworkPacketGroupDisplayImage(DateTime groupDisplayImageDateModified, byte[] groupDisplayImage = null)
            : base(MeshNetworkPacketType.GroupDisplayImage)
        {
            _groupDisplayImageDateModified = groupDisplayImageDateModified;
            _groupDisplayImage = groupDisplayImage;
        }

        public MeshNetworkPacketGroupDisplayImage(BinaryReader bR)
            : base(MeshNetworkPacketType.GroupDisplayImage)
        {
            _groupDisplayImageDateModified = bR.ReadDate();

            ushort len = bR.ReadUInt16();
            if (len > 0)
                _groupDisplayImage = bR.ReadBytes(len);
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_groupDisplayImageDateModified);

            if ((_groupDisplayImage == null) || (_groupDisplayImage.Length == 0))
            {
                bW.Write((ushort)0);
            }
            else
            {
                bW.Write(Convert.ToUInt16(_groupDisplayImage.Length));
                bW.Write(_groupDisplayImage, 0, _groupDisplayImage.Length);
            }
        }

        #endregion

        #region properties

        public DateTime GroupDisplayImageDateModified
        { get { return _groupDisplayImageDateModified; } }

        public byte[] GroupDisplayImage
        { get { return _groupDisplayImage; } }

        #endregion
    }

    public class MeshNetworkPacketGroupLockNetwork : MeshNetworkPacket
    {
        #region variables

        readonly DateTime _groupLockNetworkDateModified;
        readonly bool _groupLockNetwork;

        #endregion

        #region constructor

        public MeshNetworkPacketGroupLockNetwork(DateTime groupLockNetworkDateModified, bool groupLockNetwork)
            : base(MeshNetworkPacketType.GroupLockNetwork)
        {
            _groupLockNetworkDateModified = groupLockNetworkDateModified;
            _groupLockNetwork = groupLockNetwork;
        }

        public MeshNetworkPacketGroupLockNetwork(BinaryReader bR)
            : base(MeshNetworkPacketType.GroupLockNetwork)
        {
            _groupLockNetworkDateModified = bR.ReadDate();
            _groupLockNetwork = bR.ReadBoolean();
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_groupLockNetworkDateModified);
            bW.Write(_groupLockNetwork);
        }

        #endregion

        #region properties

        public DateTime GroupLockNetworkDateModified
        { get { return _groupLockNetworkDateModified; } }

        public bool GroupLockNetwork
        { get { return _groupLockNetwork; } }

        #endregion
    }

    public class MeshNetworkPacketMessage : MeshNetworkPacket
    {
        #region variables

        readonly int _messageNumber;
        readonly DateTime _messageDate;
        readonly byte[] _messageData;

        #endregion

        #region constructor

        public MeshNetworkPacketMessage(int messageNumber, DateTime messageDate, byte[] messageData)
            : base(MeshNetworkPacketType.Message)
        {
            _messageNumber = messageNumber;
            _messageDate = messageDate;
            _messageData = messageData;
        }

        public MeshNetworkPacketMessage(BinaryReader bR)
            : base(MeshNetworkPacketType.Message)
        {
            _messageNumber = bR.ReadInt32();
            _messageDate = bR.ReadDate();

            ushort len = bR.ReadUInt16();
            if (len > 0)
                _messageData = bR.ReadBytes(len);
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_messageNumber);
            bW.Write(_messageDate);

            bW.Write(Convert.ToUInt16(_messageData.Length));
            bW.Write(_messageData, 0, _messageData.Length);
        }

        #endregion

        #region properties

        public int MessageNumber
        { get { return _messageNumber; } }

        public DateTime MessageDate
        { get { return _messageDate; } }

        public byte[] MessageData
        { get { return _messageData; } }

        #endregion
    }

    public class MeshNetworkPacketMessageDeliveryNotification : MeshNetworkPacket
    {
        #region variables

        readonly int _messageNumber;

        #endregion

        #region constructor

        public MeshNetworkPacketMessageDeliveryNotification(int messageNumber)
            : base(MeshNetworkPacketType.MessageDeliveryNotification)
        {
            _messageNumber = messageNumber;
        }

        public MeshNetworkPacketMessageDeliveryNotification(BinaryReader bR)
            : base(MeshNetworkPacketType.MessageDeliveryNotification)
        {
            _messageNumber = bR.ReadInt32();
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_messageNumber);
        }

        #endregion

        #region properties

        public int MessageNumber
        { get { return _messageNumber; } }

        #endregion
    }

    public class MeshNetworkPacketFileRequest : MeshNetworkPacket
    {
        #region variables

        readonly int _messageNumber;
        readonly long _fileOffset;
        readonly ushort _dataPort;

        #endregion

        #region constructor

        public MeshNetworkPacketFileRequest(int messageNumber, long fileOffset, ushort dataPort)
            : base(MeshNetworkPacketType.FileRequest)
        {
            _messageNumber = messageNumber;
            _fileOffset = fileOffset;
            _dataPort = dataPort;
        }

        public MeshNetworkPacketFileRequest(BinaryReader bR)
            : base(MeshNetworkPacketType.FileRequest)
        {
            _messageNumber = bR.ReadInt32();
            _fileOffset = bR.ReadInt64();
            _dataPort = bR.ReadUInt16();
        }

        #endregion

        #region public

        public override void WriteTo(BinaryWriter bW)
        {
            base.WriteTo(bW);

            bW.Write(_messageNumber);
            bW.Write(_fileOffset);
            bW.Write(_dataPort);
        }

        #endregion

        #region properties

        public int MessageNumber
        { get { return _messageNumber; } }

        public long FileOffset
        { get { return _fileOffset; } }

        public ushort DataPort
        { get { return _dataPort; } }

        #endregion
    }
}
