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
using System.Net;
using System.Windows.Forms;
using TechnitiumLibrary.Net.Proxy;

namespace MeshApp
{
    public partial class frmProxyConfig : Form
    {
        #region variables

        string _proxyAddress;
        ushort _proxyPort = 0;

        #endregion

        #region constructor

        public frmProxyConfig()
        {
            InitializeComponent();
        }

        #endregion

        #region form code

        public frmProxyConfig(NetProxyType proxyType, string proxyAddress, int proxyPort, NetworkCredential proxyCredentials)
        {
            InitializeComponent();

            if ((proxyPort == 9150) && (proxyAddress == "127.0.0.1"))
                cmbProxy.SelectedIndex = 3;
            else
                cmbProxy.SelectedIndex = (int)proxyType;

            txtProxyAddress.Text = proxyAddress;
            txtProxyPort.Text = proxyPort.ToString();

            if (proxyCredentials != null)
            {
                chkProxyAuth.Checked = true;
                txtProxyUser.Text = proxyCredentials.UserName;
                txtProxyPass.Text = proxyCredentials.Password;
            }
        }

        private void chkProxyAuth_CheckedChanged(object sender, EventArgs e)
        {
            txtProxyUser.Enabled = chkProxyAuth.Checked;
            txtProxyPass.Enabled = chkProxyAuth.Checked;
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            if ((cmbProxy.SelectedIndex != 0) && (string.IsNullOrWhiteSpace(txtProxyAddress.Text)))
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

            this.DialogResult = DialogResult.OK;
            this.Close();
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

        #endregion

        #region properties

        public NetProxyType ProxyType
        { get { return (NetProxyType)cmbProxy.SelectedIndex; } }

        public string ProxyAddress
        { get { return _proxyAddress; } }

        public int ProxyPort
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
