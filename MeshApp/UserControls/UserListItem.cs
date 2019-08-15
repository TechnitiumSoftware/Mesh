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
using System.Drawing;
using System.IO;

namespace MeshApp.UserControls
{
    public partial class UserListItem : CustomListViewItem
    {
        #region variables

        MeshNetwork.Peer _peer;

        #endregion

        #region constructor

        public UserListItem(MeshNetwork.Peer peer)
        {
            InitializeComponent();

            _peer = peer;

            RefreshIcon();
            RefreshText();
            RefreshView();

            _peer.StateChanged += peer_StateChanged;
            _peer.ConnectivityStatusChanged += peer_ConnectivityStatusChanged;
            _peer.ProfileChanged += peer_ProfileChanged;
        }

        #endregion

        #region private

        private void RefreshIcon()
        {
            if (_peer.IsOnline)
            {
                byte[] image = _peer.ProfileDisplayImage;

                if ((image == null) || (image.Length == 0))
                {
                    labIcon.BackColor = Color.FromArgb(102, 153, 255);

                    labIcon.Visible = true;
                    picIcon.Visible = false;
                }
                else
                {
                    using (MemoryStream mS = new MemoryStream(image))
                    {
                        picIcon.Image = new Bitmap(Image.FromStream(mS), picIcon.Size);
                    }

                    labIcon.Visible = false;
                    picIcon.Visible = true;
                }
            }
            else
            {
                labIcon.BackColor = Color.Gray;

                labIcon.Visible = true;
                picIcon.Visible = false;
            }
        }

        private void RefreshText()
        {
            string name = _peer.ProfileDisplayName;

            if (name.Length > 0)
            {
                labIcon.Text = name.Substring(0, 1).ToUpper();

                int x = name.LastIndexOf(" ", StringComparison.CurrentCultureIgnoreCase);
                if (x > 0)
                {
                    labIcon.Text += name.Substring(x + 1, 1).ToUpper();
                }
                else if (name.Length > 1)
                {
                    labIcon.Text += name.Substring(1, 1).ToLower();
                }
            }
            else
            {
                labIcon.Text = "";
            }

            labName.Text = name;
            labStatusMessage.Text = _peer.ProfileStatusMessage;
        }

        private void RefreshView()
        {
            if (_peer.IsOnline)
            {
                switch (_peer.ConnectivityStatus)
                {
                    case MeshNetwork.PeerConnectivityStatus.FullMeshNetwork:
                        this.BackColor = Color.White;
                        break;

                    case MeshNetwork.PeerConnectivityStatus.PartialMeshNetwork:
                        this.BackColor = Color.Orange;
                        break;

                    default:
                        this.BackColor = Color.Gainsboro;
                        break;
                }
            }
            else
            {
                this.BackColor = Color.Gainsboro;
            }
        }

        private void peer_StateChanged(object sender, EventArgs e)
        {
            RefreshIcon();
            SortListView();
        }

        private void peer_ProfileChanged(object sender, EventArgs e)
        {
            RefreshText();
            RefreshIcon();
        }

        private void peer_ConnectivityStatusChanged(object sender, EventArgs e)
        {
            RefreshView();
        }

        #endregion

        #region protected

        protected override void OnMouseOver(bool hovering)
        {
            if (hovering)
                this.BackColor = Color.FromArgb(241, 245, 249);
            else
                RefreshView();
        }

        #endregion

        #region public

        public override string ToString()
        {
            string prepend;

            if (_peer.IsSelfPeer)
                prepend = "0";
            else if (_peer.IsOnline)
                prepend = "1";
            else
                prepend = "2";

            return prepend + _peer.ProfileDisplayName;
        }

        #endregion

        #region property

        public MeshNetwork.Peer Peer
        { get { return _peer; } }

        #endregion
    }
}
