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
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MeshApp
{
    public partial class frmEditProfile : Form
    {
        #region variables

        MeshNode _node;

        Image _profileImage;
        bool _changesMade = false;

        #endregion

        #region constructor

        public frmEditProfile(MeshNode node)
        {
            InitializeComponent();

            _node = node;

            ShowDetails(node.ProfileDisplayName, node.UserId.ToString(), node.ProfileStatus, node.ProfileStatusMessage, node.ProfileDisplayImage);
        }

        #endregion

        #region private

        private void ShowDetails(string name, string userId, MeshProfileStatus status, string statusMessage, byte[] image)
        {
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

                labIcon.BackColor = Color.FromArgb(102, 153, 255);
                txtDisplayName.Text = name;
            }

            //userid
            txtUserId.Text = userId;

            //status
            txtStatusMessage.Text = statusMessage;

            if (status == MeshProfileStatus.None)
                status = MeshProfileStatus.Active;

            cmbStatus.SelectedIndex = ((byte)status - 1);

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

                    picIcon.Visible = true;
                    labIcon.Visible = false;
                }
            }
        }

        private void mnuRemovePhoto_Click(object sender, EventArgs e)
        {
            _profileImage = null;
            picIcon.Image = Properties.Resources.change_photo;

            picIcon.Visible = false;
            labIcon.Visible = true;
        }

        private void labIcon_MouseUp(object sender, MouseEventArgs e)
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

        private void labIcon_MouseEnter(object sender, EventArgs e)
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

        private void picIcon_MouseEnter(object sender, EventArgs e)
        {
            if (_profileImage != null)
                picIcon.Image = Properties.Resources.change_photo;
        }

        private void picIcon_MouseLeave(object sender, EventArgs e)
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

        private void btnCopyUserId_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(txtUserId.Text);
            }
            catch
            { }
        }

        private void btnRandomUserId_Click(object sender, EventArgs e)
        {
            _node.GenerateNewUserId();
            txtUserId.Text = _node.UserId.ToString();

            _changesMade = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_changesMade)
                this.DialogResult = DialogResult.Yes;

            this.Close();
        }

        #endregion

        #region properties

        public string ProfileDisplayName
        { get { return txtDisplayName.Text; } }

        public MeshProfileStatus ProfileStatus
        { get { return (MeshProfileStatus)Enum.Parse(typeof(MeshProfileStatus), cmbStatus.Text); } }

        public string ProfileStatusMessage
        { get { return txtStatusMessage.Text; } }

        public byte[] ProfileDisplayImage
        {
            get
            {
                if (_profileImage == null)
                    return new byte[] { };

                using (MemoryStream mS = new MemoryStream(4096))
                {
                    (new Bitmap(_profileImage)).Save(mS, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return mS.ToArray();
                }
            }
        }

        #endregion
    }
}
