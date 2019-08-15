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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TechnitiumLibrary.Net;

namespace MeshApp.UserControls
{
    public partial class ChatMessageFileItem : CustomListViewItem, IChatMessageItem
    {
        #region events

        public event EventHandler ForwardTo;

        #endregion  

        #region variables

        MeshNetwork.Peer _senderPeer;
        MessageItem _message;

        MeshNetwork.Peer.FileTransfer _fileTransfer;
        Timer _updateTimer;

        #endregion

        #region constructor

        public ChatMessageFileItem(MeshNetwork.Peer senderPeer, MessageItem message)
        {
            InitializeComponent();

            _senderPeer = senderPeer;
            _message = message;

            lblFileName.Text = _message.FileName;
            lblContentType.Text = WebUtilities.GetContentType(_message.FileName).ToString();
            lblFileSize.Text = WebUtilities.GetFormattedSize(_message.FileSize);

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

            if (_senderPeer.IsSelfPeer)
            {
                if (File.Exists(_message.FilePath))
                {
                    linkAction.Visible = true;
                    linkAction.Text = "Open";
                }
                else
                {
                    linkAction.Visible = false;
                }

                pbDownloadProgress.Visible = false;
            }
            else if (WasFileDownloaded())
            {
                linkAction.Visible = true;
                linkAction.Text = "Open";
                pbDownloadProgress.Visible = false;
            }
            else
            {
                linkAction.Visible = true;
                linkAction.Text = "Download";
                pbDownloadProgress.Visible = false;
            }

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
                    pnlBubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
                else
                {
                    lblDateTime.Left = pnlBubble.Width - lblDateTime.Width - 2;
                }
            }
        }

        #endregion

        #region form code

        public override bool AllowTriming()
        {
            if (_fileTransfer == null)
                return true;

            switch (_fileTransfer.Status)
            {
                case MeshNetwork.Peer.FileTransferStatus.Downloading:
                case MeshNetwork.Peer.FileTransferStatus.Starting:
                    return false;

                default:
                    return true;
            }
        }

        private bool WasFileDownloaded()
        {
            string filePath = Path.Combine(_senderPeer.Network.Node.DownloadFolder, _message.FileName);
            return (File.Exists(filePath) && ((new FileInfo(filePath)).Length >= _message.FileSize));
        }

        private void linkAction_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            switch (linkAction.Text)
            {
                case "Download":
                    downloadToolStripMenuItem_Click(null, null);
                    break;

                case "Pause":
                    pauseToolStripMenuItem_Click(null, null);
                    break;

                case "Open":
                    openFileToolStripMenuItem_Click(null, null);
                    break;
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

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_senderPeer.IsSelfPeer)
            {
                if (File.Exists(_message.FilePath))
                {
                    downloadToolStripMenuItem.Visible = false;
                    pauseToolStripMenuItem.Visible = false;
                    mnuForwardTo.Visible = true;
                    openFileToolStripMenuItem.Visible = true;
                    openContainingFolderToolStripMenuItem.Visible = true;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else if (WasFileDownloaded())
            {
                downloadToolStripMenuItem.Visible = false;
                pauseToolStripMenuItem.Visible = false;
                mnuForwardTo.Visible = true;
                openFileToolStripMenuItem.Visible = true;
                openContainingFolderToolStripMenuItem.Visible = true;
            }
            else if (_fileTransfer == null)
            {
                downloadToolStripMenuItem.Visible = true;
                pauseToolStripMenuItem.Visible = false;
                mnuForwardTo.Visible = false;
                openFileToolStripMenuItem.Visible = false;
                openContainingFolderToolStripMenuItem.Visible = false;
            }
            else
            {
                switch (_fileTransfer.Status)
                {
                    case MeshNetwork.Peer.FileTransferStatus.Starting:
                    case MeshNetwork.Peer.FileTransferStatus.Downloading:
                        downloadToolStripMenuItem.Visible = false;
                        pauseToolStripMenuItem.Visible = true;
                        mnuForwardTo.Visible = false;
                        openFileToolStripMenuItem.Visible = false;
                        openContainingFolderToolStripMenuItem.Visible = false;
                        break;

                    case MeshNetwork.Peer.FileTransferStatus.Complete:
                        downloadToolStripMenuItem.Visible = false;
                        pauseToolStripMenuItem.Visible = false;
                        mnuForwardTo.Visible = true;
                        openFileToolStripMenuItem.Visible = true;
                        openContainingFolderToolStripMenuItem.Visible = true;
                        break;

                    case MeshNetwork.Peer.FileTransferStatus.Canceled:
                    case MeshNetwork.Peer.FileTransferStatus.Failed:
                    case MeshNetwork.Peer.FileTransferStatus.Error:
                        downloadToolStripMenuItem.Visible = true;
                        pauseToolStripMenuItem.Visible = false;
                        mnuForwardTo.Visible = false;
                        openFileToolStripMenuItem.Visible = false;
                        openContainingFolderToolStripMenuItem.Visible = false;
                        break;

                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _fileTransfer = _senderPeer.ReceiveFileAttachment(_message.RemoteMessageNumber, _message.FileName);

            linkAction.Text = "Pause";
            pbDownloadProgress.Visible = true;

            if (_updateTimer != null)
                _updateTimer.Dispose();

            _updateTimer = new Timer();
            _updateTimer.Interval = 1000;
            _updateTimer.Tick += updateTimer_Tick;
            _updateTimer.Start();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            switch (_fileTransfer.Status)
            {
                case MeshNetwork.Peer.FileTransferStatus.Starting:
                case MeshNetwork.Peer.FileTransferStatus.Downloading:
                    lblFileSize.Text = WebUtilities.GetFormattedSize(_fileTransfer.BytesReceived) + " of " + WebUtilities.GetFormattedSize(_fileTransfer.FileSize) + " (" + WebUtilities.GetFormattedSpeed(_fileTransfer.DownloadSpeed, false) + ")";
                    pbDownloadProgress.Value = _fileTransfer.ProgressPercentage;
                    break;

                case MeshNetwork.Peer.FileTransferStatus.Complete:
                    pbDownloadProgress.Visible = false;
                    lblFileSize.Text = WebUtilities.GetFormattedSize(_fileTransfer.BytesReceived) + " (" + WebUtilities.GetFormattedSpeed(_fileTransfer.DownloadSpeed, false) + ")";
                    linkAction.Text = "Open";
                    _updateTimer.Dispose();
                    break;

                case MeshNetwork.Peer.FileTransferStatus.Canceled:
                    pbDownloadProgress.Visible = false;
                    linkAction.Text = "Download";
                    _updateTimer.Dispose();
                    break;

                default:
                    linkAction.Text = "Download";
                    lblFileSize.Text = _fileTransfer.Status.ToString();
                    _updateTimer.Dispose();
                    break;
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _fileTransfer.Cancel();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath;
            long fileSize;

            if (_senderPeer.IsSelfPeer)
            {
                filePath = _message.FilePath;

                if (File.Exists(filePath))
                {
                    fileSize = (new FileInfo(filePath)).Length;
                }
                else
                {
                    linkAction.Visible = false;
                    return;
                }
            }
            else if (WasFileDownloaded())
            {
                filePath = Path.Combine(_senderPeer.Network.Node.DownloadFolder, _message.FileName);
                fileSize = (new FileInfo(filePath)).Length;
            }
            else
            {
                linkAction.Text = "Download";
                return;
            }

            string fileName = Path.GetFileName(filePath);

            if (MessageBox.Show("Are you sure to open the file?\r\n\r\nFile: " + fileName + "\r\nType: " + WebUtilities.GetContentType(fileName) + "\r\nSize: " + WebUtilities.GetFormattedSize(fileSize) + "\r\n\r\nWARNING! Do NOT open files sent by untrusted people as the files may be infected with trojan/virus.", "Open File Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                try
                {
                    Process.Start(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error! " + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath;

            if (_senderPeer.IsSelfPeer)
            {
                filePath = _message.FilePath;

                if (!File.Exists(filePath))
                {
                    linkAction.Visible = false;
                    return;
                }
            }
            else if (WasFileDownloaded())
            {
                filePath = Path.Combine(_senderPeer.Network.Node.DownloadFolder, _message.FileName);
            }
            else
            {
                return;
            }

            try
            {
                Process.Start(Path.GetDirectoryName(filePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
