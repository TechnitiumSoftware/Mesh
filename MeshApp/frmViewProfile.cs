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

using MeshCore;
using MeshCore.Network;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MeshApp
{
    public partial class frmViewProfile : Form
    {
        #region variables

        MeshNetwork.Peer _peer;

        Image _profileImage;
        bool _changesMade = false;

        #endregion

        #region constructor

        public frmViewProfile(MeshNetwork.Peer peer)
        {
            InitializeComponent();

            _peer = peer;

            ShowDetails(_peer.ProfileDisplayName, _peer.PeerUserId.ToString(), _peer.ProfileStatus, _peer.ProfileStatusMessage, _peer.ProfileDisplayImage);
        }

        #endregion

        #region private

        private void ShowDetails(string name, string userId, MeshProfileStatus status, string statusMessage, byte[] image)
        {
            if (!_peer.IsSelfPeer)
            {
                labIcon.Cursor = Cursors.Default;
                picIcon.Cursor = Cursors.Default;
            }

            //name
            {
                //name icon
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

                if ((_peer == null) || _peer.IsOnline)
                    labIcon.BackColor = Color.FromArgb(102, 153, 255);
                else
                    labIcon.BackColor = Color.Gray;

                labName.Text = name;
            }

            //status
            labStatus.Text = statusMessage;

            switch (status)
            {
                case MeshProfileStatus.Inactive:
                    labStatus.Text = "(Inactive) " + labStatus.Text;
                    break;

                case MeshProfileStatus.Busy:
                    labStatus.Text = "(Busy) " + labStatus.Text;
                    break;
            }

            //image icon
            if ((image != null) && (image.Length > 0))
            {
                using (MemoryStream mS = new MemoryStream(image))
                {
                    _profileImage = Image.FromStream(mS);
                    picIcon.Image = _profileImage;
                }

                picIcon.Visible = true;
                labIcon.Visible = false;
            }
        }

        #endregion

        #region form code

        private void mnuChangePhoto_Click(object sender, EventArgs e)
        {
            using (frmImageDialog frm = new frmImageDialog())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _profileImage = frm.SelectedImage;
                    picIcon.Image = _profileImage;

                    using (MemoryStream mS = new MemoryStream(4096))
                    {
                        frm.SelectedImage.Save(mS, System.Drawing.Imaging.ImageFormat.Jpeg);
                        _peer.Network.Node.UpdateProfileDisplayImage(mS.ToArray());
                    }

                    _changesMade = true;

                    picIcon.Visible = true;
                    labIcon.Visible = false;
                }
            }
        }

        private void mnuRemovePhoto_Click(object sender, EventArgs e)
        {
            _peer.Network.Node.UpdateProfileDisplayImage(null);

            _profileImage = null;
            picIcon.Image = Properties.Resources.change_photo;

            _changesMade = true;

            picIcon.Visible = false;
            labIcon.Visible = true;
        }

        private void labIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (_peer.IsSelfPeer)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        mnuChangePhoto_Click(null, null);
                        break;

                    case MouseButtons.Right:
                        mnuRemovePhoto.Enabled = (_profileImage != null);
                        Control control = sender as Control;
                        mnuProfileImage.Show(control, e.Location);
                        break;
                }
            }
        }

        private void labIcon_MouseEnter(object sender, EventArgs e)
        {
            if (_peer.IsSelfPeer)
            {
                if (_profileImage == null)
                {
                    picIcon.Visible = true;
                    labIcon.Visible = false;
                }
                else
                {
                    picIcon.Image = Properties.Resources.change_photo;
                }
            }
        }

        private void picIcon_MouseEnter(object sender, EventArgs e)
        {
            if (_peer.IsSelfPeer)
            {
                if (_profileImage != null)
                    picIcon.Image = Properties.Resources.change_photo;
            }
        }

        private void picIcon_MouseLeave(object sender, EventArgs e)
        {
            if (_peer.IsSelfPeer)
            {
                if (_profileImage == null)
                {
                    labIcon.Visible = true;
                    picIcon.Visible = false;
                }
                else
                {
                    picIcon.Image = _profileImage;
                }
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            using (frmViewUserDetails frm = new frmViewUserDetails(_peer))
            {
                frm.ShowDialog(this);
            }
        }

        private void labName_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                mnuCopyUtility.Tag = sender;
                mnuCopyUtility.Show(sender as Control, e.Location);
            }
        }

        private void mnuCopy_Click(object sender, EventArgs e)
        {
            Label label = mnuCopyUtility.Tag as Label;

            if (label != null)
            {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetText(label.Text);
                }
                catch
                { }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_changesMade)
                this.DialogResult = DialogResult.OK;

            this.Close();
        }

        #endregion
    }
}
