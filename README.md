# Mesh
Mesh is a secure, anonymous, peer-to-peer (p2p), open source instant messenger designed to provide end-to-end encryption. Primary aim of developing this instant messenger is to provide privacy which is achieved using cryptography and anonymity using Tor network. It can be used over Internet and private LAN networks (without Internet) for instant messaging and file transfer with support for private chats and group chats. 

Mesh is based on Bit Chat and is its successor. Mesh takes core ideas from its predecessor and removes a few. Notably, Mesh does not require centralized user registration and provides anonymous profile support using Tor hidden service. It also removes use of BitTorrent trackers for finding peers which was causing issues with Bit Chat since some ISPs blocking BitTorrent traffic would also block Bit Chat traffic. Instead, Mesh relies only on DHT for all purposes.

Mesh allows creating both pure p2p and anonymous profiles with support for running multiple profiles concurrently. Both p2p and anonymous profiles are interoperable such that a p2p profile user can connect with anonymous profile using Tor.

With Mesh, there is no meta data generated. Since, there is no user registration, we don't know who uses Mesh or how many people use it. In p2p mode, the connections use IPv4 or IPv6 connectivity directly to connect with peers without any server in between. With anonymous mode, all connectivity occurs over Tor network and uses Tor hidden service to accept inbound connections.

Mesh is still under development and the website is not yet deployed.

Website: https://mesh.im
