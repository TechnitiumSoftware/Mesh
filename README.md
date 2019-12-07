<p align="center">
	<a href="https://mesh.im/">
		<img src="https://mesh.im/img/logo.png" alt="Technitium Mesh" /><br />
		<b>Technitium Mesh</b>
	</a><br />
	<br />
	<b>Get a secure, anonymous, peer-to-peer instant messenger</b><br />
	<b>One messenger for Internet and LAN chat with end-to-end encryption</b>
</p>
<p>
<img src="https://mesh.im/img/mesh-screenshot.png" alt="Technitium Mesh" />
</p>

Technitium Mesh is a secure, anonymous, peer-to-peer (p2p), open source instant messenger designed to provide end-to-end encryption. Primary aim of developing this instant messenger is to provide privacy which is achieved using cryptography and anonymity using Tor network. It can be used over Internet and private LAN networks (without Internet) for instant messaging and file transfer with support for private chats and group chats.

Mesh is based on [Bit Chat](https://github.com/TechnitiumSoftware/BitChatClient) and is its successor. Mesh takes core ideas from its predecessor and removes a few. Notably, Mesh does not require centralized user registration and provides anonymous profile support using Tor hidden service. It also removes use of BitTorrent trackers for finding peers which was causing issues with Bit Chat since some ISPs blocking BitTorrent traffic would also block Bit Chat traffic. Instead, Mesh relies only on [Distributed Hash Tables (DHT)](https://en.wikipedia.org/wiki/Distributed_hash_table) for all purposes.

Mesh allows creating both pure p2p and anonymous profiles with support for running multiple profiles concurrently. Both p2p and anonymous profiles are interoperable such that a p2p profile user can connect with an anonymous profile user via Tor Network.

With Mesh, there is no meta data generated. User identifier is designed in such a way that it can be changed anytime to hide identity. Since, there is no user registration, we don't know who uses Mesh or how many people use it. In p2p mode, the connections use IPv4 or IPv6 connectivity directly to connect with peers without any server in between. With anonymous mode, all connectivity occurs over Tor network and uses Tor hidden service to accept inbound connections.

# Peer-to-Peer
- Serverless, peer-to-peer architecture that uses [Distributed Hash Tables (DHT)](https://en.wikipedia.org/wiki/Distributed_hash_table).
- No meta data is stored since even we don't know to whom you are chatting with.
- Works as LAN chat just as it works on the Internet.
- Works in private LAN networks not connected to Internet.
- Anonymous profiles use [Tor Network](https://www.torproject.org/) to hide your identity.

# Secure
- Uses RSA 2048 bit keys to generate profiles.
- Provides end-to-end encryption with [Perfect Forward Secrecy (PFS)](https://en.wikipedia.org/wiki/Forward_secrecy) using DHE-2048 or ECDHE-256.
- Protocol is secured with AES 256-bit encryption with [Authenticated Encryption](https://en.wikipedia.org/wiki/Authenticated_encryption).
- Changeable user identifier to hide identity.
- Open source implementation allows you to inspect the code.

# Installation
- **Windows (Setup)**: [Download setup installer](https://download.technitium.com/mesh/MeshSetup.zip)
- **Windows (Standalone)**: [Download portable zip](https://download.technitium.com/mesh/MeshPortable.zip)

# Frequently Asked Questions (FAQ)
Read this [FAQ](https://mesh.im/faq.html) page which should answer most of your queries.

# Support
For support, send an email to support@technitium.com. For any issues, feedback, or feature request, create an issue on [GitHub](https://github.com/TechnitiumSoftware/Mesh/issues).

# Become A Patron
Make contribution to Technitium by becoming a Patron and help making new software, updates, and features possible.

[Become a Patron now!](https://www.patreon.com/technitium)
