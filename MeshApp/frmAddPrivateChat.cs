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

using System;
using System.Windows.Forms;
using TechnitiumLibrary.IO;

namespace MeshApp
{
    public partial class frmAddPrivateChat : Form
    {
        #region constructor

        public frmAddPrivateChat()
        {
            InitializeComponent();
        }

        #endregion

        #region form code

        private void btnAdd_Click(object sender, EventArgs e)
        {
            txtPeerUserId.Text = txtPeerUserId.Text.Trim();

            if (string.IsNullOrEmpty(txtPeerUserId.Text) || (txtPeerUserId.Text.Length != 40))
            {
                MessageBox.Show("Please enter a valid User ID of your peer to chat with.", "Invalid User ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtPeerDisplayName.Text = txtPeerDisplayName.Text.Trim();

            if (string.IsNullOrEmpty(txtPeerDisplayName.Text))
            {
                MessageBox.Show("Please enter a name for the peer to display.", "Missing Peer Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtInvitationMessage.Text))
            {
                MessageBox.Show("Please enter an invitation message for the new private chat.", "Missing Invitation Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion

        #region properties

        public BinaryNumber PeerUserId
        { get { return BinaryNumber.Parse(txtPeerUserId.Text); } }

        public string PeerDisplayName
        { get { return txtPeerDisplayName.Text; } }

        public bool LocalNetworkOnly
        { get { return chkLocalNetworkOnly.Checked; } }

        public string InvitationMessage
        { get { return txtInvitationMessage.Text; } }

        #endregion
    }
}
