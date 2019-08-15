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
using System.Collections.Generic;
using System.Windows.Forms;

namespace MeshApp
{
    public partial class frmForwardToNetwork : Form
    {
        List<MeshNetwork> _selectedNetworks;

        public frmForwardToNetwork(MeshNode node)
        {
            InitializeComponent();

            foreach (MeshNetwork networks in node.GetNetworks())
                checkedListBox1.Items.Add(networks);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _selectedNetworks = new List<MeshNetwork>();

            foreach (MeshNetwork network in checkedListBox1.CheckedItems)
                _selectedNetworks.Add(network);

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public List<MeshNetwork> SelectedNetworks
        { get { return _selectedNetworks; } }
    }
}
