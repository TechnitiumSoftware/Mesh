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
using System.Net;
using System.Windows.Forms;

namespace MeshApp
{
    public partial class frmChatProperties : Form
    {
        #region variables

        MeshNetwork _network;

        Timer _timer;

        #endregion

        #region constructor

        public frmChatProperties(MeshNetwork network)
        {
            InitializeComponent();

            _network = network;

            this.Text = _network.NetworkName + " - Properties";
            chkLocalNetworkOnly.Checked = _network.LocalNetworkOnly;
            txtNetworkId.Text = _network.NetworkId.ToString();
            txtPassword.Text = _network.SharedSecret;

            if (_network.Type == MeshNetworkType.Private)
            {
                labNetworkName.Text = "Peer's User Id";
                txtNetworkName.Text = _network.OtherPeer.PeerUserId.ToString();
            }
            else
            {
                labNetworkName.Text = "Group Name";
                txtNetworkName.Text = _network.NetworkName;
            }

            {
                ListViewItem dhtItem = lstPeerInfo.Items.Add("IPv4 DHT");

                dhtItem.SubItems.Add("");
                dhtItem.SubItems.Add("");
            }

            {
                ListViewItem dhtItem = lstPeerInfo.Items.Add("IPv6 DHT");

                dhtItem.SubItems.Add("");
                dhtItem.SubItems.Add("");
            }

            {
                ListViewItem dhtItem = lstPeerInfo.Items.Add("LAN DHT");

                dhtItem.SubItems.Add("");
                dhtItem.SubItems.Add("");
            }

            {
                ListViewItem dhtItem = lstPeerInfo.Items.Add("Tor DHT");

                dhtItem.SubItems.Add("");
                dhtItem.SubItems.Add("");
            }

            {
                ListViewItem dhtItem = lstPeerInfo.Items.Add("IPv4 Tcp Relay");

                dhtItem.SubItems.Add("");
                dhtItem.SubItems.Add("");
            }

            {
                ListViewItem dhtItem = lstPeerInfo.Items.Add("IPv6 Tcp Relay");

                dhtItem.SubItems.Add("");
                dhtItem.SubItems.Add("");
            }

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += timer_Tick;
            _timer.Start();
        }

        #endregion

        #region form code

        private void timer_Tick(object sender, EventArgs e)
        {
            lock (lstPeerInfo.Items)
            {
                foreach (ListViewItem item in lstPeerInfo.Items)
                {
                    int peerCount;
                    bool enabled = false;

                    switch (item.Text)
                    {
                        case "IPv4 DHT":
                            peerCount = _network.DhtGetTotalIPv4Peers();
                            enabled = _network.IsIPv4DhtRunning;
                            break;

                        case "IPv6 DHT":
                            peerCount = _network.DhtGetTotalIPv6Peers();
                            enabled = _network.IsIPv6DhtRunning;
                            break;

                        case "LAN DHT":
                            peerCount = _network.DhtGetTotalLanPeers();
                            enabled = _network.IsLanDhtRunning;
                            break;

                        case "Tor DHT":
                            peerCount = _network.DhtGetTotalTorPeers();
                            enabled = _network.IsTorDhtRunning;
                            break;

                        case "IPv4 Tcp Relay":
                            peerCount = _network.TcpRelayGetTotalIPv4Peers();
                            enabled = _network.IsTcpRelayClientRunning;
                            break;

                        case "IPv6 Tcp Relay":
                            peerCount = _network.TcpRelayGetTotalIPv6Peers();
                            enabled = _network.IsTcpRelayClientRunning;
                            break;

                        default:
                            continue;
                    }

                    if (enabled)
                    {
                        if (item.Text.Contains("Tcp Relay"))
                        {
                            item.SubItems[1].Text = "working";
                            item.SubItems[2].Text = peerCount.ToString();
                        }
                        else
                        {
                            string strUpdateIn;
                            TimeSpan updateIn = _network.DhtNextUpdateIn();

                            if (updateIn.TotalSeconds > 2)
                            {
                                strUpdateIn = "";
                                if (updateIn.Hours > 0)
                                    strUpdateIn = updateIn.Hours + "h ";

                                if (updateIn.Minutes > 0)
                                    strUpdateIn += updateIn.Minutes + "m ";

                                strUpdateIn += updateIn.Seconds + "s";
                            }
                            else
                            {
                                strUpdateIn = "updating...";
                            }

                            item.SubItems[1].Text = strUpdateIn;
                            item.SubItems[2].Text = peerCount.ToString();
                        }
                    }
                    else
                    {
                        item.SubItems[1].Text = "inactive";
                        item.SubItems[2].Text = "";
                    }
                }
            }
        }

        private void lstPeerInfo_DoubleClick(object sender, EventArgs e)
        {
            showPeersToolStripMenuItem_Click(null, null);
        }

        private void showPeersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstPeerInfo.SelectedItems.Count > 0)
            {
                EndPoint[] peerEPs;

                switch (lstPeerInfo.SelectedItems[0].Text)
                {
                    case "IPv4 DHT":
                        peerEPs = _network.DhtGetIPv4Peers();
                        break;

                    case "IPv6 DHT":
                        peerEPs = _network.DhtGetIPv6Peers();
                        break;

                    case "LAN DHT":
                        peerEPs = _network.DhtGetLanPeers();
                        break;

                    case "Tor DHT":
                        peerEPs = _network.DhtGetTorPeers();
                        break;

                    case "IPv4 Tcp Relay":
                        peerEPs = _network.TcpRelayGetIPv4Peers();
                        break;

                    case "IPv6 Tcp Relay":
                        peerEPs = _network.TcpRelayGetIPv6Peers();
                        break;

                    default:
                        return;
                }

                string peers = "";

                foreach (EndPoint peerEP in peerEPs)
                    peers += peerEP.ToString() + "\r\n";

                if (peers == "")
                    MessageBox.Show("No peer returned by " + lstPeerInfo.SelectedItems[0].Text + ".", "No Peer Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(peers, "Peers List", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void chkShowSecret_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowSecret.Checked)
                txtPassword.PasswordChar = '\0';
            else
                txtPassword.PasswordChar = '#';
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion

        #region properties

        public string SharedSecret
        { get { return txtPassword.Text; } }

        public bool LocalNetworkOnly
        { get { return chkLocalNetworkOnly.Checked; } }

        #endregion
    }
}
