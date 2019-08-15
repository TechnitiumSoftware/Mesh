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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public partial class ChatListItem : CustomListViewItem
    {
        #region variables

        MeshNetwork _network;
        MeshNetworkPanel _chatPanel;

        string _message;
        DateTime _messageDate;
        int _unreadMessageCount;

        #endregion

        #region constructor

        public ChatListItem(MeshNetwork network)
        {
            InitializeComponent();

            _network = network;

            RefreshTitle();
            RefreshIcon();
            OnSelected();

            labLastMessage.Text = "";
            SetLastMessageDate();
            ResetUnreadMessageCount();

            _network.MessageReceived += network_MessageReceived;
            _network.PeerTyping += network_PeerTyping;

            if (_network.Type == MeshNetworkType.Private)
            {
                _network.OtherPeer.StateChanged += otherPeer_StateChanged;
                _network.OtherPeer.ProfileChanged += otherPeer_ProfileChanged;
            }
            else
            {
                _network.GroupImageChanged += network_GroupImageChanged;
            }

            _chatPanel = new MeshNetworkPanel(_network, this);
            _chatPanel.Dock = DockStyle.Fill;
        }

        #endregion

        #region private

        private void network_MessageReceived(MeshNetwork.Peer peer, MessageItem message)
        {
            string msg = null;

            switch (message.Type)
            {
                case MessageType.TextMessage:
                    if (peer.IsSelfPeer)
                        msg = message.MessageText;
                    else
                        msg = peer.ProfileDisplayName + ": " + message.MessageText;

                    break;

                case MessageType.InlineImage:
                    if (peer.IsSelfPeer)
                        msg = "shared an image.";
                    else
                        msg = peer.ProfileDisplayName + " shared an image.";

                    break;

                case MessageType.FileAttachment:
                    if (peer.IsSelfPeer)
                        msg = "shared a file.";
                    else
                        msg = peer.ProfileDisplayName + " shared a file: " + message.FileName;

                    break;
            }

            if (msg != null)
            {
                if (msg.Length > 100)
                    msg = msg.Substring(0, 100) + "...";

                SetLastMessage(msg, message.MessageDate, true);
            }
        }

        private void network_PeerTyping(MeshNetwork.Peer peer)
        {
            //show typing notification
            if ((_network.Type == MeshNetworkType.Private) && !peer.IsSelfPeer)
                labLastMessage.Text = "typing...";
            else
                labLastMessage.Text = peer.ProfileDisplayName + " is typing...";

            labLastMessage.ForeColor = Color.FromArgb(255, 213, 89);

            timerTypingNotification.Stop();
            timerTypingNotification.Start();
        }

        private void network_GroupImageChanged(MeshNetwork.Peer peer)
        {
            RefreshIcon();
        }

        private void otherPeer_StateChanged(object sender, EventArgs e)
        {
            RefreshIcon();
        }

        private void otherPeer_ProfileChanged(object sender, EventArgs e)
        {
            RefreshTitle();
            RefreshIcon();
        }

        private void timerTypingNotification_Tick(object sender, EventArgs e)
        {
            //hide typing notification
            labLastMessage.Text = _message;
            labLastMessage.ForeColor = Color.White;
        }

        protected override void OnSelected()
        {
            this.SuspendLayout();

            if (_network.Status == MeshNetworkStatus.Offline)
            {
                if (Selected)
                {
                    this.BackColor = Color.FromArgb(61, 78, 93);
                    labIcon.BackColor = Color.Gray;

                    ResetUnreadMessageCount();
                }
                else
                {
                    this.BackColor = Color.FromArgb(51, 65, 78);
                    labIcon.BackColor = Color.Gray;
                }
            }
            else
            {
                if (Selected)
                {
                    this.BackColor = Color.FromArgb(61, 78, 93);
                    labIcon.BackColor = Color.FromArgb(255, 213, 89);

                    ResetUnreadMessageCount();
                }
                else
                {
                    this.BackColor = Color.FromArgb(51, 65, 78);
                    labIcon.BackColor = Color.White;
                }
            }

            this.ResumeLayout();
        }

        protected override void OnMouseOver(bool hovering)
        {
            if (!Selected)
            {
                if (hovering)
                    this.BackColor = Color.FromArgb(61, 78, 93);
                else
                    this.BackColor = Color.FromArgb(51, 65, 78);
            }
        }

        private void ResetUnreadMessageCount()
        {
            _unreadMessageCount = 0;
            labUnreadMessageCount.Visible = false;
            labLastMessage.Width += labUnreadMessageCount.Width;
        }

        private void SetLastMessageDate()
        {
            if (string.IsNullOrEmpty(labLastMessage.Text))
            {
                labLastMessageDate.Text = "";
            }
            else
            {
                TimeSpan span = DateTime.UtcNow.Date - _messageDate.Date;

                if (span.TotalDays >= 7)
                    labLastMessageDate.Text = _messageDate.ToLocalTime().ToShortDateString();
                else if (span.TotalDays >= 2)
                    labLastMessageDate.Text = _messageDate.ToLocalTime().DayOfWeek.ToString();
                else if (span.TotalDays >= 1)
                    labLastMessageDate.Text = "Yesterday";
                else
                    labLastMessageDate.Text = _messageDate.ToLocalTime().ToShortTimeString();
            }

            labTitle.Width = this.Width - labTitle.Left - labLastMessageDate.Width - 3;
            labLastMessageDate.Left = labTitle.Left + labTitle.Width;
        }

        private void RefreshTitle()
        {
            string title = _network.NetworkName;

            labTitle.Text = title;
            labIcon.Text = title.Substring(0, 1).ToUpper();

            int x = title.LastIndexOf(" ", StringComparison.CurrentCultureIgnoreCase);
            if (x > 0)
            {
                labIcon.Text += title.Substring(x + 1, 1).ToUpper();
            }
            else if (title.Length > 1)
            {
                labIcon.Text += title.Substring(1, 1).ToLower();
            }
        }

        private void RefreshIcon()
        {
            if (_network.Status == MeshNetworkStatus.Online)
            {
                byte[] image;

                if (_network.Type == MeshNetworkType.Group)
                    image = _network.GroupDisplayImage;
                else if (_network.OtherPeer.IsOnline)
                    image = _network.OtherPeer.ProfileDisplayImage;
                else
                    image = null;

                if ((image == null) || (image.Length == 0))
                {
                    picIcon.Image = null;

                    labIcon.Visible = true;
                    picIcon.Visible = false;
                }
                else
                {
                    using (MemoryStream mS = new MemoryStream(image))
                    {
                        picIcon.Image = new Bitmap(Image.FromStream(mS), picIcon.Size);
                    }

                    if (_network.Status == MeshNetworkStatus.Online)
                    {
                        picIcon.Visible = true;
                        labIcon.Visible = false;
                    }
                    else
                    {
                        picIcon.Visible = false;
                        labIcon.Visible = true;
                    }
                }

            }
            else
            {
                labIcon.Visible = true;
                picIcon.Visible = false;
            }
        }

        #endregion

        #region public

        public void SetLastMessage(string message, DateTime messageDate, bool unread)
        {
            _message = message;
            _messageDate = messageDate;

            timerTypingNotification.Stop();

            labLastMessage.Text = _message;
            labLastMessage.ForeColor = Color.White;

            SetLastMessageDate();

            if (!this.Selected && unread)
            {
                if (_unreadMessageCount < 999)
                    _unreadMessageCount++;

                if (!labUnreadMessageCount.Visible)
                {
                    labUnreadMessageCount.Visible = true;
                    labLastMessage.Width -= labUnreadMessageCount.Width;
                }

                labUnreadMessageCount.Text = _unreadMessageCount.ToString();
            }

            this.SortListView();
        }

        public void GoOffline()
        {
            _network.GoOffline();
            RefreshIcon();
            OnSelected();
            SortListView();
        }

        public void GoOnline()
        {
            _network.GoOnline();
            RefreshIcon();
            OnSelected();
            SortListView();
        }

        public override string ToString()
        {
            SetLastMessageDate();

            string dateString = ((int)(DateTime.UtcNow - _messageDate).TotalSeconds).ToString().PadLeft(12, '0');
            return dateString + labTitle.Text;
        }

        #endregion

        #region properties

        public MeshNetwork Network
        { get { return _network; } }

        public MeshNetworkPanel ChatPanel
        { get { return _chatPanel; } }

        #endregion
    }
}
