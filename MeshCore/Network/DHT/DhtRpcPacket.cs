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
using System.Net;
using TechnitiumLibrary.IO;
using TechnitiumLibrary.Net;

namespace MeshCore.Network.DHT
{
    enum DhtRpcType : byte
    {
        PING = 0,
        FIND_NODE = 1,
        FIND_PEERS = 2,
        ANNOUNCE_PEER = 3
    }

    class DhtRpcPacket
    {
        #region variables

        readonly EndPoint _sourceNodeEP;
        readonly DhtRpcType _type;

        readonly BinaryNumber _networkId;
        readonly NodeContact[] _contacts;
        readonly EndPoint[] _peers;

        #endregion

        #region constructor

        public DhtRpcPacket(BinaryReader bR)
        {
            int version = bR.ReadByte();

            switch (version)
            {
                case 1:
                    _sourceNodeEP = EndPointExtension.Parse(bR);
                    _type = (DhtRpcType)bR.ReadByte();

                    switch (_type)
                    {
                        case DhtRpcType.PING:
                            break;

                        case DhtRpcType.FIND_NODE:
                            _networkId = new BinaryNumber(bR.BaseStream);

                            _contacts = new NodeContact[bR.ReadByte()];
                            for (int i = 0; i < _contacts.Length; i++)
                                _contacts[i] = new NodeContact(bR);

                            break;

                        case DhtRpcType.FIND_PEERS:
                            _networkId = new BinaryNumber(bR.BaseStream);

                            _contacts = new NodeContact[bR.ReadByte()];
                            for (int i = 0; i < _contacts.Length; i++)
                                _contacts[i] = new NodeContact(bR);

                            _peers = new EndPoint[bR.ReadByte()];
                            for (int i = 0; i < _peers.Length; i++)
                                _peers[i] = EndPointExtension.Parse(bR);

                            break;

                        case DhtRpcType.ANNOUNCE_PEER:
                            _networkId = new BinaryNumber(bR.BaseStream);

                            _peers = new EndPoint[bR.ReadByte()];

                            for (int i = 0; i < _peers.Length; i++)
                                _peers[i] = EndPointExtension.Parse(bR);

                            break;

                        default:
                            throw new IOException("Invalid DHT-RPC type.");
                    }

                    break;

                default:
                    throw new InvalidDataException("DHT-RPC packet version not supported: " + version);
            }
        }

        private DhtRpcPacket(EndPoint sourceNodeEP, DhtRpcType type, BinaryNumber networkId, NodeContact[] contacts, EndPoint[] peers)
        {
            _sourceNodeEP = sourceNodeEP;
            _type = type;

            _networkId = networkId;
            _contacts = contacts;
            _peers = peers;
        }

        #endregion

        #region static create

        public static DhtRpcPacket CreatePingPacket(NodeContact sourceNode)
        {
            return new DhtRpcPacket(sourceNode.NodeEP, DhtRpcType.PING, null, null, null);
        }

        public static DhtRpcPacket CreateFindNodePacketQuery(NodeContact sourceNode, BinaryNumber networkId)
        {
            return new DhtRpcPacket(sourceNode.NodeEP, DhtRpcType.FIND_NODE, networkId, null, null);
        }

        public static DhtRpcPacket CreateFindNodePacketResponse(NodeContact sourceNode, BinaryNumber networkId, NodeContact[] contacts)
        {
            return new DhtRpcPacket(sourceNode.NodeEP, DhtRpcType.FIND_NODE, networkId, contacts, null);
        }

        public static DhtRpcPacket CreateFindPeersPacketQuery(NodeContact sourceNode, BinaryNumber networkId)
        {
            return new DhtRpcPacket(sourceNode.NodeEP, DhtRpcType.FIND_PEERS, networkId, null, null);
        }

        public static DhtRpcPacket CreateFindPeersPacketResponse(NodeContact sourceNode, BinaryNumber networkId, NodeContact[] contacts, EndPoint[] peers)
        {
            return new DhtRpcPacket(sourceNode.NodeEP, DhtRpcType.FIND_PEERS, networkId, contacts, peers);
        }

        public static DhtRpcPacket CreateAnnouncePeerPacketQuery(NodeContact sourceNode, BinaryNumber networkId, EndPoint serviceEP)
        {
            return new DhtRpcPacket(sourceNode.NodeEP, DhtRpcType.ANNOUNCE_PEER, networkId, null, new EndPoint[] { serviceEP });
        }

        public static DhtRpcPacket CreateAnnouncePeerPacketResponse(NodeContact sourceNode, BinaryNumber networkId, EndPoint[] peers)
        {
            return new DhtRpcPacket(sourceNode.NodeEP, DhtRpcType.ANNOUNCE_PEER, networkId, null, peers);
        }

        #endregion

        #region public

        public void WriteTo(BinaryWriter bW)
        {
            bW.Write((byte)1); //version
            _sourceNodeEP.WriteTo(bW); //source node EP
            bW.Write((byte)_type); //type

            switch (_type)
            {
                case DhtRpcType.FIND_NODE:
                    _networkId.WriteTo(bW.BaseStream);

                    if (_contacts == null)
                    {
                        bW.Write((byte)0);
                    }
                    else
                    {
                        bW.Write(Convert.ToByte(_contacts.Length));
                        foreach (NodeContact contact in _contacts)
                            contact.WriteTo(bW);
                    }

                    break;

                case DhtRpcType.FIND_PEERS:
                    _networkId.WriteTo(bW.BaseStream);

                    if (_contacts == null)
                    {
                        bW.Write((byte)0);
                    }
                    else
                    {
                        bW.Write(Convert.ToByte(_contacts.Length));
                        foreach (NodeContact contact in _contacts)
                            contact.WriteTo(bW);
                    }

                    if (_peers == null)
                    {
                        bW.Write((byte)0);
                    }
                    else
                    {
                        bW.Write(Convert.ToByte(_peers.Length));
                        foreach (EndPoint peer in _peers)
                            peer.WriteTo(bW);
                    }
                    break;

                case DhtRpcType.ANNOUNCE_PEER:
                    _networkId.WriteTo(bW.BaseStream);

                    if (_peers == null)
                    {
                        bW.Write((byte)0);
                    }
                    else
                    {
                        bW.Write(Convert.ToByte(_peers.Length));
                        foreach (EndPoint peer in _peers)
                            peer.WriteTo(bW);
                    }
                    break;
            }
        }

        #endregion

        #region properties

        public EndPoint SourceNodeEP
        { get { return _sourceNodeEP; } }

        public DhtRpcType Type
        { get { return _type; } }

        public BinaryNumber NetworkId
        { get { return _networkId; } }

        public NodeContact[] Contacts
        { get { return _contacts; } }

        public EndPoint[] Peers
        { get { return _peers; } }

        #endregion
    }
}
