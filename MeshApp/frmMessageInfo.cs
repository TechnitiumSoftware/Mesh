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

using MeshApp.UserControls;
using MeshCore.Message;
using MeshCore.Network;
using System.Windows.Forms;

namespace MeshApp
{
    public partial class frmMessageInfo : Form
    {
        public frmMessageInfo(MeshNetwork.Peer selfPeer, MessageItem message)
        {
            this.SuspendLayout();

            InitializeComponent();

            CustomListViewItem displayItem;

            switch (message.Type)
            {
                case MessageType.TextMessage:
                    ChatMessageTextItem textItem = new ChatMessageTextItem(selfPeer, message);
                    textItem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    textItem.Width = chatMessagePanel.Width - 30;
                    textItem.ContextMenuStrip = null;

                    displayItem = textItem;
                    break;

                case MessageType.FileAttachment:
                    ChatMessageFileItem fileItem = new ChatMessageFileItem(selfPeer, message);
                    fileItem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    fileItem.Width = chatMessagePanel.Width - 30;
                    fileItem.ContextMenuStrip = null;

                    displayItem = fileItem;
                    break;

                default:
                    return;
            }

            if (displayItem.Height > 150)
            {
                chatMessagePanel.Height = 140;
            }
            else
            {
                chatMessagePanel.Height = displayItem.Height + 10;

                int heightDiff = 150 - chatMessagePanel.Height;

                this.Height -= heightDiff;
                listView1.Top -= heightDiff;
                btnClose.Top -= heightDiff;
            }

            chatMessagePanel.Controls.Add(displayItem);

            MeshNetwork.Peer[] peers = selfPeer.Network.GetPeers();

            foreach (MessageRecipient rcpt in message.Recipients)
            {
                string rcptName = null;

                foreach (MeshNetwork.Peer peer in peers)
                {
                    if (peer.PeerUserId.Equals(rcpt.UserId))
                    {
                        rcptName = peer.ProfileDisplayName;
                        break;
                    }
                }

                if (rcptName == null)
                    rcptName = rcpt.UserId.ToString();

                ListViewItem item = listView1.Items.Add(rcptName);

                switch (rcpt.Status)
                {
                    case MessageRecipientStatus.Delivered:
                        item.SubItems.Add("Delivered on " + rcpt.DeliveredOn.ToLocalTime().ToString());
                        break;

                    case MessageRecipientStatus.Undelivered:
                        item.SubItems.Add("Undelivered");
                        break;
                }
            }

            this.ResumeLayout();
        }
    }
}
