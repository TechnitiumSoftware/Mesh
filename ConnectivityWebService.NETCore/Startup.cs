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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TechnitiumLibrary.Net;

namespace ConnectivityWebService.NETCore
{
    public class Startup
    {
        #region variables

        const int CONNECTION_TIMEOUT = 10000;
        const int SOCKET_SEND_TIMEOUT = 10000;
        const int SOCKET_RECV_TIMEOUT = 10000;

        const int MAINTENANCE_TIMER_INTERVAL = 60 * 1000;
        const int PEER_ENDPOINT_EXPIRY_INTERVAL = 1 * 60 * 60 * 1000; //1hr expiry

        const int MAX_PEER_ENDPOINTS = 100;

        static readonly List<PeerEndPoint> _openPeerEPs = new List<PeerEndPoint>(MAX_PEER_ENDPOINTS);
        static readonly ReaderWriterLockSlim _openPeerEPsLock = new ReaderWriterLockSlim();
        static readonly Timer _maintenanceTimer;

        #endregion

        #region constructor

        static Startup()
        {
            _maintenanceTimer = new Timer(delegate (object state)
            {
                try
                {
                    List<PeerEndPoint> expiredPeerEPs = new List<PeerEndPoint>();

                    _openPeerEPsLock.EnterReadLock();
                    try
                    {
                        foreach (PeerEndPoint peerEP in _openPeerEPs)
                        {
                            if (peerEP.HasExpired())
                                expiredPeerEPs.Add(peerEP);
                        }
                    }
                    finally
                    {
                        _openPeerEPsLock.ExitReadLock();
                    }

                    if (expiredPeerEPs.Count > 0)
                    {
                        _openPeerEPsLock.EnterWriteLock();
                        try
                        {
                            foreach (PeerEndPoint expiredPeerEP in expiredPeerEPs)
                                _openPeerEPs.Remove(expiredPeerEP);
                        }
                        finally
                        {
                            _openPeerEPsLock.ExitWriteLock();
                        }
                    }
                }
                catch
                { }

            }, null, MAINTENANCE_TIMER_INTERVAL, MAINTENANCE_TIMER_INTERVAL);
        }

        #endregion

        #region public

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await Task.Run(() =>
                {
                    if (context.Request.Path == "/connectivity/")
                    {
                        if (!ushort.TryParse(context.Request.Query["port"], out ushort port))
                        {
                            context.Response.StatusCode = 400;
                            return;
                        }

                        IPEndPoint remoteEP = new IPEndPoint(GetRequestRemoteAddress(context), port);
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
                        bool add;

                        _openPeerEPsLock.EnterReadLock();
                        try
                        {
                            add = (_openPeerEPs.Count < MAX_PEER_ENDPOINTS);

                            foreach (PeerEndPoint peerEP in _openPeerEPs)
                            {
                                if (remoteEP.Equals(peerEP.EndPoint))
                                {
                                    add = false;

                                    if (success)
                                        peerEP.AddedOn = DateTime.UtcNow;
                                }
                                else
                                {
                                    peerEPs.Add(peerEP.EndPoint);
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
                                if (_openPeerEPs.Count < MAX_PEER_ENDPOINTS)
                                    _openPeerEPs.Add(new PeerEndPoint(remoteEP));
                            }
                            finally
                            {
                                _openPeerEPsLock.ExitWriteLock();
                            }
                        }

                        context.Response.ContentType = "application/octet-stream";

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

                            context.Response.Headers.Add("Content-Length", output.Length.ToString());

                            using (context.Response.Body)
                            {
                                context.Response.Body.WriteAsync(output, 0, output.Length);
                            }
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                    }
                });
            });
        }

        #endregion

        #region private

        private IPAddress GetRequestRemoteAddress(HttpContext context)
        {
            try
            {
                if (NetUtilities.IsPrivateIP(context.Connection.RemoteIpAddress))
                {
                    //reverse proxy X-Real-IP header supported only when remote IP address is private

                    string xRealIp = context.Request.Headers["X-Real-IP"];
                    if (!string.IsNullOrEmpty(xRealIp))
                    {
                        //get the real IP address of the requesting client from X-Real-IP header set in nginx proxy_pass block
                        return IPAddress.Parse(xRealIp);
                    }
                }

                return context.Connection.RemoteIpAddress;
            }
            catch
            {
                return IPAddress.Any;
            }
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

        #endregion

        class PeerEndPoint
        {
            #region variables

            public IPEndPoint EndPoint;
            public DateTime AddedOn;

            #endregion

            #region constructor

            public PeerEndPoint(IPEndPoint endPoint)
            {
                this.EndPoint = endPoint;
                this.AddedOn = DateTime.UtcNow;
            }

            #endregion

            #region public

            public bool HasExpired()
            {
                return DateTime.UtcNow > AddedOn.AddMilliseconds(PEER_ENDPOINT_EXPIRY_INTERVAL);
            }

            public override bool Equals(object obj)
            {
                PeerEndPoint other = obj as PeerEndPoint;
                if (obj == null)
                    return false;

                return this.EndPoint.Equals(other.EndPoint);
            }

            public override int GetHashCode()
            {
                return this.EndPoint.GetHashCode();
            }

            #endregion
        }
    }
}
