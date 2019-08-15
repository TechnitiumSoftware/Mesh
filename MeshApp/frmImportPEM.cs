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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using TechnitiumLibrary.Security.Cryptography;

namespace MeshApp
{
    public partial class frmImportPEM : Form
    {
        #region variables

        RSAParameters _privateKey;

        #endregion

        #region form code

        public frmImportPEM()
        {
            InitializeComponent();
        }

        private void txtRSAKey_TextChanged(object sender, EventArgs e)
        {
            if (!txtRSAKey.Text.Contains("\r\n"))
                txtRSAKey.Text = txtRSAKey.Text.Replace("\n", "\r\n");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (MemoryStream mS = new MemoryStream(Encoding.UTF8.GetBytes(txtRSAKey.Text)))
                {
                    _privateKey = PEMFormat.ReadRSAPrivateKey(mS);
                }

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(_privateKey);

                    if (rsa.KeySize < 2048)
                    {
                        MessageBox.Show("The RSA private key must be at least 2048-bit. The current key is " + rsa.KeySize + "-bit.", "Short RSA Private Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Error in reading PEM format. Please make sure you have pasted the RSA private key in a proper PEM format.", "Invalid PEM Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region properties

        public RSAParameters PrivateKey
        { get { return _privateKey; } }

        #endregion
    }
}
