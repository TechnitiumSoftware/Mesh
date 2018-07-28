/*
Technitium Mesh
Copyright (C) 2018  Shreyas Zare (shreyas@technitium.com)

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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Web.UI;
using TechnitiumLibrary.Net;

namespace ConnectivityWebService
{
    public partial class check : Page
    {
        const int CONNECTION_TIMEOUT = 10000;
        const int SOCKET_SEND_TIMEOUT = 10000;
        const int SOCKET_RECV_TIMEOUT = 10000;

        const int MAINTENANCE_TIMER_INTERVAL = 30000;
        const int PEER_ENDPOINT_EXPIRY_INTERVAL = 900000;

        static readonly Dictionary<IPEndPoint, DateTime> _openPeerEPs = new Dictionary<IPEndPoint, DateTime>();
        static readonly ReaderWriterLockSlim _openPeerEPsLock = new ReaderWriterLockSlim();
        static readonly System.Threading.Timer _maintenanceTimer;

        static check()
        {
            _maintenanceTimer = new System.Threading.Timer(delegate (object state)
            {
                _openPeerEPsLock.EnterWriteLock();
                try
                {
                    DateTime currentTime = DateTime.UtcNow;
                    List<IPEndPoint> expiredPeerEPs = new List<IPEndPoint>();

                    foreach (KeyValuePair<IPEndPoint, DateTime> peerEP in _openPeerEPs)
                    {
                        if ((currentTime - peerEP.Value).TotalMilliseconds > PEER_ENDPOINT_EXPIRY_INTERVAL)
                            expiredPeerEPs.Add(peerEP.Key);
                    }

                    foreach (IPEndPoint peerEP in expiredPeerEPs)
                        _openPeerEPs.Remove(peerEP);
                }
                catch
                {
                    //ignore errors
                }
                finally
                {
                    _openPeerEPsLock.ExitWriteLock();
                }
            }, null, MAINTENANCE_TIMER_INTERVAL, MAINTENANCE_TIMER_INTERVAL);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(Request.UserHostAddress), ushort.Parse(Request.QueryString["port"]));
            bool success = false;

            try
            {
                using (Socket socket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult result = socket.BeginConnect(remoteEP, null, null);
                    if (!result.AsyncWaitHandle.WaitOne(CONNECTION_TIMEOUT))
                        throw new SocketException((int)SocketError.TimedOut);

                    socket.NoDelay = true;
                    socket.SendTimeout = SOCKET_SEND_TIMEOUT;
                    socket.ReceiveTimeout = SOCKET_RECV_TIMEOUT;

                    MakeDecoyHttpConnection(new NetworkStream(socket), remoteEP);

                    success = true;
                }
            }
            catch
            { }

            List<IPEndPoint> peerEPs = new List<IPEndPoint>();
            bool add = false;

            _openPeerEPsLock.EnterReadLock();
            try
            {
                foreach (IPEndPoint peerEP in _openPeerEPs.Keys)
                {
                    if (!remoteEP.Equals(peerEP))
                        peerEPs.Add(peerEP);
                }

                if (success)
                {
                    if (_openPeerEPs.Count < 100)
                    {
                        if (_openPeerEPs.ContainsKey(remoteEP))
                            _openPeerEPs[remoteEP] = DateTime.UtcNow;
                        else
                            add = true;
                    }
                }
            }
            finally
            {
                _openPeerEPsLock.ExitReadLock();
            }

            if (add)
            {
                _openPeerEPsLock.EnterWriteLock();
                try
                {
                    if (_openPeerEPs.Count < 100)
                        _openPeerEPs.Add(remoteEP, DateTime.UtcNow);
                }
                finally
                {
                    _openPeerEPsLock.ExitWriteLock();
                }
            }

            Response.ContentType = "application/octet-stream";

            using (MemoryStream mS = new MemoryStream(20))
            {
                BinaryWriter bW = new BinaryWriter(mS);

                bW.Write((byte)1); //version
                bW.Write(success); //test status
                remoteEP.WriteTo(bW); //self end-point

                //write peers
                bW.Write(Convert.ToByte(peerEPs.Count));
                foreach (IPEndPoint peerEP in peerEPs)
                    peerEP.WriteTo(bW);

                byte[] output = mS.ToArray();

                Response.AddHeader("Content-Length", output.Length.ToString());
                Response.OutputStream.Write(output, 0, output.Length);
                Response.OutputStream.Flush();
            }

            Response.End();
        }

        private static void MakeDecoyHttpConnection(Stream networkStream, IPEndPoint remotePeerEP)
        {
            //write http request
            string httpHeaders = "GET / HTTP/1.1\r\nHost: $HOST\r\nConnection: keep-alive\r\nUser-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8\r\nAccept-Encoding: gzip, deflate\r\nAccept-Language: en-GB,en-US;q=0.8,en;q=0.6\r\n\r\n";

            httpHeaders = httpHeaders.Replace("$HOST", remotePeerEP.ToString());
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(httpHeaders);

            networkStream.Write(buffer, 0, buffer.Length);

            //read http response
            int byteRead;
            int crlfCount = 0;

            while (true)
            {
                byteRead = networkStream.ReadByte();
                switch (byteRead)
                {
                    case '\r':
                    case '\n':
                        crlfCount++;
                        break;

                    case -1:
                        throw new EndOfStreamException();

                    default:
                        crlfCount = 0;
                        break;
                }

                if (crlfCount == 4)
                    break; //http request completed
            }
        }
    }
}