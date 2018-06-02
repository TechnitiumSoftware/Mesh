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

/*  Connection Frame
*   0                8                                 168                               184
*  +----------------+---------------//----------------+----------------+----------------+---------------//----------------+
*  | signal (1 byte)|     channel id  (20 bytes)      |     data length (uint16)        |              data               |
*  +----------------+---------------//----------------+----------------+----------------+---------------//----------------+
*  
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

namespace MeshCore.Network.Connections
{
    enum ConnectionSignal : byte
    {
        PingRequest = 1,
        PingResponse = 2,

        ConnectChannelNetwork = 4,
        ChannelData = 5,
        DisconnectChannel = 6,
        ConnectChannelTunnel = 7,
        ConnectChannelVirtualConnection = 8,

        TcpRelayRegisterHostedNetwork = 9,
        TcpRelayUnregisterHostedNetwork = 10,
        TcpRelayReceivedNetworkPeers = 11
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

        readonly Thread _readThread;

        readonly List<Joint> _tunnelJointList = new List<Joint>();

        int _channelWriteTimeout = 30000;

        const int TCP_RELAY_CLIENT_MODE_TIMER_INTERVAL = 30000;
        Timer _tcpRelayClientModeTimer;

        bool _hasRegisteredHostedNetwork = false;

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

        #region IDisposable support

        bool _disposed = false;

        public void Dispose()
        {
            lock (this)
            {
                if (!_disposed)
                {
                    //dispose tcp relay mode timer
                    if (_tcpRelayClientModeTimer != null)
                        _tcpRelayClientModeTimer.Dispose();

                    //dispose all channels
                    List<ChannelStream> streamList = new List<ChannelStream>();

                    lock (_channels)
                    {
                        foreach (KeyValuePair<BinaryNumber, ChannelStream> channel in _channels)
                            streamList.Add(channel.Value);
                    }

                    foreach (ChannelStream stream in streamList)
                    {
                        try
                        {
                            stream.Dispose();
                        }
                        catch
                        { }
                    }

                    //dispose base stream
                    Monitor.Enter(_baseStream);
                    try
                    {
                        _baseStream.Dispose();
                    }
                    catch
                    { }
                    finally
                    {
                        Monitor.Exit(_baseStream);
                    }

                    _disposed = true;

                    _connectionManager.ConnectionDisposed(this);

                    if (_hasRegisteredHostedNetwork)
                        _connectionManager.TcpRelayServerUnregisterAllHostedNetworks(this);
                }
            }
        }

        #endregion

        #region private

        private void WriteFrame(ConnectionSignal signal, BinaryNumber channelId, byte[] buffer, int offset, int count)
        {
            int frameCount = CONNECTION_FRAME_BUFFER_SIZE;

            do
            {
                if (count < frameCount)
                    frameCount = count;

                lock (_baseStream)
                {
                    _baseStream.WriteByte((byte)signal); //write frame signal
                    channelId.WriteTo(_baseStream); //write channel id
                    _baseStream.Write(BitConverter.GetBytes(Convert.ToUInt16(frameCount)), 0, 2); //write data length

                    if (frameCount > 0)
                        _baseStream.Write(buffer, offset, frameCount); //write data

                    //flush base stream
                    _baseStream.Flush();
                }

                offset += frameCount;
                count -= frameCount;
            }
            while (count > 0);
        }

        private void ReadFrameAsync()
        {
            try
            {
                //frame parameters
                int signal;
                ushort dataLength;
                byte[] dataLengthBuffer = new byte[2];

                while (true)
                {
                    #region read frame from base stream

                    //read frame signal
                    signal = _baseStream.ReadByte();
                    if (signal == -1)
                        return; //End of stream

                    //read channel id
                    BinaryNumber channelId = new BinaryNumber(_baseStream);

                    //read data length
                    _baseStream.ReadBytes(dataLengthBuffer, 0, 2);
                    dataLength = BitConverter.ToUInt16(dataLengthBuffer, 0);

                    //read data stream
                    OffsetStream dataStream = null;

                    if (dataLength > 0)
                        dataStream = new OffsetStream(_baseStream, 0, dataLength, true, false);

                    #endregion

                    switch ((ConnectionSignal)signal)
                    {
                        case ConnectionSignal.PingRequest:
                            WriteFrame(ConnectionSignal.PingResponse, channelId, null, 0, 0);
                            break;

                        case ConnectionSignal.PingResponse:
                            //do nothing!
                            break;

                        case ConnectionSignal.ConnectChannelNetwork:
                            #region ConnectChannelNetwork

                            lock (_channels)
                            {
                                if (_channels.ContainsKey(channelId))
                                {
                                    WriteFrame(ConnectionSignal.DisconnectChannel, channelId, null, 0, 0);
                                }
                                else
                                {
                                    ChannelStream channel = new ChannelStream(this, channelId);
                                    _channels.Add(channel.ChannelId, channel);

                                    ThreadPool.QueueUserWorkItem(delegate (object state)
                                    {
                                        try
                                        {
                                            //done async since the call is blocking and will block the current read thread which can cause DOS
                                            _connectionManager.Node.MeshNetworkRequest(this, channel.ChannelId, channel);
                                        }
                                        catch
                                        {
                                            channel.Dispose();
                                        }
                                    });
                                }
                            }

                            //check if tcp relay is hosted for the channel. reply back tcp relay peers list if available
                            Connection[] connections = _connectionManager.GetTcpRelayServerHostedNetworkConnections(channelId);

                            if (connections.Length > 0)
                            {
                                using (MemoryStream mS = new MemoryStream(128))
                                {
                                    BinaryWriter bW = new BinaryWriter(mS);

                                    bW.Write(Convert.ToByte(connections.Length));

                                    foreach (Connection connection in connections)
                                        (connection.RemotePeerEP as IPEndPoint).WriteTo(bW);

                                    byte[] data = mS.ToArray();

                                    WriteFrame(ConnectionSignal.TcpRelayReceivedNetworkPeers, channelId, data, 0, data.Length);
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
                                    _channels.Remove(channelId);
                                }

                                channel.SetDisconnected();
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
                                        remoteChannel1 = new ChannelStream(this, channelId);
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
                                            Joint joint = new Joint(remoteChannel1, remoteChannel2);

                                            joint.Disposed += delegate (object sender, EventArgs e)
                                            {
                                                lock (_tunnelJointList)
                                                {
                                                    _tunnelJointList.Remove(sender as Joint);
                                                }
                                            };

                                            lock (_tunnelJointList)
                                            {
                                                _tunnelJointList.Add(joint);
                                            }

                                            joint.Start();
                                        }
                                        catch
                                        {
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
                                        ChannelStream channel = new ChannelStream(this, channelId);
                                        _channels.Add(channel.ChannelId, channel);

                                        //pass channel as connection async
                                        Thread t = new Thread(delegate (object state)
                                        {
                                            try
                                            {
                                                _connectionManager.AcceptConnectionInitiateProtocol(channel, ConvertChannelIdToEp(channel.ChannelId));
                                            }
                                            catch
                                            {
                                                channel.Dispose();
                                            }
                                        });

                                        t.IsBackground = true;
                                        t.Start();
                                    }
                                }
                            }

                            #endregion
                            break;

                        case ConnectionSignal.TcpRelayRegisterHostedNetwork:
                            #region TcpRelayRegisterHostedNetwork

                            _connectionManager.TcpRelayServerRegisterHostedNetwork(this, channelId);

                            _hasRegisteredHostedNetwork = true;

                            #endregion
                            break;

                        case ConnectionSignal.TcpRelayUnregisterHostedNetwork:
                            #region TcpRelayUnregisterHostedNetwork

                            _connectionManager.TcpRelayServerUnregisterHostedNetwork(this, channelId);

                            #endregion
                            break;

                        case ConnectionSignal.TcpRelayReceivedNetworkPeers:
                            #region TcpRelayNetworkPeers
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

                    if (dataStream != null)
                    {
                        //discard any unread data
                        if (dataStream.Length > dataStream.Position)
                            dataStream.CopyTo(Stream.Null, 1024);
                    }
                }
            }
            catch
            { }
            finally
            {
                Dispose();
            }
        }

        private BinaryNumber ConvertEpToChannelId(EndPoint ep)
        {
            using (MemoryStream mS = new MemoryStream())
            {
                ep.WriteTo(new BinaryWriter(mS));

                return new BinaryNumber(mS.ToArray());
            }
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
                    throw new ArgumentException("Channel already exists.");

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
                    throw new ArgumentException("Channel already exists.");

                channel = new ChannelStream(this, channelId);
                _channels.Add(channel.ChannelId, channel);
            }

            //send connect signal
            WriteFrame(ConnectionSignal.ConnectChannelNetwork, channelId, null, 0, 0);

            return channel;
        }

        public Stream MakeTunnelConnection(EndPoint remotePeerEP)
        {
            BinaryNumber channelId = ConvertEpToChannelId(remotePeerEP);
            ChannelStream channel;

            lock (_channels)
            {
                if (_channels.ContainsKey(channelId))
                    throw new ArgumentException("Channel already exists.");

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
                    WriteFrame(ConnectionSignal.PingRequest, BinaryNumber.GenerateRandomNumber256(), null, 0, 0);
                }, null, 1000, TCP_RELAY_CLIENT_MODE_TIMER_INTERVAL);
            }
        }

        public void TcpRelayRegisterHostedNetwork(BinaryNumber channelId)
        {
            WriteFrame(ConnectionSignal.TcpRelayRegisterHostedNetwork, channelId, null, 0, 0);
        }

        public void TcpRelayUnregisterHostedNetwork(BinaryNumber channelId)
        {
            WriteFrame(ConnectionSignal.TcpRelayUnregisterHostedNetwork, channelId, null, 0, 0);
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

        #endregion

        private class ChannelStream : Stream
        {
            #region variables

            const int CHANNEL_READ_TIMEOUT = 60000; //channel read timeout; application must NOOP
            const int CHANNEL_WRITE_TIMEOUT = 30000; //dummy timeout for write since base channel write timeout will be used

            readonly Connection _connection;
            readonly BinaryNumber _channelId;

            readonly byte[] _readBuffer = new byte[CONNECTION_FRAME_BUFFER_SIZE];
            int _readBufferOffset;
            int _readBufferCount;

            int _readTimeout = CHANNEL_READ_TIMEOUT;
            int _writeTimeout = CHANNEL_WRITE_TIMEOUT;

            bool _disconnected = false;

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
                lock (this)
                {
                    if (!_disposed)
                    {
                        if (!_disconnected)
                        {
                            lock (_connection._channels)
                            {
                                _connection._channels.Remove(_channelId);
                            }

                            try
                            {
                                //send disconnect signal
                                _connection.WriteFrame(ConnectionSignal.DisconnectChannel, _channelId, null, 0, 0);
                            }
                            catch
                            { }
                        }

                        _disposed = true;
                        Monitor.PulseAll(this);
                    }
                }
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
                    throw new ArgumentOutOfRangeException("Count must be atleast 1 byte.");

                lock (this)
                {
                    if (_readBufferCount < 1)
                    {
                        if (_disposed)
                            return 0;

                        if (!Monitor.Wait(this, _readTimeout))
                            throw new IOException("Read timed out.");

                        if (_readBufferCount < 1)
                            return 0;
                    }

                    int bytesToCopy = count;

                    if (bytesToCopy > _readBufferCount)
                        bytesToCopy = _readBufferCount;

                    Buffer.BlockCopy(_readBuffer, _readBufferOffset, buffer, offset, bytesToCopy);

                    _readBufferOffset += bytesToCopy;
                    _readBufferCount -= bytesToCopy;

                    if (_readBufferCount < 1)
                        Monitor.Pulse(this);

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
                    lock (this)
                    {
                        if (_disposed)
                            throw new ObjectDisposedException("ChannelStream");

                        if (_readBufferCount > 0)
                        {
                            if (!Monitor.Wait(this, timeout))
                                throw new IOException("Channel FeedReadBuffer timed out.");

                            if (_readBufferCount > 0)
                                throw new IOException("Channel FeedReadBuffer failed. Buffer not empty.");
                        }

                        if (count < readCount)
                            readCount = count;

                        s.ReadBytes(_readBuffer, 0, readCount);
                        _readBufferOffset = 0;
                        _readBufferCount = readCount;
                        count -= readCount;

                        Monitor.Pulse(this);
                    }
                }
            }

            internal void SetDisconnected()
            {
                _disconnected = true;
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
