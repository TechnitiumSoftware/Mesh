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

using MeshCore.Message;
using MeshCore.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public partial class ChatMessageView : CustomPanel
    {
        #region events

        public event EventHandler SettingsModified;
        public event EventHandler ForwardTo;

        #endregion

        #region variables

        const int MESSAGE_COUNT_PER_SCROLL = 20;

        MeshNetwork _network;
        ChatListItem _chatItem;

        bool _skipSettingsModifiedEvent = false;
        DateTime lastTypingNotification;

        #endregion

        #region constructor

        public ChatMessageView(MeshNetwork network, ChatListItem chatItem)
        {
            InitializeComponent();

            _network = network;
            _chatItem = chatItem;

            this.Title = _network.NetworkName;

            _network.MessageReceived += network_MessageReceived;
            _network.MessageDeliveryNotification += network_MessageDeliveryNotification;
            _network.PeerTyping += network_PeerTyping;

            if (_network.Type == MeshNetworkType.Private)
                _network.OtherPeer.ProfileChanged += otherPeer_ProfileChanged;

            //load stored messages
            int totalMessageCount = _network.GetMessageCount();
            if (totalMessageCount > 0)
            {
                try
                {
                    customListView1.ReplaceItems(ConvertToListViewItems(_network.GetLatestMessages(totalMessageCount, MESSAGE_COUNT_PER_SCROLL), true));
                    customListView1.ScrollToBottom();
                }
                catch
                { }
            }
        }

        #endregion

        #region mesh network events

        private void network_MessageReceived(MeshNetwork.Peer peer, MessageItem message)
        {
            switch (message.Type)
            {
                case MessageType.Info:
                    AddMessage(new ChatMessageInfoItem(message), peer.IsSelfPeer);
                    break;

                case MessageType.TextMessage:
                    ChatMessageTextItem textItem = new ChatMessageTextItem(peer, message);
                    textItem.ForwardTo += MessageItem_ForwardTo;

                    AddMessage(textItem, peer.IsSelfPeer);
                    ShowPeerTypingNotification(peer.ProfileDisplayName, false);
                    break;

                case MessageType.InlineImage:
                case MessageType.FileAttachment:
                    ChatMessageFileItem fileItem = new ChatMessageFileItem(peer, message);
                    fileItem.ForwardTo += MessageItem_ForwardTo;

                    AddMessage(fileItem, peer.IsSelfPeer);
                    break;
            }
        }

        private void network_MessageDeliveryNotification(MeshNetwork.Peer sender, MessageItem message)
        {
            foreach (Control item in customListView1.Controls)
            {
                IChatMessageItem msgItem = item as IChatMessageItem;
                if ((msgItem != null) && (msgItem.Message.MessageNumber == message.MessageNumber))
                {
                    msgItem.DeliveryNotification(message);
                    break;
                }
            }
        }

        private void network_PeerTyping(MeshNetwork.Peer peer)
        {
            ShowPeerTypingNotification(peer.ProfileDisplayName, true);
        }

        private void otherPeer_ProfileChanged(object sender, EventArgs e)
        {
            this.Title = _network.NetworkName;
        }

        #endregion

        #region protected UI code

        protected override void OnResize(EventArgs e)
        {
            _skipSettingsModifiedEvent = true;

            base.OnResize(e);

            if (splitContainer1 != null)
                txtMessage.Size = new Size(splitContainer1.Panel2.Width - 1 - 2 - btnSend.Width - 1, splitContainer1.Panel2.Height - 2);

            _skipSettingsModifiedEvent = false;
        }

        #endregion

        #region public

        public void SetFocusMessageEditor()
        {
            txtMessage.Focus();
        }

        public void TrimMessageList()
        {
            if (customListView1.IsScrolledToBottom())
                customListView1.TrimListFromTop(MESSAGE_COUNT_PER_SCROLL);
        }

        public void ReadSettingsFrom(BinaryReader bR)
        {
            _skipSettingsModifiedEvent = true;
            splitContainer1.SplitterDistance = splitContainer1.Height - bR.ReadInt32();
            _skipSettingsModifiedEvent = false;
        }

        public void WriteSettingsTo(BinaryWriter bW)
        {
            bW.Write(splitContainer1.Height - splitContainer1.SplitterDistance);
        }

        #endregion

        #region UI code

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if ((SettingsModified != null) && !_skipSettingsModifiedEvent)
                SettingsModified(this, EventArgs.Empty);
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                btnSend_Click(null, null);

                e.Handled = true;
                lastTypingNotification = DateTime.UtcNow.AddSeconds(-10);
            }
            else
            {
                DateTime current = DateTime.UtcNow;

                if ((current - lastTypingNotification).TotalSeconds > 5)
                {
                    lastTypingNotification = current;
                    _network.SendTypingNotification();
                }
            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.V:
                        if (Clipboard.ContainsFileDropList())
                        {
                            List<string> fileNames = new List<string>();

                            foreach (string filePath in Clipboard.GetFileDropList())
                            {
                                if (File.Exists(filePath))
                                    fileNames.Add(filePath);
                            }

                            foreach (string fileName in fileNames)
                                _network.SendFileAttachment("", fileName);

                            e.Handled = true;
                            e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.Back:
                        string msgRight = txtMessage.Text.Substring(0, txtMessage.SelectionStart);
                        string msgLeft = txtMessage.Text.Substring(txtMessage.SelectionStart);

                        int i = msgRight.TrimEnd().LastIndexOfAny(new char[] { ' ', '\n' });

                        if (i > -1)
                        {
                            i++;
                            txtMessage.Text = msgRight.Substring(0, i) + msgLeft;
                            txtMessage.SelectionStart = i;
                        }
                        else
                        {
                            txtMessage.Text = msgLeft;
                            txtMessage.SelectionStart = 0;
                        }

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text != "")
            {
                _network.SendTextMessage(txtMessage.Text);

                txtMessage.Text = "";
                txtMessage.Focus();
            }
        }

        private void btnShareFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog oFD = new OpenFileDialog())
            {
                oFD.Title = "Select files to share ...";
                oFD.CheckFileExists = true;
                oFD.Multiselect = true;

                if (oFD.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (string fileName in oFD.FileNames)
                        _network.SendFileAttachment("", fileName);
                }
            }
        }

        private List<CustomListViewItem> ConvertToListViewItems(MessageItem[] items, bool updateLastMessageInChatItem)
        {
            MeshNetwork.Peer[] peerList = _network.GetPeers();

            List<CustomListViewItem> listItems = new List<CustomListViewItem>(items.Length);
            DateTime lastItemDate = new DateTime();
            string lastMessage = null;
            DateTime lastMessageDate = new DateTime();

            foreach (MessageItem item in items)
            {
                if (lastItemDate.Date < item.MessageDate.Date)
                {
                    lastItemDate = item.MessageDate;
                    listItems.Add(new ChatMessageInfoItem(new MessageItem(lastItemDate)));
                }

                switch (item.Type)
                {
                    case MessageType.Info:
                        listItems.Add(new ChatMessageInfoItem(item));
                        break;

                    case MessageType.TextMessage:
                        {
                            MeshNetwork.Peer sender = null;

                            foreach (MeshNetwork.Peer peer in peerList)
                            {
                                if (peer.PeerUserId.Equals(item.SenderUserId))
                                {
                                    sender = peer;
                                    break;
                                }
                            }

                            if (sender == null)
                                continue;

                            ChatMessageTextItem textItem = new ChatMessageTextItem(sender, item);
                            textItem.ForwardTo += MessageItem_ForwardTo;

                            listItems.Add(textItem);

                            if (sender == null)
                                lastMessage = item.SenderUserId + ": " + item.MessageText;
                            else if (sender.IsSelfPeer)
                                lastMessage = item.MessageText;
                            else
                                lastMessage = sender.ProfileDisplayName + ": " + item.MessageText;

                            lastMessageDate = item.MessageDate;
                        }
                        break;

                    case MessageType.InlineImage:
                    case MessageType.FileAttachment:
                        {
                            MeshNetwork.Peer sender = null;

                            foreach (MeshNetwork.Peer peer in peerList)
                            {
                                if (peer.PeerUserId.Equals(item.SenderUserId))
                                {
                                    sender = peer;
                                    break;
                                }
                            }

                            if (sender == null)
                                continue;

                            ChatMessageFileItem fileItem = new ChatMessageFileItem(sender, item);
                            fileItem.ForwardTo += MessageItem_ForwardTo;

                            listItems.Add(fileItem);

                            string messageText = item.MessageText;
                            if (string.IsNullOrEmpty(messageText))
                                messageText = "file attachment";

                            if (sender == null)
                                lastMessage = item.SenderUserId + ": " + messageText;
                            else if (sender.IsSelfPeer)
                                lastMessage = messageText;
                            else
                                lastMessage = sender.ProfileDisplayName + ": " + messageText;

                            lastMessageDate = item.MessageDate;
                        }
                        break;
                }
            }

            if (updateLastMessageInChatItem && (lastMessage != null))
                _chatItem.SetLastMessage(lastMessage, lastMessageDate, false);

            return listItems;
        }

        private void MessageItem_ForwardTo(object sender, EventArgs e)
        {
            ForwardTo(sender, e);
        }

        private void customListView1_ScrolledNearStart(object sender, EventArgs e)
        {
            foreach (CustomListViewItem item in customListView1.Controls)
            {
                IChatMessageItem messageItem = item as IChatMessageItem;

                if (messageItem.Message.MessageNumber == 0)
                {
                    return;
                }
                else if (messageItem.Message.MessageNumber > -1)
                {
                    customListView1.InsertItemsAtTop(ConvertToListViewItems(_network.GetLatestMessages(messageItem.Message.MessageNumber, MESSAGE_COUNT_PER_SCROLL), false));
                    return;
                }
            }
        }

        private void ShowPeerTypingNotification(string peerName, bool add)
        {
            lock (timerTypingNotification)
            {
                List<string> peerNames;

                if (labTypingNotification.Tag == null)
                {
                    peerNames = new List<string>(3);
                    labTypingNotification.Tag = peerNames;
                }
                else
                {
                    peerNames = labTypingNotification.Tag as List<string>;
                }

                {
                    if (add)
                    {
                        if (!peerNames.Contains(peerName))
                            peerNames.Add(peerName);
                    }
                    else
                    {
                        if (peerName == null)
                            peerNames.Clear();
                        else
                            peerNames.Remove(peerName);
                    }
                }

                switch (peerNames.Count)
                {
                    case 0:
                        labTypingNotification.Text = "";
                        break;

                    case 1:
                        labTypingNotification.Text = peerNames[0] + " is typing...";
                        break;

                    case 2:
                        labTypingNotification.Text = peerNames[0] + " and " + peerNames[1] + " are typing...";
                        break;

                    case 3:
                        labTypingNotification.Text = peerNames[0] + ", " + peerNames[1] + " and " + peerNames[2] + " are typing...";
                        break;

                    default:
                        labTypingNotification.Text = "many people are typing...";
                        break;
                }

                if (peerName != null)
                {
                    timerTypingNotification.Stop();
                    timerTypingNotification.Start();
                }
            }
        }

        private void timerTypingNotification_Tick(object sender, EventArgs e)
        {
            lock (timerTypingNotification)
            {
                timerTypingNotification.Stop();

                ShowPeerTypingNotification(null, false);
            }
        }

        private void AddMessage(CustomListViewItem item, bool selfSender)
        {
            CustomListViewItem lastItem = customListView1.GetLastItem();

            bool insertDateInfo = false;
            DateTime itemDate = (item as IChatMessageItem).Message.MessageDate;

            if (lastItem == null)
            {
                insertDateInfo = true;
            }
            else
            {
                if (itemDate.Date > (lastItem as IChatMessageItem).Message.MessageDate.Date)
                    insertDateInfo = true;
            }

            bool wasScrolledToBottom = customListView1.IsScrolledToBottom();

            if (insertDateInfo)
                customListView1.AddItem(new ChatMessageInfoItem(new MessageItem(DateTime.UtcNow)));

            customListView1.AddItem(item);

            if (_chatItem.Selected && (wasScrolledToBottom || selfSender))
                customListView1.ScrollToBottom();
        }

        #endregion
    }
}
