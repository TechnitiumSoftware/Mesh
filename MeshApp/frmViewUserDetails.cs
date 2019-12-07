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
using MeshCore.Network.SecureChannel;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace MeshApp
{
    public partial class frmViewUserDetails : Form
    {
        #region variables

        MeshNetwork.Peer _peer;

        #endregion

        #region constructor

        public frmViewUserDetails()
        {
            InitializeComponent();
        }

        public frmViewUserDetails(MeshNetwork.Peer peer)
        {
            InitializeComponent();

            _peer = peer;

            //name
            {
                string name = _peer.ProfileDisplayName;

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

                if (_peer.IsOnline)
                    labIcon.BackColor = Color.FromArgb(102, 153, 255);
                else
                    labIcon.BackColor = Color.Gray;

                labName.Text = name;
            }

            //status
            labStatus.Text = _peer.ProfileStatusMessage;

            switch (_peer.ProfileStatus)
            {
                case MeshProfileStatus.Inactive:
                    labStatus.Text = "(Inactive) " + labStatus.Text;
                    break;

                case MeshProfileStatus.Busy:
                    labStatus.Text = "(Busy) " + labStatus.Text;
                    break;
            }

            //userId
            labUserId.Text = _peer.PeerUserId.ToString();

            //image icon
            if ((_peer.ProfileDisplayImage != null) && (_peer.ProfileDisplayImage.Length > 0))
            {
                using (MemoryStream mS = new MemoryStream(_peer.ProfileDisplayImage))
                {
                    picIcon.Image = new Bitmap(Image.FromStream(mS), picIcon.Size);
                }

                picIcon.Visible = true;
                labIcon.Visible = false;
            }

            //network status
            switch (_peer.ConnectivityStatus)
            {
                case MeshNetwork.PeerConnectivityStatus.NoNetwork:
                    labNetworkStatus.Text = "No Network";
                    labNetworkStatus.ForeColor = Color.DimGray;
                    picNetwork.Image = Properties.Resources.NoNetwork;
                    break;

                case MeshNetwork.PeerConnectivityStatus.PartialMeshNetwork:
                    labNetworkStatus.Text = "Partial Mesh Network";
                    labNetworkStatus.ForeColor = Color.OrangeRed;
                    picNetwork.Image = Properties.Resources.PartialNetwork;
                    break;

                case MeshNetwork.PeerConnectivityStatus.FullMeshNetwork:
                    labNetworkStatus.Text = "Full Mesh Network";
                    labNetworkStatus.ForeColor = Color.Green;
                    picNetwork.Image = Properties.Resources.FullNetwork;
                    break;

                default:
                    labNetworkStatus.Text = "Unknown";
                    labNetworkStatus.ForeColor = Color.DimGray;
                    picNetwork.Image = Properties.Resources.NoNetwork;
                    break;
            }

            //cipher suite
            if (_peer.CipherSuite == SecureChannelCipherSuite.None)
                labCipherSuite.Text = "Not applicable";
            else
                labCipherSuite.Text = _peer.CipherSuite.ToString();

            //connected with
            MeshNetworkPeerInfo[] connectedWith = _peer.ConnectedWith;

            foreach (MeshNetworkPeerInfo peerInfo in connectedWith)
            {
                string peerIPs = null;

                foreach (EndPoint peerEP in peerInfo.PeerEPs)
                {
                    if (peerIPs == null)
                        peerIPs = peerEP.ToString();
                    else
                        peerIPs += ", " + peerEP.ToString();
                }

                lstConnectedWith.Items.Add(peerInfo.ToString()).SubItems.Add(peerIPs);
            }

            //not connected with
            MeshNetworkPeerInfo[] notConnectedWith = _peer.NotConnectedWith;

            foreach (MeshNetworkPeerInfo peerInfo in notConnectedWith)
            {
                string peerIPs = null;

                foreach (EndPoint peerEP in peerInfo.PeerEPs)
                {
                    if (peerIPs == null)
                        peerIPs = peerEP.ToString();
                    else
                        peerIPs += ", " + peerEP.ToString();
                }

                lstNotConnectedWith.Items.Add(peerInfo.ToString()).SubItems.Add(peerIPs);
            }
        }

        #endregion

        #region form code

        private void labUserId_MouseUp(object sender, MouseEventArgs e)
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

        #endregion
    }
}
