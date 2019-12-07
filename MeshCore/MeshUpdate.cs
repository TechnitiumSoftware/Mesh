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
using System.Reflection;
using System.Text;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;
using TechnitiumLibrary.Net.Proxy;

namespace MeshCore
{
    public delegate void UpdateCheckFailed(MeshUpdate sender, Exception ex);

    public class MeshUpdate : IDisposable
    {
        #region events

        public event EventHandler UpdateAvailable;
        public event EventHandler NoUpdateAvailable;
        public event UpdateCheckFailed UpdateCheckFailed;

        #endregion

        #region variables

        readonly Uri _updateCheckUri;
        readonly int _updateCheckInterval;

        readonly string _currentVersion;
        readonly SynchronizationContext _syncCxt = SynchronizationContext.Current;
        readonly Timer _updateCheckTimer;
        NetProxy _proxy = NetProxy.GetDefaultProxy();

        const int UPDATE_CHECK_TIMER_INITIAL_INTERVAL = 1000;
        const int UPDATE_CHECK_TIMER_PERIODIC_INTERVAL = 1 * 60 * 60 * 1000;

        string _updateVersion;
        string _displayText;
        string _downloadLink;
        DateTime _updateCheckedAt;

        bool _manualCheck;

        #endregion

        #region constructor

        public MeshUpdate(Uri updateCheckUri, int updateCheckInterval)
        {
            _updateCheckUri = updateCheckUri;
            _updateCheckInterval = updateCheckInterval;

            Assembly assembly = Assembly.GetEntryAssembly();
            AssemblyName assemblyName = assembly.GetName();

            _currentVersion = assemblyName.Version.ToString();

            _updateCheckTimer = new Timer(CheckForUpdateAsync, null, UPDATE_CHECK_TIMER_INITIAL_INTERVAL, UPDATE_CHECK_TIMER_PERIODIC_INTERVAL);
        }

        #endregion

        #region IDisposable 

        bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_updateCheckTimer != null)
                    _updateCheckTimer.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region static

        public static void CreateUpdateInfo(Stream s, string version, string displayText, string downloadLink)
        {
            BinaryWriter bW = new BinaryWriter(s);

            bW.Write(Encoding.ASCII.GetBytes("MU")); //format
            bW.Write((byte)1); //version

            bW.WriteShortString(version);
            bW.WriteShortString(displayText);
            bW.WriteShortString(downloadLink);
        }

        private static bool IsUpdateAvailable(string currentVersion, string updateVersion)
        {
            if (updateVersion == null)
                return false;

            string[] uVer = updateVersion.Split(new char[] { '.' });
            string[] cVer = currentVersion.Split(new char[] { '.' });

            int x = uVer.Length;
            if (x > cVer.Length)
                x = cVer.Length;

            for (int i = 0; i < x; i++)
            {
                if (Convert.ToInt32(uVer[i]) > Convert.ToInt32(cVer[i]))
                    return true;
                else if (Convert.ToInt32(uVer[i]) < Convert.ToInt32(cVer[i]))
                    return false;
            }

            if (uVer.Length > cVer.Length)
            {
                for (int i = x; i < uVer.Length; i++)
                {
                    if (Convert.ToInt32(uVer[i]) > 0)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region private

        private void CheckForUpdateAsync(object state)
        {
            try
            {
                if (!_manualCheck & (_updateCheckedAt.AddMilliseconds(_updateCheckInterval) > DateTime.UtcNow))
                    return;

                using (WebClientEx wC = new WebClientEx())
                {
                    wC.Proxy = _proxy;
                    wC.Timeout = 30000;

                    byte[] response = wC.DownloadData(_updateCheckUri);

                    using (BinaryReader bR = new BinaryReader(new MemoryStream(response, false)))
                    {
                        if (Encoding.ASCII.GetString(bR.ReadBytes(2)) != "MU") //format
                            throw new InvalidDataException("Mesh update info format is invalid.");

                        switch (bR.ReadByte()) //version
                        {
                            case 1:
                                _updateVersion = bR.ReadShortString();
                                _displayText = bR.ReadShortString();
                                _downloadLink = bR.ReadShortString();
                                break;

                            default:
                                throw new InvalidDataException("Mesh update info version not supported.");
                        }

                        if (IsUpdateAvailable(_currentVersion, _updateVersion))
                        {
                            _syncCxt.Send(delegate (object state2)
                            {
                                UpdateAvailable?.Invoke(this, EventArgs.Empty);
                            }, null);
                        }
                        else if (_manualCheck)
                        {
                            _syncCxt.Send(delegate (object state2)
                            {
                                NoUpdateAvailable?.Invoke(this, EventArgs.Empty);
                            }, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_manualCheck)
                {
                    _syncCxt.Send(delegate (object state2)
                    {
                        UpdateCheckFailed?.Invoke(this, ex);
                    }, null);
                }
            }
            finally
            {
                _manualCheck = false;
                _updateCheckedAt = DateTime.UtcNow;
            }
        }

        #endregion

        #region public

        public void CheckForUpdate()
        {
            if (_manualCheck)
                return;

            _manualCheck = true;
            _updateCheckTimer.Change(1000, UPDATE_CHECK_TIMER_PERIODIC_INTERVAL);
        }

        #endregion

        #region properties

        public Uri UpdateCheckUri
        { get { return _updateCheckUri; } }

        public int UpdateCheckInterval
        { get { return _updateCheckInterval; } }

        public string CurrentVersion
        { get { return _currentVersion; } }

        public string UpdateVersion
        { get { return _updateVersion; } }

        public string DisplayText
        { get { return _displayText; } }

        public string DownloadLink
        { get { return _downloadLink; } }

        public NetProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        #endregion
    }
}
