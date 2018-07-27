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

using MeshCore.Network;
using System;
using System.IO;
using TechnitiumLibrary.IO;

namespace MeshCore.Message
{
    public enum MessageType : byte
    {
        None = 0,
        Info = 1,
        TextMessage = 2,
        InlineImage = 3,
        FileAttachment = 4
    }

    public enum MessageDeliveryStatus
    {
        None = 0,
        Undelivered = 1,
        PartiallyDelivered = 2,
        Delivered = 3
    }

    public class MessageItem
    {
        #region variables

        int _messageNumber;

        //local data
        int _remoteMessageNumber;
        DateTime _messageDate;
        BinaryNumber _senderUserId;
        MessageRecipient[] _recipients;

        //transmitted message data 
        MessageType _type;
        string _messageText;
        byte[] _imageThumbnail;
        string _fileName;
        long _fileSize;

        //other local data
        string _filePath;

        #endregion

        #region constructor

        internal MessageItem(string info)
        {
            _messageNumber = -1;

            _type = MessageType.Info;
            _remoteMessageNumber = -1;
            _messageDate = DateTime.UtcNow;
            _messageText = info;
        }

        public MessageItem(DateTime infoDate)
        {
            _messageNumber = -1;

            _type = MessageType.Info;
            _remoteMessageNumber = -1;
            _messageDate = infoDate;
            _messageText = "";
        }

        internal MessageItem(DateTime messageDate, BinaryNumber senderUserId, MessageRecipient[] recipients, MessageType type, string messageText, byte[] imageThumbnail, string fileName, long fileSize, string filePath)
        {
            _messageNumber = -1;

            _remoteMessageNumber = -1;
            _messageDate = messageDate;
            _senderUserId = senderUserId;
            _recipients = recipients;

            _type = type;
            _messageText = messageText;
            _imageThumbnail = imageThumbnail;
            _fileName = fileName;
            _fileSize = fileSize;

            _filePath = filePath;
        }

        internal MessageItem(BinaryNumber senderUserId, MeshNetworkPacketMessage message)
        {
            _messageNumber = -1;

            _remoteMessageNumber = message.MessageNumber;
            _messageDate = message.MessageDate;
            _senderUserId = senderUserId;

            using (BinaryReader bR = new BinaryReader(new MemoryStream(message.MessageData, false)))
            {
                ReadTransmittedMessageDataFrom(bR);
            }
        }

        internal MessageItem(MessageStore store, int messageNumber)
        {
            _messageNumber = messageNumber;

            using (MemoryStream mS = new MemoryStream())
            {
                store.ReadMessage(messageNumber, mS);
                mS.Position = 0; //reset

                ReadFrom(new BinaryReader(mS));
            }
        }

        #endregion

        #region private

        private void ReadFrom(BinaryReader bR)
        {
            switch (bR.ReadByte()) //version
            {
                case 1:
                    _remoteMessageNumber = bR.ReadInt32();
                    _messageDate = bR.ReadDate();
                    _senderUserId = new BinaryNumber(bR.BaseStream);

                    _recipients = new MessageRecipient[bR.ReadByte()];
                    for (int i = 0; i < _recipients.Length; i++)
                        _recipients[i] = new MessageRecipient(bR);

                    ReadTransmittedMessageDataFrom(bR);

                    _filePath = bR.ReadShortString();
                    break;

                default:
                    throw new InvalidDataException("Cannot decode message data format: version not supported.");
            }
        }

        private void ReadTransmittedMessageDataFrom(BinaryReader bR)
        {
            switch (bR.ReadByte()) //version
            {
                case 1:
                    _type = (MessageType)bR.ReadByte();
                    _messageText = bR.ReadString();
                    _imageThumbnail = bR.ReadBuffer();
                    _fileName = bR.ReadShortString();
                    _fileSize = bR.ReadInt64();
                    break;

                default:
                    throw new InvalidDataException("Cannot decode message data format: version not supported.");
            }
        }

        #endregion

        #region static

        public static MessageItem[] GetLatestMessageItems(MessageStore store, int index, int count)
        {
            int totalMessages = store.GetMessageCount();

            if (index > totalMessages)
                index = totalMessages;
            else if (index < 1)
                return new MessageItem[] { };

            int firstMessageNumber = index - count;

            if (firstMessageNumber < 0)
                firstMessageNumber = 0;

            int itemCount = index - firstMessageNumber;

            MessageItem[] items = new MessageItem[itemCount];

            for (int i = firstMessageNumber, x = 0; i < index; i++, x++)
                items[x] = new MessageItem(store, i);

            return items;
        }

        #endregion

        #region internal

        internal MeshNetworkPacketMessage GetMeshNetworkPacket()
        {
            using (MemoryStream mS = new MemoryStream())
            {
                WriteTransmittedMessageDataTo(new BinaryWriter(mS));

                return new MeshNetworkPacketMessage(_messageNumber, _messageDate, mS.ToArray());
            }
        }

        internal void WriteTo(MessageStore store)
        {
            using (MemoryStream mS = new MemoryStream(128))
            {
                WriteTo(new BinaryWriter(mS));
                mS.Position = 0;

                if (_messageNumber == -1)
                    _messageNumber = store.WriteMessage(mS);
                else
                    store.UpdateMessage(_messageNumber, mS);
            }
        }

        internal void WriteTo(BinaryWriter bW)
        {
            bW.Write((byte)1); //version

            bW.Write(_remoteMessageNumber);
            bW.Write(_messageDate);

            if (_senderUserId == null)
                bW.Write((byte)0);
            else
                _senderUserId.WriteTo(bW.BaseStream);

            if (_recipients == null)
            {
                bW.Write((byte)0);
            }
            else
            {
                bW.Write(Convert.ToByte(_recipients.Length));
                foreach (MessageRecipient recipient in _recipients)
                    recipient.WriteTo(bW);
            }

            WriteTransmittedMessageDataTo(bW);

            if (_filePath == null)
                bW.Write((byte)0);
            else
                bW.WriteShortString(_filePath);
        }

        internal void WriteTransmittedMessageDataTo(BinaryWriter bW)
        {
            bW.Write((byte)1); //version

            bW.Write((byte)_type);
            bW.Write(_messageText);

            if (_imageThumbnail == null)
                bW.Write((byte)0);
            else
                bW.WriteBuffer(_imageThumbnail);

            if (_fileName == null)
                bW.Write((byte)0);
            else
                bW.WriteShortString(_fileName);

            bW.Write(_fileSize);
        }

        #endregion

        #region public

        public MessageDeliveryStatus GetDeliveryStatus()
        {
            bool containsDelivered = false;
            bool containsUndelivered = false;

            if (_recipients != null)
            {
                foreach (MessageRecipient rcpt in _recipients)
                {
                    if (rcpt.Status == MessageRecipientStatus.Delivered)
                        containsDelivered = true;
                    else
                        containsUndelivered = true;
                }
            }

            if (containsDelivered && containsUndelivered)
                return MessageDeliveryStatus.PartiallyDelivered;
            else if (containsDelivered)
                return MessageDeliveryStatus.Delivered;
            else if (containsUndelivered)
                return MessageDeliveryStatus.Undelivered;
            else
                return MessageDeliveryStatus.None;
        }

        #endregion

        #region properties

        public int MessageNumber
        { get { return _messageNumber; } }

        public MessageType Type
        { get { return _type; } }

        public int RemoteMessageNumber
        { get { return _remoteMessageNumber; } }

        public DateTime MessageDate
        { get { return _messageDate; } }

        public BinaryNumber SenderUserId
        { get { return _senderUserId; } }

        public MessageRecipient[] Recipients
        { get { return _recipients; } }

        public string MessageText
        { get { return _messageText; } }

        public byte[] ImageThumbnail
        { get { return _imageThumbnail; } }

        public string FileName
        { get { return _fileName; } }

        public long FileSize
        { get { return _fileSize; } }

        public string FilePath
        { get { return _filePath; } }

        #endregion
    }
}
