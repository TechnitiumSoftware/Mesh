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
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public partial class ChatMessageTextItem : CustomListViewItem, IChatMessageItem
    {
        #region events

        public event EventHandler ForwardTo;

        #endregion  

        #region variables

        MeshNetwork.Peer _senderPeer;
        MessageItem _message;

        #endregion

        #region constructor

        public ChatMessageTextItem(MeshNetwork.Peer senderPeer, MessageItem message)
        {
            InitializeComponent();

            this.SuspendLayout();

            _senderPeer = senderPeer;
            _message = message;

            lblMessage.Text = _message.MessageText;

            TimeSpan span = DateTime.UtcNow.Date - _message.MessageDate.Date;

            if (span.TotalDays >= 7)
                lblDateTime.Text = _message.MessageDate.ToLocalTime().ToString();
            else if (span.TotalDays >= 2)
                lblDateTime.Text = _message.MessageDate.ToLocalTime().DayOfWeek.ToString() + " " + _message.MessageDate.ToLocalTime().ToShortTimeString();
            else if (span.TotalDays >= 1)
                lblDateTime.Text = "Yesterday " + _message.MessageDate.ToLocalTime().ToShortTimeString();
            else
                lblDateTime.Text = _message.MessageDate.ToLocalTime().ToShortTimeString();

            toolTip1.SetToolTip(lblDateTime, _message.MessageDate.ToLocalTime().ToString());

            if (_senderPeer == null)
            {
                lblUsername.Text = _message.SenderUserId.ToString();
            }
            else
            {
                lblUsername.Text = _senderPeer.ProfileDisplayName;

                if (_senderPeer.IsSelfPeer)
                {
                    lblUsername.ForeColor = Color.FromArgb(63, 186, 228);
                    pnlBubble.Left = this.Width - pnlBubble.Width - 20;
                    picPointLeft.Visible = false;
                    picPointRight.Visible = true;
                    mnuMessageInfo.Visible = true;

                    switch (_message.GetDeliveryStatus())
                    {
                        case MessageDeliveryStatus.Undelivered:
                            if (_senderPeer.Network.Type == MeshNetworkType.Private)
                            {
                                picDeliveryStatus.Image = Properties.Resources.waiting;
                            }
                            else
                            {
                                if ((_message.Recipients.Length > 0) && ((DateTime.UtcNow - _message.MessageDate).TotalSeconds < 10))
                                {
                                    picDeliveryStatus.Image = Properties.Resources.waiting;
                                    timer1.Start();
                                }
                                else
                                {
                                    picDeliveryStatus.Image = Properties.Resources.message_failed;
                                }
                            }

                            break;

                        case MessageDeliveryStatus.PartiallyDelivered:
                            picDeliveryStatus.Image = Properties.Resources.single_tick;
                            break;

                        case MessageDeliveryStatus.Delivered:
                            picDeliveryStatus.Image = Properties.Resources.double_ticks;
                            break;

                        default:
                            picDeliveryStatus.Image = null;
                            break;
                    }

                    picDeliveryStatus.Visible = (picDeliveryStatus.Image != null);
                }
            }

            this.ResumeLayout();

            ResizeByTextSize();
        }

        #endregion

        #region form code

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            ResizeByTextSize();
        }

        private void ResizeByTextSize()
        {
            if ((lblMessage != null) && lblMessage.Text != "")
            {
                int maxBubbleWidth = (int)(this.Width * 0.75);

                Size msgSize = TextRenderer.MeasureText(lblMessage.Text, lblMessage.Font, new Size(maxBubbleWidth - 5 - 5, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

                int bubbleWidth = msgSize.Width + 5 + 5;

                if (bubbleWidth < (lblUsername.Width + 5 + 5))
                    bubbleWidth = lblUsername.Width + 5 + 5;

                if (bubbleWidth < 160)
                    bubbleWidth = 160;
                else if (bubbleWidth > maxBubbleWidth)
                    bubbleWidth = maxBubbleWidth;

                this.SuspendLayout();

                if (pnlBubble.Width != bubbleWidth)
                {
                    pnlBubble.Width = bubbleWidth;
                    lblMessage.Width = pnlBubble.Width - 5 - 5;

                    if ((_senderPeer != null) && _senderPeer.IsSelfPeer)
                        pnlBubble.Left = this.Width - pnlBubble.Width - 20;

                    if (picDeliveryStatus.Visible)
                    {
                        lblDateTime.Left = pnlBubble.Width - lblDateTime.Width - 24;
                        picDeliveryStatus.Left = lblDateTime.Left + lblDateTime.Width + 2;
                    }
                    else
                    {
                        lblDateTime.Left = pnlBubble.Width - lblDateTime.Width - 2;
                    }

                    //recalculate size for height due to change in width
                    msgSize = TextRenderer.MeasureText(lblMessage.Text, lblMessage.Font, new Size(lblMessage.Size.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                }

                int height = 4 + 21 + msgSize.Height + 21 + 4;

                if (this.Height != height)
                    this.Height = height;

                this.ResumeLayout();
            }
        }

        private void lblUsername_Click(object sender, EventArgs e)
        {
            if (_senderPeer != null)
            {
                using (frmViewProfile frm = new frmViewProfile(_senderPeer))
                {
                    frm.ShowDialog(this);
                }
            }
        }

        private void lblUsername_MouseEnter(object sender, EventArgs e)
        {
            lblUsername.Font = new Font(lblUsername.Font, FontStyle.Underline | FontStyle.Bold);
        }

        private void lblUsername_MouseLeave(object sender, EventArgs e)
        {
            lblUsername.Font = new Font(lblUsername.Font, FontStyle.Regular | FontStyle.Bold);
        }

        private void mnuCopyMessage_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText("[" + _message.MessageDate.ToString("d MMM, yyyy HH:mm:ss") + "] " + lblUsername.Text + "> " + lblMessage.Text);
            }
            catch
            { }
        }

        private void mnuForwardTo_Click(object sender, EventArgs e)
        {
            ForwardTo.Invoke(_message, EventArgs.Empty);
        }

        private void mnuMessageInfo_Click(object sender, EventArgs e)
        {
            using (frmMessageInfo frm = new frmMessageInfo(_senderPeer, _message))
            {
                frm.ShowDialog(this);
            }
        }

        public void DeliveryNotification(MessageItem msg)
        {
            _message = msg;

            switch (_message.GetDeliveryStatus())
            {
                case MessageDeliveryStatus.PartiallyDelivered:
                    picDeliveryStatus.Image = Properties.Resources.single_tick;
                    break;

                case MessageDeliveryStatus.Delivered:
                    picDeliveryStatus.Image = Properties.Resources.double_ticks;
                    break;

                default:
                    picDeliveryStatus.Image = Properties.Resources.message_failed;
                    break;
            }

            if (_senderPeer.Network.Type == MeshNetworkType.Group)
                timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            picDeliveryStatus.Image = Properties.Resources.message_failed;
            timer1.Stop();
        }

        #endregion

        #region properties

        public MessageItem Message
        { get { return _message; } }

        #endregion
    }
}
