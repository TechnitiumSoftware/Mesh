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

namespace MeshApp
{
    public partial class frmAddGroupChat : Form
    {
        #region constructor

        public frmAddGroupChat()
        {
            InitializeComponent();
        }

        #endregion

        #region form code

        private void btnAdd_Click(object sender, EventArgs e)
        {
            txtNetworkName.Text = txtNetworkName.Text.Trim();

            if (string.IsNullOrEmpty(txtNetworkName.Text))
            {
                MessageBox.Show("Please enter a valid group name.", "Invalid Group Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion

        #region properties

        public string NetworkName
        { get { return txtNetworkName.Text; } }

        public string SharedSecret
        { get { return txtPassword.Text; } }

        public bool LocalNetworkOnly
        { get { return chkLocalNetworkOnly.Checked; } }

        #endregion
    }
}
