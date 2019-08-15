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

using MeshCore.Network;
using System;
using System.IO;
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public delegate void MessageNotification(MeshNetwork sender, MeshNetwork.Peer messageSender, string message);

    public partial class MeshNetworkPanel : UserControl
    {
        #region events

        public event EventHandler SettingsModified;
        public event EventHandler ForwardTo;

        #endregion

        #region variables

        MeshNetwork _network;
        ChatMessageView _view;

        bool _skipSettingsModifiedEvent = false;

        #endregion

        #region constructor

        public MeshNetworkPanel(MeshNetwork network, ChatListItem chatItem)
        {
            InitializeComponent();

            _network = network;

            _network.PeerAdded += network_PeerAdded;

            //create view
            _view = new ChatMessageView(_network, chatItem);
            _view.Dock = DockStyle.Fill;
            _view.AllowDrop = true;
            _view.SettingsModified += view_SettingsModified;
            _view.ForwardTo += view_ForwardTo;
            _view.DragEnter += lstFiles_DragEnter;
            _view.DragDrop += lstFiles_DragDrop;

            //load all peers
            foreach (MeshNetwork.Peer peer in _network.GetPeers())
            {
                lstUsers.AddItem(new UserListItem(peer));

                peer.StateChanged += peer_StateChanged;
            }

            //add view to panel
            meshPanelSplitContainer.Panel1.Controls.Add(_view);
        }

        #endregion

        #region mesh events

        private void network_PeerAdded(MeshNetwork.Peer peer)
        {
            lstUsers.AddItem(new UserListItem(peer));

            peer.StateChanged += peer_StateChanged;
        }

        private void peer_StateChanged(object sender, EventArgs e)
        {
            MeshNetwork.Peer peer = sender as MeshNetwork.Peer;

            if (_network.Type == MeshNetworkType.Private)
                _view.Title = _network.NetworkName;

            //MessageNotification(_network, null, message);
        }

        private void sharedFile_FileRemoved(object sender, EventArgs e)
        {
            //SharedFileItem item = sender as SharedFileItem;

            //lstFiles.RemoveItem(item);
        }

        #endregion

        #region private

        private void mnuViewUserProfile_Click(object sender, EventArgs e)
        {
            UserListItem item = lstUsers.SelectedItem as UserListItem;

            if (item != null)
            {
                using (frmViewProfile frm = new frmViewProfile(item.Peer))
                {
                    frm.ShowDialog(this);
                }
            }
        }

        private void lstUsers_ItemMouseUp(object sender, MouseEventArgs e)
        {
            UserListItem item = lstUsers.SelectedItem as UserListItem;

            if (item != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    mnuUserList.Show(sender as Control, e.Location);
                }
                else
                {
                    using (frmViewProfile frm = new frmViewProfile(item.Peer))
                    {
                        frm.ShowDialog(this);
                    }
                }
            }
        }

        private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if ((SettingsModified != null) && !_skipSettingsModifiedEvent)
                SettingsModified(this, EventArgs.Empty);
        }

        private void view_SettingsModified(object sender, EventArgs e)
        {
            if ((SettingsModified != null) && !_skipSettingsModifiedEvent)
                SettingsModified(this, EventArgs.Empty);
        }

        private void view_ForwardTo(object sender, EventArgs e)
        {
            ForwardTo(sender, e);
        }

        private void lstFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];

            foreach (string fileName in fileNames)
                _network.SendFileAttachment("", fileName);
        }

        private void lstFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        protected override void OnResize(EventArgs e)
        {
            _skipSettingsModifiedEvent = true;
            base.OnResize(e);
            _skipSettingsModifiedEvent = false;
        }

        #endregion

        #region public

        public void SetFocusMessageEditor()
        {
            _view.SetFocusMessageEditor();
        }

        public void TrimMessageList()
        {
            _view.TrimMessageList();
        }

        public void ReadSettingsFrom(Stream s)
        {
            _skipSettingsModifiedEvent = true;
            ReadSettingsFrom(new BinaryReader(s));
            _skipSettingsModifiedEvent = false;
        }

        public void ReadSettingsFrom(BinaryReader bR)
        {
            meshPanelSplitContainer.SplitterDistance = meshPanelSplitContainer.Width - bR.ReadInt32();

            _view.ReadSettingsFrom(bR);
        }

        public void WriteSettingsTo(Stream s)
        {
            WriteSettingsTo(new BinaryWriter(s));
        }

        public void WriteSettingsTo(BinaryWriter bW)
        {
            bW.Write(meshPanelSplitContainer.Width - meshPanelSplitContainer.SplitterDistance);

            _view.WriteSettingsTo(bW);
        }

        #endregion
    }
}
