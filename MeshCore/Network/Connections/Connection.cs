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

/*  Connection Frame
*   0                8                                 168                               184
*  +----------------+---------------//----------------+----------------+----------------+---------------//----------------+
*  | signal (1 byte)|     channel id  (32 bytes)      |     data length (uint16)        |              data               |
*  +----------------+---------------//----------------+----------------+----------------+---------------//----------------+
*  
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

namespace MeshCore.Network.Connections
{
    enum ConnectionSignal : byte
    {
        PingRequest = 1,
        PingResponse = 2,

        ConnectChannelMeshNetwork = 3,
        ChannelData = 4,
        DisconnectChannel = 5,
        ConnectChannelTunnel = 6,
        ConnectChannelVirtualConnection = 7,

        TcpRelayServerRegisterHostedNetwork = 8,
        TcpRelayServerUnregisterHostedNetwork = 9,
        MeshNetworkPeers = 10
    }

    public class Connection : IDisposable
    {
        #region variables

        const int CONNECTION_FRAME_BUFFER_SIZE = 8 * 1024;

        readonly ConnectionManager _connectionManager;
        readonly Stream _baseStream;
        readonly BinaryNumber _remotePeerId;
        readonly EndPoint _remotePeerEP;

        readonly Dictionary<BinaryNumber, ChannelStream> _channels = new Dictionary<BinaryNumber, ChannelStream>();

        Thread _readThread;

        readonly List<Joint> _tunnelJointList = new List<Joint>();

        int _channelWriteTimeout = 30000;

        const int TCP_RELAY_CLIENT_MODE_TIMER_INTERVAL = 30000;
        Timer _tcpRelayClientModeTimer;

        bool _tcpRelayServerModeEnabled = false;

        readonly object _lock = new object();
        readonly object _baseStreamLock = new object();

        #endregion

        #region constructor

        public Connection(ConnectionManager connectionManager, Stream baseStream, BinaryNumber remotePeerId, EndPoint remotePeerEP)
        {
            _connectionManager = connectionManager;
            _baseStream = baseStream;
            _remotePeerId = remotePeerId;
            _remotePeerEP = remotePeerEP;

            //start read thread
            _readThread = new Thread(ReadFrameAsync);
            _readThread.IsBackground = true;
            _readThread.Start();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    if (_readThread != null)
                        _readThread.Abort();

                    //dispose tcp relay mode timer
                    if (_tcpRelayClientModeTimer != null)
                        _tcpRelayClientModeTimer.Dispose();

                    //dispose all channels
                    List<ChannelStream> channels = new List<ChannelStream>();

                    lock (_channels)
                    {
                        channels.AddRange(_channels.Values);
                        _channels.Clear(); //clear to prevent sending disconnect signal from ChannelStream.Dispose()
                    }

                    foreach (ChannelStream channel in channels)
                        channel.Dispose();

                    //dispose base stream
                    lock (_baseStreamLock)
                    {
                        _baseStream.Dispose();
                    }

                    _connectionManager.ConnectionDisposed(this);
                }

                _disposed = true;
            }
        }

        #endregion

        #region private

        private void WriteFrame(ConnectionSignal signal, BinaryNumber channelId, byte[] buffer, int offset, int count)
        {
            int frameCount = CONNECTION_FRAME_BUFFER_SIZE;

            while (true)
            {
                if (count < frameCount)
                    frameCount = count;

                lock (_baseStreamLock)
                {
                    _baseStream.WriteByte((byte)signal); //write frame signal
                    _baseStream.Write(channelId.Value); //write channel id
                    _baseStream.Write(BitConverter.GetBytes(Convert.ToUInt16(frameCount)), 0, 2); //write data length

                    if (frameCount > 0)
                        _baseStream.Write(buffer, offset, frameCount); //write data

                    offset += frameCount;
                    count -= frameCount;

                    if (count < 1)
                    {
                        _baseStream.Flush(); //flush base stream
                        break;
                    }
                }
            }
        }

        private void ReadFrameAsync()
        {
            try
            {
                //frame parameters
                int signal;
                BinaryNumber channelId = new BinaryNumber(new byte[32]);
                byte[] dataLengthBuffer = new byte[2];
                OffsetStream dataStream = new OffsetStream(_baseStream, 0, 0, true, false);

                while (true)
                {
                    #region read frame from base stream

                    //read frame signal
                    signal = _baseStream.ReadByte();
                    if (signal == -1)
                        throw new EndOfStreamException();

                    //read channel id
                    _baseStream.ReadBytes(channelId.Value, 0, 32);

                    //read data length
                    _baseStream.ReadBytes(dataLengthBuffer, 0, 2);
                    dataStream.Reset(0, BitConverter.ToUInt16(dataLengthBuffer, 0), 0);

                    #endregion

                    switch ((ConnectionSignal)signal)
                    {
                        case ConnectionSignal.PingRequest:
                            WriteFrame(ConnectionSignal.PingResponse, channelId, null, 0, 0);
                            break;

                        case ConnectionSignal.PingResponse:
                            //do nothing!
                            break;

                        case ConnectionSignal.ConnectChannelMeshNetwork:
                            #region ConnectChannelMeshNetwork

                            lock (_channels)
                            {
                                if (_channels.ContainsKey(channelId))
                                {
                                    WriteFrame(ConnectionSignal.DisconnectChannel, channelId, null, 0, 0);
                                }
                                else
                                {
                                    ChannelStream channel = new ChannelStream(this, channelId.Clone());
                                    _channels.Add(channel.ChannelId, channel);

                                    ThreadPool.QueueUserWorkItem(delegate (object state)
                                    {
                                        try
                                        {
                                            //done async since the call is blocking and will block the current read thread which can cause DOS
                                            _connectionManager.Node.MeshNetworkRequest(this, channel.ChannelId, channel);
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.Write(this.GetType().Name, ex);

                                            channel.Dispose();
                                        }
                                    });
                                }
                            }

                            //check if tcp relay is hosted for the channel. reply back tcp relay peers list if available
                            Connection[] connections = _connectionManager.GetTcpRelayServerHostedNetworkConnections(channelId);

                            if (connections.Length > 0)
                            {
                                int count = connections.Length;

                                for (int i = 0; i < connections.Length; i++)
                                {
                                    if (connections[i].RemotePeerEP.Equals(_remotePeerEP))
                                    {
                                        connections[i] = null;
                                        count--;
                                        break;
                                    }
                                }

                                using (MemoryStream mS = new MemoryStream(128))
                                {
                                    BinaryWriter bW = new BinaryWriter(mS);

                                    bW.Write(Convert.ToByte(count));

                                    foreach (Connection connection in connections)
                                    {
                                        if (connection != null)
                                            connection.RemotePeerEP.WriteTo(bW);
                                    }

                                    byte[] data = mS.ToArray();

                                    WriteFrame(ConnectionSignal.MeshNetworkPeers, channelId, data, 0, data.Length);
                                }
                            }

                            #endregion
                            break;

                        case ConnectionSignal.ChannelData:
                            #region ChannelData

                            try
                            {
                                ChannelStream channel = null;

                                lock (_channels)
                                {
                                    channel = _channels[channelId];
                                }

                                channel.FeedReadBuffer(dataStream, _channelWriteTimeout);
                            }
                            catch
                            { }

                            #endregion
                            break;

                        case ConnectionSignal.DisconnectChannel:
                            #region DisconnectChannel

                            try
                            {
                                ChannelStream channel;

                                lock (_channels)
                                {
                                    channel = _channels[channelId];
                                    _channels.Remove(channelId); //remove here to prevent sending disconnect signal from ChannelStream.Dispose()
                                }

                                channel.Dispose();
                            }
                            catch
                            { }

                            #endregion
                            break;

                        case ConnectionSignal.ConnectChannelTunnel:
                            #region ConnectChannelTunnel

                            if (IsStreamVirtualConnection(_baseStream))
                            {
                                //nesting virtual connections not allowed
                                WriteFrame(ConnectionSignal.DisconnectChannel, channelId, null, 0, 0);
                            }
                            else
                            {
                                ChannelStream remoteChannel1 = null;

                                lock (_channels)
                                {
                                    if (_channels.ContainsKey(channelId))
                                    {
                                        WriteFrame(ConnectionSignal.DisconnectChannel, channelId, null, 0, 0);
                                    }
                                    else
                                    {
                                        //add first stream into list
                                        remoteChannel1 = new ChannelStream(this, channelId.Clone());
                                        _channels.Add(remoteChannel1.ChannelId, remoteChannel1);
                                    }
                                }

                                if (remoteChannel1 != null)
                                {
                                    EndPoint tunnelToRemotePeerEP = ConvertChannelIdToEp(channelId); //get remote peer ep                                    
                                    Connection remotePeerConnection = _connectionManager.GetExistingConnection(tunnelToRemotePeerEP); //get remote channel service

                                    if (remotePeerConnection == null)
                                    {
                                        remoteChannel1.Dispose();
                                    }
                                    else
                                    {
                                        try
                                        {
                                            //get remote proxy connection channel stream
                                            ChannelStream remoteChannel2 = remotePeerConnection.MakeVirtualConnection(_remotePeerEP);

                                            //join current and remote stream
                                            Joint joint = new Joint(remoteChannel1, remoteChannel2, delegate (object state)
                                            {
                                                lock (_tunnelJointList)
                                                {
                                                    _tunnelJointList.Remove(state as Joint);
                                                }
                                            });

                                            lock (_tunnelJointList)
                                            {
                                                _tunnelJointList.Add(joint);
                                            }

                                            joint.Start();
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.Write(this.GetType().Name, ex);

                                            remoteChannel1.Dispose();
                                        }
                                    }
                                }
                            }

                            #endregion
                            break;

                        case ConnectionSignal.ConnectChannelVirtualConnection:
                            #region ConnectChannelVirtualConnection

                            if (IsStreamVirtualConnection(_baseStream))
                            {
                                //nesting virtual connections not allowed
                                WriteFrame(ConnectionSignal.DisconnectChannel, channelId, null, 0, 0);
                            }
                            else
                            {
                                lock (_channels)
                                {
                                    if (_channels.ContainsKey(channelId))
                                    {
                                        WriteFrame(ConnectionSignal.DisconnectChannel, channelId, null, 0, 0);
                                    }
                                    else
                                    {
                                        //add proxy channel stream into list
                                        ChannelStream channel = new ChannelStream(this, channelId.Clone());
                                        _channels.Add(channel.ChannelId, channel);

                                        //pass channel as connection async
                                        ThreadPool.QueueUserWorkItem(delegate (object state)
                                        {
                                            try
                                            {
                                                _connectionManager.AcceptConnectionInitiateProtocol(channel, ConvertChannelIdToEp(channel.ChannelId));
                                            }
                                            catch (Exception ex)
                                            {
                                                Debug.Write(this.GetType().Name, ex);

                                                channel.Dispose();
                                            }
                                        });
                                    }
                                }
                            }

                            #endregion
                            break;

                        case ConnectionSignal.TcpRelayServerRegisterHostedNetwork:
                            #region TcpRelayServerRegisterHostedNetwork

                            _connectionManager.TcpRelayServerRegisterHostedNetwork(this, channelId.Clone());

                            _tcpRelayServerModeEnabled = true;

                            #endregion
                            break;

                        case ConnectionSignal.TcpRelayServerUnregisterHostedNetwork:
                            #region TcpRelayServerUnregisterHostedNetwork

                            _connectionManager.TcpRelayServerUnregisterHostedNetwork(this, channelId);

                            #endregion
                            break;

                        case ConnectionSignal.MeshNetworkPeers:
                            #region MeshNetworkPeers
                            {
                                BinaryReader bR = new BinaryReader(dataStream);

                                int count = bR.ReadByte();
                                List<EndPoint> peerEPs = new List<EndPoint>(count);

                                for (int i = 0; i < count; i++)
                                    peerEPs.Add(EndPointExtension.Parse(bR));

                                _connectionManager.Node.ReceivedMeshNetworkPeersViaTcpRelay(this, channelId, peerEPs);
                            }
                            #endregion
                            break;

                        default:
                            throw new IOException("Invalid frame signal.");
                    }

                    //discard any unread data
                    if (dataStream.Length > dataStream.Position)
                        dataStream.CopyTo(Stream.Null, 1024, Convert.ToInt32(dataStream.Length - dataStream.Position));
                }
            }
            catch (ThreadAbortException)
            {
                //stopping, do nothing
            }
            catch (EndOfStreamException)
            {
                //gracefull close
                _readThread = null; //to avoid self abort call in Dispose()
                Dispose();
            }
            catch (Exception ex)
            {
                Debug.Write(this.GetType().Name, ex);

                _readThread = null; //to avoid self abort call in Dispose()
                Dispose();
            }
        }

        private BinaryNumber ConvertEpToChannelId(EndPoint ep)
        {
            byte[] buffer = new byte[32];

            using (MemoryStream mS = new MemoryStream(buffer))
            {
                ep.WriteTo(new BinaryWriter(mS));
            }

            return new BinaryNumber(buffer);
        }

        private EndPoint ConvertChannelIdToEp(BinaryNumber channelId)
        {
            using (MemoryStream mS = new MemoryStream(channelId.Value, false))
            {
                return EndPointExtension.Parse(new BinaryReader(mS));
            }
        }

        private ChannelStream MakeVirtualConnection(EndPoint forPeerEP)
        {
            BinaryNumber channelId = ConvertEpToChannelId(forPeerEP);
            ChannelStream channel;

            lock (_channels)
            {
                if (_channels.ContainsKey(channelId))
                    throw new InvalidOperationException("Channel already exists.");

                channel = new ChannelStream(this, channelId);
                _channels.Add(channelId, channel);
            }

            //send signal
            WriteFrame(ConnectionSignal.ConnectChannelVirtualConnection, channelId, null, 0, 0);

            return channel;
        }

        #endregion

        #region static

        public static bool IsStreamVirtualConnection(Stream stream)
        {
            return (stream.GetType() == typeof(ChannelStream));
        }

        #endregion

        #region public

        public Stream ConnectMeshNetwork(BinaryNumber channelId)
        {
            ChannelStream channel;

            lock (_channels)
            {
                if (_channels.ContainsKey(channelId))
                    throw new InvalidOperationException("Channel already exists.");

                channel = new ChannelStream(this, channelId);
                _channels.Add(channel.ChannelId, channel);
            }

            //send connect signal
            WriteFrame(ConnectionSignal.ConnectChannelMeshNetwork, channelId, null, 0, 0);

            return channel;
        }

        public Stream MakeTunnelConnection(EndPoint remotePeerEP)
        {
            BinaryNumber channelId = ConvertEpToChannelId(remotePeerEP);
            ChannelStream channel;

            lock (_channels)
            {
                if (_channels.ContainsKey(channelId))
                    throw new InvalidOperationException("Channel already exists.");

                channel = new ChannelStream(this, channelId);
                _channels.Add(channelId, channel);
            }

            //send signal
            WriteFrame(ConnectionSignal.ConnectChannelTunnel, channelId, null, 0, 0);

            return channel;
        }

        public bool ChannelExists(BinaryNumber channelId)
        {
            lock (_channels)
            {
                return _channels.ContainsKey(channelId);
            }
        }

        public void EnableTcpRelayClientMode()
        {
            if (_tcpRelayClientModeTimer == null)
            {
                _tcpRelayClientModeTimer = new Timer(delegate (object state)
                {
                    try
                    {
                        WriteFrame(ConnectionSignal.PingRequest, BinaryNumber.GenerateRandomNumber256(), null, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(this.GetType().Name, ex);
                    }
                }, null, 1000, TCP_RELAY_CLIENT_MODE_TIMER_INTERVAL);
            }
        }

        public void TcpRelayRegisterHostedNetwork(BinaryNumber networkId)
        {
            WriteFrame(ConnectionSignal.TcpRelayServerRegisterHostedNetwork, networkId, null, 0, 0);
        }

        public void TcpRelayUnregisterHostedNetwork(BinaryNumber networkId)
        {
            WriteFrame(ConnectionSignal.TcpRelayServerUnregisterHostedNetwork, networkId, null, 0, 0);
        }

        #endregion

        #region properties

        public BinaryNumber LocalPeerId
        { get { return _connectionManager.LocalPeerId; } }

        public BinaryNumber RemotePeerId
        { get { return _remotePeerId; } }

        public EndPoint RemotePeerEP
        { get { return _remotePeerEP; } }

        public EndPoint ViaRemotePeerEP
        {
            get
            {
                if (IsStreamVirtualConnection(_baseStream))
                    return (_baseStream as ChannelStream).Connection.RemotePeerEP;
                else
                    return null;
            }
        }

        public int ChannelWriteTimeout
        {
            get { return _channelWriteTimeout; }
            set { _channelWriteTimeout = value; }
        }

        public bool IsVirtualConnection
        { get { return (_baseStream.GetType() == typeof(ChannelStream)); } }

        public bool IsTcpRelayClientModeEnabled
        { get { return (_tcpRelayClientModeTimer != null); } }

        public bool IsTcpRelayServerModeEnabled
        { get { return _tcpRelayServerModeEnabled; } }

        #endregion

        private class ChannelStream : Stream
        {
            #region variables

            const int CHANNEL_READ_TIMEOUT = 60000; //channel read timeout; application must PING to keep alive
            const int CHANNEL_WRITE_TIMEOUT = 30000; //dummy timeout for write since base channel write timeout will be used

            readonly Connection _connection;
            readonly BinaryNumber _channelId;

            readonly byte[] _readBuffer = new byte[CONNECTION_FRAME_BUFFER_SIZE];
            volatile int _readBufferPosition;
            volatile int _readBufferCount;

            int _readTimeout = CHANNEL_READ_TIMEOUT;
            int _writeTimeout = CHANNEL_WRITE_TIMEOUT;

            readonly object _lock = new object();

            #endregion

            #region constructor

            public ChannelStream(Connection connection, BinaryNumber channelId)
            {
                _connection = connection;
                _channelId = channelId;
            }

            #endregion

            #region IDisposable

            bool _disposed = false;

            protected override void Dispose(bool disposing)
            {
                lock (_lock)
                {
                    if (_disposed)
                        return;

                    if (disposing)
                    {
                        bool removed;

                        lock (_connection._channels)
                        {
                            removed = _connection._channels.Remove(_channelId);
                        }

                        if (removed)
                        {
                            try
                            {
                                //send disconnect signal
                                _connection.WriteFrame(ConnectionSignal.DisconnectChannel, _channelId, null, 0, 0);
                            }
                            catch
                            { }
                        }
                    }

                    _disposed = true;
                    Monitor.PulseAll(_lock);
                }

                base.Dispose(disposing);
            }

            #endregion

            #region stream support

            public override bool CanRead
            {
                get { return _connection._baseStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return _connection._baseStream.CanWrite; }
            }

            public override bool CanTimeout
            {
                get { return true; }
            }

            public override int ReadTimeout
            {
                get { return _readTimeout; }
                set { _readTimeout = value; }
            }

            public override int WriteTimeout
            {
                get { return _writeTimeout; }
                set { _writeTimeout = value; }
            }

            public override void Flush()
            {
                //do nothing
            }

            public override long Length
            {
                get { throw new NotSupportedException("ChannelStream stream does not support seeking."); }
            }

            public override long Position
            {
                get
                {
                    throw new NotSupportedException("ChannelStream stream does not support seeking.");
                }
                set
                {
                    throw new NotSupportedException("ChannelStream stream does not support seeking.");
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException("ChannelStream stream does not support seeking.");
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException("ChannelStream stream does not support seeking.");
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (count < 1)
                    throw new ArgumentOutOfRangeException("Count cannot be less than 1.");

                lock (_lock)
                {
                    if (_readBufferCount < 1)
                    {
                        if (_disposed)
                            return 0;

                        if (!Monitor.Wait(_lock, _readTimeout))
                            throw new IOException("Read timed out.");

                        if (_readBufferCount < 1)
                            return 0;
                    }

                    int bytesToCopy = count;

                    if (bytesToCopy > _readBufferCount)
                        bytesToCopy = _readBufferCount;

                    Buffer.BlockCopy(_readBuffer, _readBufferPosition, buffer, offset, bytesToCopy);

                    _readBufferPosition += bytesToCopy;
                    _readBufferCount -= bytesToCopy;

                    if (_readBufferCount < 1)
                        Monitor.Pulse(_lock);

                    return bytesToCopy;
                }
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (_disposed)
                    throw new ObjectDisposedException("ChannelStream");

                _connection.WriteFrame(ConnectionSignal.ChannelData, _channelId, buffer, offset, count);
            }

            #endregion

            #region private

            internal void FeedReadBuffer(Stream s, int timeout)
            {
                int count = Convert.ToInt32(s.Length - s.Position);
                int readCount = _readBuffer.Length;

                while (count > 0)
                {
                    lock (_lock)
                    {
                        if (_disposed)
                            throw new ObjectDisposedException("ChannelStream");

                        if (_readBufferCount > 0)
                        {
                            if (!Monitor.Wait(_lock, timeout))
                                throw new IOException("Channel FeedReadBuffer timed out.");

                            if (_readBufferCount > 0)
                                throw new IOException("Channel FeedReadBuffer failed. Buffer not empty.");
                        }

                        if (count < readCount)
                            readCount = count;

                        s.ReadBytes(_readBuffer, 0, readCount);
                        _readBufferPosition = 0;
                        _readBufferCount = readCount;
                        count -= readCount;

                        Monitor.Pulse(_lock);
                    }
                }
            }

            #endregion

            #region properties

            public BinaryNumber ChannelId
            { get { return _channelId; } }

            public Connection Connection
            { get { return _connection; } }

            #endregion
        }
    }
}
