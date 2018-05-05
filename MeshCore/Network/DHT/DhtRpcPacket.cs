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
using System.IO;
using TechnitiumLibrary.IO;

namespace MeshCore.Network.DHT
{
    enum DhtRpcType : byte
    {
        PING = 0,
        FIND_NODE = 1,
        FIND_PEERS = 2,
        ANNOUNCE_PEER = 3
    }

    class DhtRpcPacket : IWriteStream
    {
        #region variables

        readonly ushort _sourceNodePort;
        readonly DhtRpcType _type;

        readonly BinaryNumber _networkId;
        readonly NodeContact[] _contacts;
        readonly PeerEndPoint[] _peers;

        #endregion

        #region constructor

        public DhtRpcPacket(Stream s)
        {
            int version = s.ReadByte();

            switch (version)
            {
                case -1:
                    throw new EndOfStreamException();

                case 1:
                    byte[] buffer = new byte[20];

                    OffsetStream.StreamRead(s, buffer, 0, 2);
                    _sourceNodePort = BitConverter.ToUInt16(buffer, 0);

                    _type = (DhtRpcType)s.ReadByte();

                    switch (_type)
                    {
                        case DhtRpcType.PING:
                            break;

                        case DhtRpcType.FIND_NODE:
                            {
                                OffsetStream.StreamRead(s, buffer, 0, 20);
                                _networkId = BinaryNumber.Clone(buffer, 0, 20);

                                int count = s.ReadByte();
                                _contacts = new NodeContact[count];

                                for (int i = 0; i < count; i++)
                                    _contacts[i] = new NodeContact(s);
                            }
                            break;

                        case DhtRpcType.FIND_PEERS:
                            {
                                OffsetStream.StreamRead(s, buffer, 0, 20);
                                _networkId = BinaryNumber.Clone(buffer, 0, 20);

                                int count = s.ReadByte();
                                _contacts = new NodeContact[count];

                                for (int i = 0; i < count; i++)
                                    _contacts[i] = new NodeContact(s);

                                count = s.ReadByte();
                                _peers = new PeerEndPoint[count];

                                for (int i = 0; i < count; i++)
                                    _peers[i] = new PeerEndPoint(s);
                            }
                            break;

                        case DhtRpcType.ANNOUNCE_PEER:
                            {
                                OffsetStream.StreamRead(s, buffer, 0, 20);
                                _networkId = BinaryNumber.Clone(buffer, 0, 20);

                                int count = s.ReadByte();
                                _peers = new PeerEndPoint[count];

                                for (int i = 0; i < count; i++)
                                    _peers[i] = new PeerEndPoint(s);
                            }
                            break;

                        default:
                            throw new IOException("Invalid DHT-RPC type.");
                    }

                    break;

                default:
                    throw new IOException("DHT-RPC packet version not supported: " + version);
            }
        }

        private DhtRpcPacket(ushort sourceNodePort, DhtRpcType type, BinaryNumber networkId, NodeContact[] contacts, PeerEndPoint[] peers)
        {
            _sourceNodePort = sourceNodePort;
            _type = type;

            _networkId = networkId;
            _contacts = contacts;
            _peers = peers;
        }

        #endregion

        #region static create

        public static DhtRpcPacket CreatePingPacket(NodeContact sourceNode)
        {
            return new DhtRpcPacket(Convert.ToUInt16(sourceNode.NodeEP.Port), DhtRpcType.PING, null, null, null);
        }

        public static DhtRpcPacket CreateFindNodePacketQuery(NodeContact sourceNode, BinaryNumber networkId)
        {
            return new DhtRpcPacket(Convert.ToUInt16(sourceNode.NodeEP.Port), DhtRpcType.FIND_NODE, networkId, null, null);
        }

        public static DhtRpcPacket CreateFindNodePacketResponse(NodeContact sourceNode, BinaryNumber networkId, NodeContact[] contacts)
        {
            return new DhtRpcPacket(Convert.ToUInt16(sourceNode.NodeEP.Port), DhtRpcType.FIND_NODE, networkId, contacts, null);
        }

        public static DhtRpcPacket CreateFindPeersPacketQuery(NodeContact sourceNode, BinaryNumber networkId)
        {
            return new DhtRpcPacket(Convert.ToUInt16(sourceNode.NodeEP.Port), DhtRpcType.FIND_PEERS, networkId, null, null);
        }

        public static DhtRpcPacket CreateFindPeersPacketResponse(NodeContact sourceNode, BinaryNumber networkId, NodeContact[] contacts, PeerEndPoint[] peers)
        {
            return new DhtRpcPacket(Convert.ToUInt16(sourceNode.NodeEP.Port), DhtRpcType.FIND_PEERS, networkId, contacts, peers);
        }

        public static DhtRpcPacket CreateAnnouncePeerPacketQuery(NodeContact sourceNode, BinaryNumber networkId, PeerEndPoint peer)
        {
            return new DhtRpcPacket(Convert.ToUInt16(sourceNode.NodeEP.Port), DhtRpcType.ANNOUNCE_PEER, networkId, null, new PeerEndPoint[] { peer });
        }

        public static DhtRpcPacket CreateAnnouncePeerPacketResponse(NodeContact sourceNode, BinaryNumber networkId, PeerEndPoint[] peers)
        {
            return new DhtRpcPacket(Convert.ToUInt16(sourceNode.NodeEP.Port), DhtRpcType.ANNOUNCE_PEER, networkId, null, peers);
        }

        #endregion

        #region public

        public void WriteTo(Stream s)
        {
            s.WriteByte(1); //version
            s.Write(BitConverter.GetBytes(_sourceNodePort), 0, 2); //source node port
            s.WriteByte((byte)_type); //type

            switch (_type)
            {
                case DhtRpcType.FIND_NODE:
                    s.Write(_networkId.Value, 0, 20);

                    if (_contacts == null)
                    {
                        s.WriteByte(0);
                    }
                    else
                    {
                        s.WriteByte(Convert.ToByte(_contacts.Length));

                        foreach (NodeContact contact in _contacts)
                            contact.WriteTo(s);
                    }

                    break;

                case DhtRpcType.FIND_PEERS:
                    s.Write(_networkId.Value, 0, 20);

                    if (_contacts == null)
                    {
                        s.WriteByte(0);
                    }
                    else
                    {
                        s.WriteByte(Convert.ToByte(_contacts.Length));

                        foreach (NodeContact contact in _contacts)
                            contact.WriteTo(s);
                    }

                    if (_peers == null)
                    {
                        s.WriteByte(0);
                    }
                    else
                    {
                        s.WriteByte(Convert.ToByte(_peers.Length));

                        foreach (PeerEndPoint peer in _peers)
                            peer.WriteTo(s);
                    }
                    break;

                case DhtRpcType.ANNOUNCE_PEER:
                    s.Write(_networkId.Value, 0, 20);

                    if (_peers == null)
                    {
                        s.WriteByte(0);
                    }
                    else
                    {
                        s.WriteByte(Convert.ToByte(_peers.Length));

                        foreach (PeerEndPoint peer in _peers)
                            peer.WriteTo(s);
                    }
                    break;
            }
        }

        #endregion

        #region properties

        public ushort SourceNodePort
        { get { return _sourceNodePort; } }

        public DhtRpcType Type
        { get { return _type; } }

        public BinaryNumber NetworkID
        { get { return _networkId; } }

        public NodeContact[] Contacts
        { get { return _contacts; } }

        public PeerEndPoint[] Peers
        { get { return _peers; } }

        #endregion
    }
}
