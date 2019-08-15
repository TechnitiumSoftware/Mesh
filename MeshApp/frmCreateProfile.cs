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
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using TechnitiumLibrary.Net.Proxy;
using TechnitiumLibrary.Security.Cryptography;

namespace MeshApp
{
    public partial class frmCreateProfile : Form
    {
        #region variables

        RSAParameters _privateKey;

        bool _enableProxy;
        NetProxyType _proxyType;
        string _proxyAddress;
        int _proxyPort;
        NetworkCredential _proxyCredentials;

        #endregion

        #region constructor

        public frmCreateProfile()
        {
            InitializeComponent();

            btnBack.DialogResult = DialogResult.None;
        }

        #endregion

        #region form code

        private void rbImportRSA_CheckedChanged(object sender, EventArgs e)
        {
            if (rbImportRSA.Checked)
            {
                using (frmImportPEM frm = new frmImportPEM())
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        _privateKey = frm.PrivateKey;
                    }
                    else
                    {
                        rbAutoGenRSA.Checked = true;
                        rbImportRSA.Checked = false;
                    }
                }
            }
        }

        private void chkEnableProxy_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableProxy.Checked)
            {
                using (frmProxyConfig frm = new frmProxyConfig(_proxyType, _proxyAddress, _proxyPort, _proxyCredentials))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        if (frm.ProxyType != NetProxyType.None)
                            _proxyType = frm.ProxyType;

                        _proxyAddress = frm.ProxyAddress;
                        _proxyPort = frm.ProxyPort;
                        _proxyCredentials = frm.ProxyCredentials;

                        chkEnableProxy.Checked = (frm.ProxyType != NetProxyType.None);
                    }
                    else
                    {
                        chkEnableProxy.Checked = false;
                    }
                }
            }

            _enableProxy = chkEnableProxy.Checked;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            #region validate form

            if (string.IsNullOrEmpty(txtProfileDisplayName.Text.Trim()))
            {
                MessageBox.Show("Please enter a valid name.", "Name Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtProfileDisplayName.Focus();
                return;
            }

            if (cmbType.SelectedIndex == -1)
            {
                MessageBox.Show("Please select type of profile.", "Profile Type Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                cmbType.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtProfilePassword.Text))
            {
                MessageBox.Show("Please enter a profile password.", "Profile Password Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtProfilePassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtConfirmPassword.Text))
            {
                MessageBox.Show("Please confirm profile password.", "Confirm Password Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtConfirmPassword.Focus();
                return;
            }

            if (txtConfirmPassword.Text != txtProfilePassword.Text)
            {
                txtProfilePassword.Text = "";
                txtConfirmPassword.Text = "";
                MessageBox.Show("Profile password doesn't match with confirm profile password. Please enter both passwords again.", "Password Mismatch!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtProfilePassword.Focus();
                return;
            }

            if (rbAutoGenRSA.Checked)
                _privateKey = (new RSACryptoServiceProvider(2048)).ExportParameters(true);

            #endregion

            this.DialogResult = DialogResult.OK;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to close this window?", "Close Window?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                this.DialogResult = DialogResult.Cancel;
        }

        private void frmCreateProfile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Are you sure to close this window?", "Close Window?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    this.DialogResult = DialogResult.Cancel;
                else
                    e.Cancel = true;
            }
        }

        #endregion

        #region properties

        public string ProfileDisplayName
        { get { return txtProfileDisplayName.Text.Trim(); } }

        public MeshNodeType NodeType
        { get { return (MeshNodeType)cmbType.SelectedIndex + 1; } }

        public byte[] PrivateKey
        { get { return DEREncoding.EncodeRSAPrivateKey(_privateKey); } }

        public string ProfilePassword
        { get { return txtProfilePassword.Text; } }

        #endregion
    }
}
