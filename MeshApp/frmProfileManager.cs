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
using MeshCore.Network.SecureChannel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TechnitiumLibrary.Net.Tor;

namespace MeshApp
{
    public partial class frmProfileManager : Form
    {
        #region variables

        readonly MeshUpdate _meshUpdate;
        readonly TorController _torController;
        string _profileFolder;
        bool _isPortableApp;

        readonly Dictionary<string, frmMain> _runningProfiles = new Dictionary<string, frmMain>();

        bool _createProfileWindowShown;

        #endregion

        #region constructor

        public frmProfileManager(string torExecutableFilePath)
        {
            InitializeComponent();

            //init mesh update
            _meshUpdate = new MeshUpdate(Program.UPDATE_URI, Program.UPDATE_CHECK_INTERVAL);
            _meshUpdate.UpdateAvailable += meshUpdate_UpdateAvailable;
            _meshUpdate.NoUpdateAvailable += meshUpdate_NoUpdateAvailable;
            _meshUpdate.UpdateCheckFailed += meshUpdate_UpdateCheckFailed;

            //init tor controller
            _torController = new TorController(torExecutableFilePath);
            _torController.Socks5EndPoint = new IPEndPoint(IPAddress.Loopback, 9950);
            _torController.ControlPort = 9951;
        }

        #endregion

        #region private

        private void RefreshProfileList()
        {
            string[] profiles = Directory.GetFiles(Environment.CurrentDirectory, "*.profile", SearchOption.TopDirectoryOnly);

            if (profiles.Length > 0)
            {
                _profileFolder = Environment.CurrentDirectory;
                _isPortableApp = true;
            }
            else
            {
                _profileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Technitium", "Mesh");
                _isPortableApp = false;

                if (!Directory.Exists(_profileFolder))
                    Directory.CreateDirectory(_profileFolder);

                profiles = Directory.GetFiles(_profileFolder, "*.profile", SearchOption.TopDirectoryOnly);
            }

            cmbProfiles.Items.Clear();

            foreach (string profile in profiles)
            {
                if (profile.EndsWith(".profile"))
                {
                    string profileName = Path.GetFileNameWithoutExtension(profile);
                    if (!string.IsNullOrWhiteSpace(profileName))
                    {
                        bool profileRunning = false;

                        foreach (KeyValuePair<string, frmMain> frm in _runningProfiles)
                        {
                            if (frm.Key.Equals(profileName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                profileRunning = true;
                                break;
                            }
                        }

                        if (!profileRunning)
                            cmbProfiles.Items.Add(profileName);
                    }
                }
            }

            if (cmbProfiles.Items.Count > 0)
                cmbProfiles.SelectedIndex = 0;
        }

        private string GetDownloadFolder()
        {
            if (_isPortableApp)
            {
                string downloadFolder = Path.Combine(_profileFolder, "Downloads");
                if (!Directory.Exists(downloadFolder))
                    Directory.CreateDirectory(downloadFolder);

                return downloadFolder;
            }
            else
            {
                return Program.GetDownloadsPath();
            }
        }

        private void CreateAndStartProfile()
        {
            if (_createProfileWindowShown)
                return;

            _createProfileWindowShown = true;

            using (frmCreateProfile frm = new frmCreateProfile())
            {
                frm.Activate();

                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    int localServicePort;

                    if (frm.NodeType == MeshNodeType.P2P)
                        localServicePort = (new Random()).Next(10000, 65000); //fixed random port for p2p
                    else
                        localServicePort = 0; //new random port at startup for Tor node

                    MeshNode node = new MeshNode(frm.NodeType, frm.PrivateKey, SecureChannelCipherSuite.ECDHE256_RSA2048_WITH_AES256_CBC_HMAC_SHA256 | SecureChannelCipherSuite.DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256, Convert.ToUInt16(localServicePort), frm.ProfileDisplayName, _profileFolder, GetDownloadFolder(), _torController);

                    if (frm.NodeType == MeshNodeType.P2P)
                    {
                        //create default local network group for LAN user discovery
                        node.CreateGroupChat("Local Network", "", true);
                    }

                    string profileFilePath = Path.Combine(_profileFolder, frm.ProfileDisplayName + ".profile");

                    using (FileStream fS = new FileStream(profileFilePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        node.ChangePassword(frm.ProfilePassword);
                        node.SaveTo(fS);
                    }

                    LoadProfileMainForm(frm.ProfileDisplayName, node, profileFilePath);
                }
                else
                {
                    if (cmbProfiles.Items.Count > 0)
                    {
                        this.Show();
                        this.Activate();
                    }
                }
            }

            _createProfileWindowShown = false;
        }

        private void LoadProfileMainForm(string profileName, MeshNode node, string profileFilePath)
        {
            frmMain frmMain = new frmMain(node, profileFilePath, _isPortableApp, _meshUpdate, this);
            _runningProfiles.Add(profileName, frmMain);

            ToolStripMenuItem mnuItem = new ToolStripMenuItem(profileName);
            mnuItem.Click += mnuProfileMainForm_Click;

            mnuSystemTray.Items.Insert(2, mnuItem);

            this.Hide();
            txtPassword.Text = "";

            frmMain.Show();

            RefreshProfileList(); //refresh to remove loaded profile
        }

        internal void UnloadProfileMainForm(frmMain frm)
        {
            string profileName = Path.GetFileNameWithoutExtension(frm.ProfileFilePath);

            _runningProfiles.Remove(profileName);

            ToolStripItem mnuProfile = null;

            foreach (ToolStripItem item in mnuSystemTray.Items)
            {
                if (item.Text.Equals(profileName))
                {
                    mnuProfile = item;
                    break;
                }
            }

            if (mnuProfile != null)
                mnuSystemTray.Items.Remove(mnuProfile);

            RefreshProfileList(); //refresh to add unloaded profile

            if (_runningProfiles.Count == 0)
                mnuProfileManager_Click(null, null);
        }

        #endregion

        #region form code

        private void frmProfileManager_Shown(object sender, EventArgs e)
        {
            mnuProfileManager_Click(null, null);
        }

        private void frmProfileManager_Activated(object sender, EventArgs e)
        {
            //mnuProfileManager_Click(null, null);
        }

        private void btnCreateProfile_Click(object sender, EventArgs e)
        {
            try
            {
                CreateAndStartProfile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string profileFilePath = Path.Combine(_profileFolder, (cmbProfiles.SelectedItem as string) + ".profile");

            try
            {
                using (FileStream fS = new FileStream(profileFilePath, FileMode.Open, FileAccess.Read))
                {
                    MeshNode node = new MeshNode(fS, txtPassword.Text, _profileFolder, _torController);

                    if (_isPortableApp || !Directory.Exists(node.DownloadFolder))
                        node.DownloadFolder = GetDownloadFolder();

                    LoadProfileMainForm((cmbProfiles.SelectedItem as string), node, profileFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to permanently delete selected profile?\r\n\r\nWarning! This will delete the profile file permanently and hence cannot be undone.", "Delete Profile Permanently?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    File.Delete(Path.Combine(_profileFolder, (cmbProfiles.SelectedItem as string) + ".profile"));
                    File.Delete(Path.Combine(_profileFolder, (cmbProfiles.SelectedItem as string) + ".profile.bak"));

                    mnuProfileManager_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error! Cannot delete profile '" + (cmbProfiles.SelectedItem as string) + "' due to following error:\r\n\r\n" + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog oFD = new OpenFileDialog())
            {
                oFD.Filter = "Mesh Profile (*.profile)|*.profile";
                oFD.Title = "Import Mesh Profile ...";
                oFD.CheckFileExists = true;
                oFD.Multiselect = false;

                if (oFD.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(oFD.FileName, Path.Combine(_profileFolder, Path.GetFileNameWithoutExtension(oFD.FileName) + ".profile"));

                        mnuProfileManager_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error! Cannot import profile due to following error:\r\n\r\n" + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sFD = new SaveFileDialog())
            {
                sFD.Title = "Export Mesh Profile As...";
                sFD.Filter = "Mesh Profile (*.profile)|*.profile";
                sFD.DefaultExt = ".profile";
                sFD.FileName = (cmbProfiles.SelectedItem as string) + ".profile";
                sFD.CheckPathExists = true;
                sFD.OverwritePrompt = true;

                if (sFD.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(Path.Combine(_profileFolder, (cmbProfiles.SelectedItem as string) + ".profile"), sFD.FileName, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error! Cannot export profile due to following error:\r\n\r\n" + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_runningProfiles.Count > 0)
            {
                this.Hide();
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to exit Mesh?", "Exit Mesh?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Application.Exit();
            }
        }

        private void frmProfileManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (_runningProfiles.Count > 0)
                {
                    e.Cancel = true;
                    this.Hide();
                }
                else
                {
                    if (MessageBox.Show("Are you sure you want to exit Mesh?", "Exit Mesh?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void frmProfileManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (KeyValuePair<string, frmMain> frm in _runningProfiles)
                frm.Value.Close();

            _meshUpdate.Dispose();
            _torController.Dispose();
        }

        #endregion

        #region mesh update

        private void meshUpdate_UpdateCheckFailed(MeshUpdate sender, Exception ex)
        {
            mnuCheckUpdate.Enabled = true;
            MessageBox.Show("Error ocurred while checking for update:\r\n\r\n" + ex.ToString(), "Mesh Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void meshUpdate_NoUpdateAvailable(object sender, EventArgs e)
        {
            mnuCheckUpdate.Enabled = true;
            MessageBox.Show("No update was available.", "Mesh Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void meshUpdate_UpdateAvailable(object sender, EventArgs e)
        {
            mnuCheckUpdate.Enabled = true;

            if (MessageBox.Show("New update is available for download!\r\n\r\nCurrent Version: " + _meshUpdate.CurrentVersion + "\r\nUpdate Version: " + _meshUpdate.UpdateVersion + "\r\n\r\nDetails: " + _meshUpdate.DisplayText + "\r\n\r\nDownload Link: " + _meshUpdate.DownloadLink + "\r\n\r\nDo you want to download the latest update setup now?", "Mesh Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                System.Diagnostics.Process.Start(_meshUpdate.DownloadLink);
        }

        #endregion

        #region system tray menu

        private void mnuProfileMainForm_Click(object sender, EventArgs e)
        {
            ToolStripItem mnuItem = sender as ToolStripItem;

            foreach (KeyValuePair<string, frmMain> frm in _runningProfiles)
            {
                if (frm.Key.Equals(mnuItem.Text))
                {
                    frm.Value.Show();
                    frm.Value.Activate();
                    break;
                }
            }
        }

        private void mnuSystemTray_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mnuSeparator.Visible = (_runningProfiles.Count > 0);
        }

        private void iconSystemTray_BalloonTipClicked(object sender, EventArgs e)
        {
            object objFrm = iconSystemTray.Tag;

            if (objFrm != null)
            {
                frmMain frm = objFrm as frmMain;
                frm.Show();
                frm.Activate();
            }
        }

        private void mnuProfileManager_Click(object sender, EventArgs e)
        {
            RefreshProfileList();

            if (cmbProfiles.Items.Count == 0)
            {
                btnStart.Enabled = false;
                btnDelete.Enabled = false;
                btnExport.Enabled = false;

                CreateAndStartProfile();
            }
            else
            {
                btnStart.Enabled = true;
                btnDelete.Enabled = true;
                btnExport.Enabled = true;

                this.Show();
                this.Activate();

                txtPassword.Focus();
            }
        }

        private void mnuCheckUpdate_Click(object sender, EventArgs e)
        {
            mnuCheckUpdate.Enabled = false;
            _meshUpdate.CheckForUpdate();
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            using (frmAbout frm = new frmAbout())
            {
                frm.ShowDialog(this);
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit Mesh?", "Exit Mesh?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }

        #endregion

        #region properties

        public NotifyIcon SystemTrayIcon
        { get { return iconSystemTray; } }

        #endregion
    }
}
