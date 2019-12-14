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
using MeshCore;
using MeshCore.Message;
using MeshCore.Network;
using MeshCore.Network.Connections;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TechnitiumLibrary.IO;

namespace MeshApp
{
    public partial class frmMain : Form, IDebug
    {
        #region variables

        readonly MeshNode _node;
        readonly string _profileFilePath;
        readonly bool _isPortableApp;
        readonly MeshUpdate _meshUpdate;
        readonly frmProfileManager _profileManager;

        readonly SoundPlayer _sndMessageNotification = new SoundPlayer(Properties.Resources.MessageNotification);

        MeshNetworkPanel _currentChatPanel;
        Timer _networkStatusCheckTimer;

        bool _upnpNoticeShown = false;

        #endregion

        #region constructor

        public frmMain(MeshNode node, string profileFilePath, bool isPortableApp, MeshUpdate meshUpdate, frmProfileManager profileManager)
        {
            InitializeComponent();

            _node = node;
            _profileFilePath = profileFilePath;
            _isPortableApp = isPortableApp;
            _meshUpdate = meshUpdate;
            _profileManager = profileManager;

            _node.InvitationReceived += MeshNode_InvitationReceived;

            if (_node.Type == MeshNodeType.Anonymous)
                this.Text += " [Anonymous]";

            _meshUpdate.UpdateAvailable += meshUpdate_UpdateAvailable;
            _meshUpdate.NoUpdateAvailable += meshUpdate_NoUpdateAvailable;
            _meshUpdate.UpdateCheckFailed += meshUpdate_UpdateCheckFailed;
        }

        #endregion

        #region form code

        private void frmMain_Load(object sender, EventArgs e)
        {
            //load chats and ui views
            lblProfileDisplayName.Text = _node.ProfileDisplayName;

            foreach (MeshNetwork network in _node.GetNetworks())
                AddChatView(network);

            lstChats.SelectItem(lstChats.GetFirstItem());
            ShowSelectedChatView();

            //load settings
            bool loadDefaultSettings = true;

            if ((_node.AppData != null) && (_node.AppData.Length > 0))
            {
                try
                {
                    LoadProfileSettings(_node.AppData);
                    loadDefaultSettings = false;
                }
                catch
                { }
            }

            if (loadDefaultSettings)
            {
                this.Width = 960;
                this.Height = 540;

                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

                this.Left = workingArea.Width - workingArea.Left - this.Width - 20;
                this.Top = 100;
            }

            _networkStatusCheckTimer = new Timer();
            _networkStatusCheckTimer.Interval = 10000;
            _networkStatusCheckTimer.Tick += networkStatusCheckTimer_Tick;
            _networkStatusCheckTimer.Start();
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && (e.KeyCode == Keys.D))
            {
                StartDebugging();
                e.Handled = true;
            }
            else if (e.Alt && (e.KeyCode == Keys.F))
            {
                btnPlusButton_Click(null, null);
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                SaveProfile();
            }
            catch
            { }

            _networkStatusCheckTimer.Dispose();
            _node.Dispose();

            _profileManager.UnloadProfileMainForm(this);

            StopDebugging();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (this.DialogResult != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            }
        }

        private void btnPlusButton_Click(object sender, EventArgs e)
        {
            mnuPlus.Show(btnPlusButton, new Point(0, btnPlusButton.Height));
        }

        private void lblProfileDisplayName_MouseEnter(object sender, EventArgs e)
        {
            panel1.BackColor = Color.FromArgb(61, 78, 93);
            lblProfileDisplayName.BackColor = panel1.BackColor;
        }

        private void lblProfileDisplayName_MouseLeave(object sender, EventArgs e)
        {
            panel1.BackColor = Color.FromArgb(51, 65, 78);
            lblProfileDisplayName.BackColor = panel1.BackColor;
        }

        private void lstChats_DoubleClick(object sender, EventArgs e)
        {
            ShowSelectedChatView();
        }

        private void lstChats_ItemClick(object sender, EventArgs e)
        {
            ShowSelectedChatView();
        }

        private void lstChats_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                mnuMuteNotifications.Enabled = false;
                mnuMuteNotifications.Checked = false;
                mnuLockGroup.Visible = false;
                mnuGoOffline.Enabled = false;
                mnuGoOffline.Checked = false;
                mnuDeleteChat.Enabled = false;
                mnuViewPeerProfile.Visible = false;
                mnuGroupPhoto.Visible = false;
                mnuProperties.Enabled = false;
                mnuChat.Show(lstChats, e.Location);
            }
        }

        private void lstChats_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    ShowSelectedChatView();
                    e.Handled = true;
                    break;

                case Keys.Apps:
                    mnuMuteNotifications.Enabled = false;
                    mnuMuteNotifications.Checked = false;
                    mnuLockGroup.Visible = false;
                    mnuGoOffline.Enabled = false;
                    mnuGoOffline.Checked = false;
                    mnuDeleteChat.Enabled = false;
                    mnuViewPeerProfile.Visible = false;
                    mnuGroupPhoto.Visible = false;
                    mnuProperties.Enabled = false;
                    mnuChat.Show(lstChats, lstChats.Location);
                    e.Handled = true;
                    break;
            }
        }

        private void lstChats_ItemMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                mnuMuteNotifications.Enabled = true;
                mnuMuteNotifications.Checked = itm.Network.Mute;
                mnuLockGroup.Visible = (itm.Network.Type == MeshNetworkType.Group);
                mnuLockGroup.Checked = itm.Network.GroupLockNetwork;
                mnuGoOffline.Enabled = true;
                mnuGoOffline.Checked = (itm.Network.Status == MeshNetworkStatus.Offline);
                mnuDeleteChat.Enabled = true;
                mnuViewPeerProfile.Visible = (itm.Network.Type == MeshNetworkType.Private);
                mnuViewPeerProfile.Enabled = (itm.Network.OtherPeer != null);
                mnuGroupPhoto.Visible = (itm.Network.Type == MeshNetworkType.Group);
                mnuProperties.Enabled = true;
                mnuChat.Show(sender as Control, e.Location);
            }
        }

        private void lstChats_ItemKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    ShowSelectedChatView();
                    e.Handled = true;
                    break;

                case Keys.Apps:
                    ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                    mnuMuteNotifications.Enabled = true;
                    mnuMuteNotifications.Checked = itm.Network.Mute;
                    mnuLockGroup.Visible = (itm.Network.Type == MeshNetworkType.Group);
                    mnuLockGroup.Checked = itm.Network.GroupLockNetwork;
                    mnuGoOffline.Enabled = true;
                    mnuGoOffline.Checked = (itm.Network.Status == MeshNetworkStatus.Offline);
                    mnuDeleteChat.Enabled = true;
                    mnuViewPeerProfile.Visible = (itm.Network.Type == MeshNetworkType.Private);
                    mnuViewPeerProfile.Enabled = (itm.Network.OtherPeer != null);
                    mnuGroupPhoto.Visible = (itm.Network.Type == MeshNetworkType.Group);
                    mnuProperties.Enabled = true;
                    mnuChat.Show(lstChats, lstChats.Location);
                    e.Handled = true;
                    break;
            }
        }

        private void mainContainer_Panel2_Resize(object sender, EventArgs e)
        {
            panelGetStarted.Location = new Point(mainContainer.Panel2.Width / 2 - panelGetStarted.Width / 2, mainContainer.Panel2.Height / 2 - panelGetStarted.Height / 2);
        }

        private void chatPanel_SettingsModified(object sender, EventArgs e)
        {
            MeshNetworkPanel senderPanel = sender as MeshNetworkPanel;

            using (MemoryStream mS = new MemoryStream())
            {
                senderPanel.WriteSettingsTo(mS);

                foreach (Control ctrl in mainContainer.Panel2.Controls)
                {
                    MeshNetworkPanel panel = ctrl as MeshNetworkPanel;

                    if ((panel != null) && !panel.Equals(sender))
                    {
                        mS.Position = 0;
                        panel.ReadSettingsFrom(mS);
                    }
                }
            }
        }

        private void chatPanel_ForwardTo(object sender, EventArgs e)
        {
            MessageItem originalMessage = sender as MessageItem;

            using (frmForwardToNetwork frm = new frmForwardToNetwork(_node))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        switch (originalMessage.Type)
                        {
                            case MessageType.TextMessage:
                                foreach (MeshNetwork network in frm.SelectedNetworks)
                                    network.SendTextMessage(originalMessage.MessageText);

                                break;

                            case MessageType.InlineImage:
                            case MessageType.FileAttachment:
                                string filePath = originalMessage.FilePath;

                                if (filePath == null)
                                    filePath = Path.Combine(_node.DownloadFolder, originalMessage.FileName);

                                if (!File.Exists(filePath))
                                {
                                    MessageBox.Show("File does not exists!", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    return;
                                }

                                foreach (MeshNetwork network in frm.SelectedNetworks)
                                    network.SendFileAttachment(originalMessage.MessageText, filePath);

                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        private void network_MessageReceived(MeshNetwork.Peer peer, MessageItem message)
        {
            if (!peer.Network.Mute && (!this.Visible || !ApplicationIsActivated()))
            {
                string msg;

                switch (message.Type)
                {
                    case MessageType.TextMessage:
                        msg = message.MessageText;
                        break;

                    case MessageType.InlineImage:
                        msg = "shared an image.";
                        break;

                    case MessageType.FileAttachment:
                        msg = "shared a file '" + message.FileName + "'";
                        break;

                    default:
                        return;
                }

                lock (_profileManager.SystemTrayIcon)
                {
                    if (peer.Network.Type == MeshNetworkType.Private)
                        _profileManager.SystemTrayIcon.ShowBalloonTip(30000, peer.Network.NetworkName + " - Mesh App", msg, ToolTipIcon.Info);
                    else
                        _profileManager.SystemTrayIcon.ShowBalloonTip(30000, peer.Network.NetworkName + " - Mesh App", peer.ProfileDisplayName + ": " + msg, ToolTipIcon.Info);

                    _profileManager.SystemTrayIcon.Tag = this;
                }

                _sndMessageNotification.Play();
            }
        }

        private void networkStatusCheckTimer_Tick(object sender, EventArgs e)
        {
            string title = "Mesh";

            if (_node.Type == MeshNodeType.Anonymous)
                title += " [Anonymous]";

            if (_debugFile != null)
                title += " [Debugging]";

            switch (_node.IPv4InternetStatus)
            {
                case InternetConnectivityStatus.NoInternetConnection:
                case InternetConnectivityStatus.NoProxyInternetConnection:
                    switch (_node.IPv6InternetStatus)
                    {
                        case InternetConnectivityStatus.NoInternetConnection:
                        case InternetConnectivityStatus.NoProxyInternetConnection:
                            this.Text = title + " [No Internet]";
                            return;

                        case InternetConnectivityStatus.FirewalledInternetConnection:
                            this.Text = title + " [Firewalled IPv6 Only Internet]";
                            return;

                        case InternetConnectivityStatus.HttpProxyInternetConnection:
                            this.Text = title + " [IPv6 HTTP Proxy Internet]";
                            return;

                        case InternetConnectivityStatus.Socks5ProxyInternetConnection:
                            this.Text = title + " [IPv6 SOCKS5 Proxy Internet]";
                            return;

                        case InternetConnectivityStatus.ProxyConnectionFailed:
                            this.Text = title + " [IPv6 Proxy Failed]";
                            return;

                        default:
                            this.Text = title + " [IPv6 Only Internet]";
                            return;
                    }

                case InternetConnectivityStatus.NatInternetConnectionViaUPnPRouter:
                    switch (_node.UPnPStatus)
                    {
                        case UPnPDeviceStatus.ExternalIpPrivate:
                            this.Text = title + " [UPnP: No Public IP Detected]";
                            return;

                        case UPnPDeviceStatus.PortForwardedNotAccessible:
                            this.Text = title + " [UPnP: Port Not Accessible From Internet]";
                            return;

                        case UPnPDeviceStatus.PortForwardingFailed:
                            this.Text = title + " [UPnP: Port Forwarding Failed]";
                            return;
                    }

                    break;

                case InternetConnectivityStatus.NatOrFirewalledInternetConnection:
                case InternetConnectivityStatus.FirewalledInternetConnection:
                    if (_node.IPv4ExternalEndPoint == null)
                    {
                        this.Text = title + " [NAT or Firewalled Internet]";

                        if ((_node.UPnPStatus == UPnPDeviceStatus.DeviceNotFound) && !_upnpNoticeShown)
                        {
                            _upnpNoticeShown = true;

                            MessageBox.Show("Mesh has detected that your Internet access is either firewalled or you have a NAT enabled router or access point.\r\n\r\nSince, Mesh works using peer-to-peer (p2p) technology. NAT or firewall prevents other Mesh peers from directly connect to you over the Internet. This may affect your ability to chat with your peers and hence this should be fixed.\r\n\r\nTo fix this, you should either enable UPnP feature or configure port forwarding manually in your router or access point. If you are not sure, you can refer to the user manual that came with your device or search online for the user manual.", "Enable UPnP/Port Forwarding On Your Router", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        return;
                    }

                    break;

                case InternetConnectivityStatus.HttpProxyInternetConnection:
                    this.Text = title + " [HTTP Proxy Internet]";
                    return;

                case InternetConnectivityStatus.Socks5ProxyInternetConnection:
                    this.Text = title + " [SOCKS5 Proxy Internet]";
                    return;

                case InternetConnectivityStatus.ProxyConnectionFailed:
                    this.Text = title + " [Proxy Failed]";
                    return;
            }

            this.Text = title;
        }

        #region menus

        private void mnuAddPrivateChat_Click(object sender, EventArgs e)
        {
            using (frmAddPrivateChat frm = new frmAddPrivateChat())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        MeshNetwork network = _node.CreatePrivateChat(frm.PeerUserId, frm.PeerDisplayName, frm.LocalNetworkOnly, frm.InvitationMessage);

                        lstChats.SelectItem(AddChatView(network));
                        ShowSelectedChatView();

                        SaveProfile();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        private void mnuAddGroupChat_Click(object sender, EventArgs e)
        {
            using (frmAddGroupChat frm = new frmAddGroupChat())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        MeshNetwork network = _node.CreateGroupChat(frm.NetworkName, frm.SharedSecret, frm.LocalNetworkOnly);

                        lstChats.SelectItem(AddChatView(network));
                        ShowSelectedChatView();

                        SaveProfile();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        private void mnuMyProfile_Click(object sender, EventArgs e)
        {
            using (frmEditProfile frm = new frmEditProfile(_node))
            {
                switch (frm.ShowDialog(this))
                {
                    case DialogResult.OK:
                        _node.UpdateProfile(frm.ProfileDisplayName, frm.ProfileStatus, frm.ProfileStatusMessage);
                        _node.UpdateProfileDisplayImage(frm.ProfileDisplayImage);
                        SaveProfile();

                        lblProfileDisplayName.Text = frm.ProfileDisplayName;
                        break;

                    case DialogResult.Yes:
                        SaveProfile();
                        break;
                }
            }
        }

        private void mnuProfileSettings_Click(object sender, EventArgs e)
        {
            using (frmSettings frm = new frmSettings(_node, Path.GetDirectoryName(_profileFilePath), _isPortableApp))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    if (frm.PasswordChangeRequest)
                        _node.ChangePassword(frm.Password);

                    _node.DownloadFolder = frm.DownloadFolder;
                    _node.LocalServicePort = frm.Port;
                    _node.AllowInboundInvitations = frm.AllowInboundInvitations;
                    _node.AllowOnlyLocalInboundInvitations = frm.AllowOnlyLocalInboundInvitations;
                    _node.EnableUPnP = frm.EnableUPnP;

                    if (_node.Type != MeshNodeType.Anonymous)
                    {
                        if (frm.EnableProxy)
                            _node.ConfigureProxy(frm.ProxyType, frm.ProxyAddress, frm.ProxyPort, frm.ProxyCredentials);
                        else
                            _node.DisableProxy();
                    }

                    SaveProfile();
                }
            }
        }

        private void mnuNetworkInfo_Click(object sender, EventArgs e)
        {
            using (frmNetworkInfo frm = new frmNetworkInfo(_node))
            {
                frm.ShowDialog(this);
            }
        }

        private void mnuCheckUpdate_Click(object sender, EventArgs e)
        {
            mnuCheckUpdate.Enabled = false;

            if (_node.Proxy != null)
                _meshUpdate.Proxy = _node.Proxy;

            _meshUpdate.CheckForUpdate();
        }

        private void mnuAboutMesh_Click(object sender, EventArgs e)
        {
            using (frmAbout frm = new frmAbout())
            {
                frm.ShowDialog(this);
            }
        }

        private void mnuProfileManager_Click(object sender, EventArgs e)
        {
            _profileManager.Show();
            _profileManager.Activate();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit Mesh?", "Exit Mesh?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }

        private void mnuCloseWindow_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void mnuLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
            }
        }

        private void mnuMuteNotifications_Click(object sender, EventArgs e)
        {
            if (lstChats.SelectedItem != null)
            {
                ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                mnuMuteNotifications.Checked = !mnuMuteNotifications.Checked;
                itm.Network.Mute = mnuMuteNotifications.Checked;
            }
        }

        private void mnuLockGroup_Click(object sender, EventArgs e)
        {
            if (lstChats.SelectedItem != null)
            {
                ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                mnuLockGroup.Checked = !mnuLockGroup.Checked;
                itm.Network.GroupLockNetwork = mnuLockGroup.Checked;
            }
        }

        private void mnuGoOffline_Click(object sender, EventArgs e)
        {
            if (lstChats.SelectedItem != null)
            {
                ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                mnuGoOffline.Checked = !mnuGoOffline.Checked;

                if (mnuGoOffline.Checked)
                    itm.GoOffline();
                else
                    itm.GoOnline();
            }
        }

        private void mnuDeleteChat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete chat?\r\n\r\nWarning! You will lose all stored messages in this chat. If you wish to join back the same chat again, you will need to remember the Group Name/UserId and password.", "Delete Chat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                if (lstChats.SelectedItem != null)
                {
                    ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                    lstChats.RemoveItem(itm);
                    mainContainer.Panel2.Controls.Remove(itm.ChatPanel);

                    itm.ChatPanel.SettingsModified -= chatPanel_SettingsModified;

                    itm.Network.DeleteNetwork();

                    SaveProfile();
                }
            }
        }

        private void mnuViewPeerProfile_Click(object sender, EventArgs e)
        {
            if (lstChats.SelectedItem != null)
            {
                ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                using (frmViewProfile frm = new frmViewProfile(itm.Network.OtherPeer))
                {
                    frm.ShowDialog(this);
                }
            }
        }

        private void mnuGroupPhoto_Click(object sender, EventArgs e)
        {
            if (lstChats.SelectedItem != null)
            {
                ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                using (frmViewGroup frm = new frmViewGroup(itm.Network))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        SaveProfile();
                }
            }
        }

        private void mnuProperties_Click(object sender, EventArgs e)
        {
            if (lstChats.SelectedItem != null)
            {
                ChatListItem itm = lstChats.SelectedItem as ChatListItem;

                using (frmChatProperties frm = new frmChatProperties(itm.Network))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        try
                        {
                            if (itm.Network.SharedSecret != frm.SharedSecret)
                                itm.Network.SharedSecret = frm.SharedSecret;

                            if (itm.Network.LocalNetworkOnly != frm.LocalNetworkOnly)
                                itm.Network.LocalNetworkOnly = frm.LocalNetworkOnly;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region mesh update

        private void meshUpdate_UpdateCheckFailed(MeshUpdate sender, Exception ex)
        {
            mnuCheckUpdate.Enabled = true;
        }

        private void meshUpdate_NoUpdateAvailable(object sender, EventArgs e)
        {
            mnuCheckUpdate.Enabled = true;
        }

        private void meshUpdate_UpdateAvailable(object sender, EventArgs e)
        {
            mnuCheckUpdate.Enabled = true;
        }

        #endregion

        #region private

        private void MeshNode_InvitationReceived(MeshNetwork network)
        {
            AddChatView(network);

            if (lstChats.Controls.Count == 1)
            {
                lstChats.SelectItem(lstChats.GetFirstItem());
                ShowSelectedChatView();
            }

            SaveProfile();
        }

        private ChatListItem AddChatView(MeshNetwork network)
        {
            ChatListItem itm = new ChatListItem(network);

            itm.ChatPanel.SettingsModified += chatPanel_SettingsModified;
            itm.ChatPanel.ForwardTo += chatPanel_ForwardTo;

            network.MessageReceived += network_MessageReceived;

            mainContainer.Panel2.Controls.Add(itm.ChatPanel);

            lstChats.AddItem(itm);

            return itm;
        }

        private void ShowSelectedChatView()
        {
            if (lstChats.SelectedItem != null)
            {
                if (_currentChatPanel != null)
                    _currentChatPanel.TrimMessageList();

                MeshNetworkPanel chatPanel = (lstChats.SelectedItem as ChatListItem).ChatPanel;
                chatPanel.BringToFront();
                chatPanel.SetFocusMessageEditor();

                _currentChatPanel = chatPanel;
            }
        }

        private void ReadUISettingsFrom(Stream s)
        {
            BinaryReader bR = new BinaryReader(s);

            byte version = bR.ReadByte();

            switch (version) //version
            {
                case 1:
                case 2:
                    //form location
                    this.Location = new Point(bR.ReadInt32(), bR.ReadInt32());

                    //form size
                    this.Size = new Size(bR.ReadInt32(), bR.ReadInt32());

                    //form maximized
                    if (Convert.ToBoolean(bR.ReadByte()))
                        this.WindowState = FormWindowState.Maximized;

                    //form main container splitter position
                    if (version > 1)
                    {
                        mainContainer.SplitterDistance = mainContainer.Width - bR.ReadInt32();
                    }

                    //first chat panel settings
                    if (Convert.ToBoolean(bR.ReadByte()))
                    {
                        foreach (Control ctrl in mainContainer.Panel2.Controls)
                        {
                            MeshNetworkPanel panel = ctrl as MeshNetworkPanel;

                            if (panel != null)
                            {
                                panel.ReadSettingsFrom(bR);
                                break;
                            }
                        }
                    }
                    break;

                default:
                    throw new Exception("Settings format version not supported.");
            }
        }

        private void WriteUISettingsTo(Stream s)
        {
            BinaryWriter bW = new BinaryWriter(s);

            bW.Write((byte)2); //version

            //form location
            bW.Write(this.Location.X);
            bW.Write(this.Location.Y);

            //form size
            bool maximized = this.WindowState == FormWindowState.Maximized;
            Size size;

            if (maximized)
                size = new Size(960, 540);
            else
                size = this.Size;

            bW.Write(size.Width);
            bW.Write(size.Height);

            //form maximized
            if (maximized)
                bW.Write((byte)1);
            else
                bW.Write((byte)0);

            //form main container splitter position
            bW.Write(mainContainer.Width - mainContainer.SplitterDistance);


            //write first chat panel settings
            bool panelFound = false;

            foreach (Control ctrl in mainContainer.Panel2.Controls)
            {
                MeshNetworkPanel panel = ctrl as MeshNetworkPanel;

                if (panel != null)
                {
                    bW.Write((byte)1);
                    panel.WriteSettingsTo(bW);

                    panelFound = true;
                    break;
                }
            }

            if (!panelFound)
                bW.Write((byte)0);
        }

        private void LoadProfileSettings(byte[] appData)
        {
            using (Package pkg = new Package(new MemoryStream(appData, false), PackageMode.Open))
            {
                foreach (PackageItem item in pkg.Items)
                {
                    switch (item.Name)
                    {
                        case "ui":
                            ReadUISettingsFrom(item.DataStream);
                            break;
                    }
                }
            }
        }

        private byte[] SaveProfileSettings()
        {
            using (MemoryStream mS = new MemoryStream())
            {
                using (Package pkg = new Package(mS, PackageMode.Create))
                {
                    {
                        MemoryStream ui = new MemoryStream();
                        WriteUISettingsTo(ui);
                        ui.Position = 0;

                        pkg.AddItem(new PackageItem("ui", ui));
                    }
                }

                return mS.ToArray();
            }
        }

        private void SaveProfile()
        {
            //write profile in tmp file
            using (FileStream fS = new FileStream(_profileFilePath + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                _node.AppData = SaveProfileSettings();
                _node.SaveTo(fS);
            }

            File.Delete(_profileFilePath + ".bak"); //remove old backup file
            File.Move(_profileFilePath, _profileFilePath + ".bak"); //make current profile file as backup file
            File.Move(_profileFilePath + ".tmp", _profileFilePath); //make tmp file as profile file
        }

        #endregion

        #region application active check

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        #endregion

        #region IDebug

        FileStream _debugFile;
        StreamWriter _debugWriter;

        public void Write(string message)
        {
            lock (_debugFile)
            {
                if (_debugWriter != null)
                    _debugWriter.WriteLine(message);
            }
        }

        private void StartDebugging()
        {
            if (_debugFile == null)
            {
                _debugFile = new FileStream(_profileFilePath + ".log", FileMode.Create, FileAccess.Write, FileShare.Read);
                _debugWriter = new StreamWriter(_debugFile);
                _debugWriter.AutoFlush = true;

                MeshCore.Debug.SetDebug(this);

                this.Text += " [Debugging]";
            }
        }

        private void StopDebugging()
        {
            if (_debugFile != null)
            {
                lock (_debugFile)
                {
                    _debugWriter.Flush();
                    _debugFile.Close();

                    _debugWriter = null;
                }
            }
        }

        #endregion

        #region properties

        public string ProfileFilePath
        { get { return _profileFilePath; } }

        #endregion
    }
}
