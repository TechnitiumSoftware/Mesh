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
using System.IO;
using System.Net;
using System.Windows.Forms;
using TechnitiumLibrary.Net.Proxy;

namespace MeshApp
{
    public partial class frmSettings : Form
    {
        #region variables

        ushort _port = 0;

        string _proxyAddress;
        ushort _proxyPort = 0;

        #endregion

        #region constructor

        public frmSettings(MeshNode node, string profileFolder, bool isPortableApp)
        {
            InitializeComponent();

            if (isPortableApp)
            {
                txtDownloadFolder.Text = Path.Combine(profileFolder, "Downloads");
                btnBrowseDLFolder.Enabled = false;
            }
            else
            {
                txtDownloadFolder.Text = node.DownloadFolder;
                btnBrowseDLFolder.Enabled = true;
            }

            txtPort.Text = node.LocalServicePort.ToString();
            chkAllowInvitations.Checked = node.AllowInboundInvitations;
            chkAllowOnlyLocalInvitations.Enabled = chkAllowInvitations.Checked;
            chkAllowOnlyLocalInvitations.Checked = node.AllowOnlyLocalInboundInvitations;
            chkUPnP.Checked = node.EnableUPnP;

            if (node.Type == MeshNodeType.Anonymous)
            {
                cmbProxy.SelectedIndex = 0;
                cmbProxy.Enabled = false;
                chkUPnP.Enabled = false;
            }
            else if (node.Proxy == null)
            {
                cmbProxy.SelectedIndex = 0;
            }
            else
            {
                cmbProxy.SelectedIndex = (int)node.Proxy.Type;
                txtProxyAddress.Text = node.Proxy.Address;
                txtProxyPort.Text = node.Proxy.Port.ToString();

                if (node.Proxy.Credential == null)
                {
                    chkProxyAuth.Checked = false;
                }
                else
                {
                    chkProxyAuth.Checked = true;
                    txtProxyUser.Text = node.Proxy.Credential.UserName;
                    txtProxyPass.Text = node.Proxy.Credential.Password;
                }
            }

            btnCheckProxy.Enabled = (cmbProxy.SelectedIndex != 0);
            txtProxyAddress.Enabled = btnCheckProxy.Enabled;
            txtProxyPort.Enabled = btnCheckProxy.Enabled;
            chkProxyAuth.Enabled = btnCheckProxy.Enabled;
            txtProxyUser.Enabled = chkProxyAuth.Enabled && chkProxyAuth.Checked;
            txtProxyPass.Enabled = chkProxyAuth.Enabled && chkProxyAuth.Checked;

            if ((cmbProxy.SelectedIndex == 2) && (txtProxyPort.Text == "9150") && ("txtProxyAddress.Text" == "127.0.0.1"))
                cmbProxy.SelectedIndex = 3;
        }

        #endregion

        #region form code

        private void btnBrowseDLFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fBD = new FolderBrowserDialog())
            {
                fBD.SelectedPath = txtDownloadFolder.Text;
                fBD.Description = "Select a default folder to save downloaded files:";

                if (fBD.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    txtDownloadFolder.Text = fBD.SelectedPath;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtProfilePassword.Text) && (txtProfilePassword.Text != txtConfirmPassword.Text))
            {
                MessageBox.Show("Passwords don't match. Please enter password again.", "Passwords Don't Match!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                txtProfilePassword.Text = "";
                txtConfirmPassword.Text = "";

                txtProfilePassword.Focus();
                return;
            }

            if (!Directory.Exists(txtDownloadFolder.Text))
            {
                MessageBox.Show("Download folder does not exists. Please select a valid folder.", "Download Folder Does Not Exists!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ushort.TryParse(txtPort.Text, out _port))
            {
                MessageBox.Show("The port number specified is invalid. The number must be in 0-65535 range.", "Invalid Port Specified!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.EnableProxy)
            {
                if (string.IsNullOrWhiteSpace(txtProxyAddress.Text))
                {
                    MessageBox.Show("The proxy address is missing. Please enter a valid proxy address.", "Proxy Address Missing!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _proxyAddress = txtProxyAddress.Text;

                if (!ushort.TryParse(txtProxyPort.Text, out _proxyPort))
                {
                    MessageBox.Show("The proxy port number specified is invalid. The number must be in 0-65535 range.", "Invalid Proxy Port Specified!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if ((chkProxyAuth.Checked) && (string.IsNullOrWhiteSpace(txtProxyUser.Text)))
                {
                    MessageBox.Show("The proxy username is missing. Please enter a username.", "Proxy Username Missing!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cmbProxy_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnCheckProxy.Enabled = (cmbProxy.SelectedIndex != 0);

            txtProxyAddress.Enabled = btnCheckProxy.Enabled;
            txtProxyPort.Enabled = btnCheckProxy.Enabled;
            chkProxyAuth.Enabled = btnCheckProxy.Enabled;
            txtProxyUser.Enabled = chkProxyAuth.Enabled && chkProxyAuth.Checked;
            txtProxyPass.Enabled = chkProxyAuth.Enabled && chkProxyAuth.Checked;
        }

        private void chkProxyAuth_CheckedChanged(object sender, EventArgs e)
        {
            txtProxyUser.Enabled = chkProxyAuth.Checked;
            txtProxyPass.Enabled = chkProxyAuth.Checked;
        }

        private void btnCheckProxy_Click(object sender, EventArgs e)
        {
            try
            {
                NetProxyType proxyType = this.ProxyType;
                NetProxy proxy;
                NetworkCredential credentials = null;

                if (chkProxyAuth.Checked)
                    credentials = new NetworkCredential(txtProxyUser.Text, txtProxyPass.Text);

                switch (proxyType)
                {
                    case NetProxyType.Http:
                        proxy = new NetProxy(new WebProxyEx(new Uri("http://" + txtProxyAddress.Text + ":" + int.Parse(txtProxyPort.Text)), false, new string[] { }, credentials));
                        break;

                    case NetProxyType.Socks5:
                        proxy = new NetProxy(new SocksClient(txtProxyAddress.Text, int.Parse(txtProxyPort.Text), credentials));
                        break;

                    default:
                        return;
                }

                proxy.CheckProxyAccess();

                MessageBox.Show("Mesh was able to connect to the proxy server successfully.", "Proxy Check Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Proxy Check Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkAllowInvitations_CheckedChanged(object sender, EventArgs e)
        {
            chkAllowOnlyLocalInvitations.Enabled = chkAllowInvitations.Checked;
        }

        #endregion

        #region properties

        public bool PasswordChangeRequest
        { get { return !string.IsNullOrEmpty(txtProfilePassword.Text); } }

        public string Password
        { get { return txtProfilePassword.Text; } }

        public string DownloadFolder
        { get { return txtDownloadFolder.Text; } }

        public ushort Port
        { get { return _port; } }

        public bool AllowInboundInvitations
        { get { return chkAllowInvitations.Checked; } }

        public bool AllowOnlyLocalInboundInvitations
        { get { return chkAllowOnlyLocalInvitations.Checked; } }

        public bool EnableUPnP
        { get { return chkUPnP.Checked; } }

        public bool EnableProxy
        { get { return cmbProxy.SelectedIndex != 0; } }

        public NetProxyType ProxyType
        {
            get
            {
                if (cmbProxy.SelectedIndex == 3)
                    return NetProxyType.Socks5;
                else
                    return (NetProxyType)cmbProxy.SelectedIndex;
            }
        }

        public string ProxyAddress
        { get { return _proxyAddress; } }

        public ushort ProxyPort
        { get { return _proxyPort; } }

        public NetworkCredential ProxyCredentials
        {
            get
            {
                if (chkProxyAuth.Checked)
                    return new NetworkCredential(txtProxyUser.Text, txtProxyPass.Text);
                else
                    return null;
            }
        }

        #endregion
    }
}
