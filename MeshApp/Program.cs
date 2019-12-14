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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using TechnitiumLibrary.Net.Firewall;

namespace MeshApp
{
    static class Program
    {
        #region variables

        public const string MUTEX_NAME = "MeshApp";
        public readonly static Uri UPDATE_URI_WINDOWS_PORTABLE_APP = new Uri("https://go.technitium.com/?id=29");
        public readonly static Uri UPDATE_URI_WINDOWS_SETUP_APP = new Uri("https://go.technitium.com/?id=30");
        public const int UPDATE_CHECK_INTERVAL = 1 * 24 * 60 * 60 * 1000; //1 day

        static bool _firewallEntryExists;
        static Mutex _app;

        #endregion

        #region public

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                #region check windows firewall entry

                string appPath = Assembly.GetEntryAssembly().Location;
                _firewallEntryExists = WindowsFirewallEntryExists(appPath);

                if (!_firewallEntryExists)
                {
                    bool isAdmin = (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);

                    if (isAdmin)
                    {
                        _firewallEntryExists = AddWindowsFirewallEntry(appPath);
                    }
                    else
                    {
                        ProcessStartInfo processInfo = new ProcessStartInfo(appPath, string.Join(" ", args));

                        processInfo.UseShellExecute = true;
                        processInfo.Verb = "runas";

                        try
                        {
                            Process.Start(processInfo);
                            return;
                        }
                        catch
                        { }
                    }
                }

                #endregion

                #region check for multiple instances

                bool createdNewMutex;

                _app = new Mutex(true, MUTEX_NAME, out createdNewMutex);

                if (!createdNewMutex)
                {
                    MessageBox.Show("Mesh App is already running. Please click on the Mesh App system tray icon to open the chat window.", "Mesh App Already Running!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                #endregion

                Application.Run(new frmProfileManager(Path.Combine(Path.GetDirectoryName(appPath), "tor", "tor.exe")));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex.ToString() + "\r\n\r\nClick OK to quit the application.", "Error - Mesh App", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool FirewallEntryExists
        { get { return _firewallEntryExists; } }

        #endregion

        #region download folder

        public static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                IntPtr pathPtr;
                int hr = SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                if (hr == 0)
                {
                    string path = Marshal.PtrToStringUni(pathPtr);
                    Marshal.FreeCoTaskMem(pathPtr);
                    return path;
                }
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

        #endregion

        #region private

        private static bool WindowsFirewallEntryExists(string appPath)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    if (Environment.OSVersion.Version.Major > 5)
                    {
                        //vista and above
                        try
                        {
                            return WindowsFirewall.RuleExistsVista("", appPath) == RuleStatus.Allowed;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            return WindowsFirewall.ApplicationExists(appPath) == RuleStatus.Allowed;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                default:
                    return false;
            }
        }

        private static bool AddWindowsFirewallEntry(string appPath)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    if (Environment.OSVersion.Version.Major > 5)
                    {
                        //vista and above
                        try
                        {
                            RuleStatus status = WindowsFirewall.RuleExistsVista("", appPath);

                            switch (status)
                            {
                                case RuleStatus.Blocked:
                                case RuleStatus.Disabled:
                                    WindowsFirewall.RemoveRuleVista("", appPath);
                                    break;

                                case RuleStatus.Allowed:
                                    return true;
                            }

                            WindowsFirewall.AddRuleVista("Mesh App", "Allow incoming connection request to Mesh App.", FirewallAction.Allow, appPath, Protocol.ANY, null, null, null, null, InterfaceTypeFlags.All, true, Direction.Inbound, true);
                            return true;
                        }
                        catch
                        { }
                    }
                    else
                    {
                        try
                        {
                            RuleStatus status = WindowsFirewall.ApplicationExists(appPath);

                            switch (status)
                            {
                                case RuleStatus.Disabled:
                                    WindowsFirewall.RemoveApplication(appPath);
                                    break;

                                case RuleStatus.Allowed:
                                    return true;
                            }

                            WindowsFirewall.AddApplication("Mesh App", appPath);
                            return true;
                        }
                        catch
                        { }
                    }

                    break;
            }

            return false;
        }

        #endregion
    }
}
