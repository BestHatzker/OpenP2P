﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public interface INetworkMessage { }

    public class NetworkMessage : INetworkMessage
    {
        public class Header
        {
            //encoded into packet
            public bool isReliable = false;
            public bool isStream = false;
            public bool isRedirect = false;
            public SendType sendType = 0;
            public ChannelType channelType = ChannelType.Invalid;
            public ushort sequence = 0;
            public ushort id = 0;

            //packet status
            public uint ackkey = 0;
            public long sentTime = 0;
            public int retryCount = 0;
            
            //packet endpoints
            public EndPoint source = null;
            public EndPoint destination = null;
            public NetworkPeer peer = null;
        }

        public Header header = new Header();

        public virtual void WriteMessage(NetworkPacket packet) { }
        public virtual void WriteResponse(NetworkPacket packet) { }
        public virtual void ReadMessage(NetworkPacket packet) { }
        public virtual void ReadResponse(NetworkPacket packet) { }

        public virtual void StreamMessage(NetworkPacket packet) {}

    }
}
