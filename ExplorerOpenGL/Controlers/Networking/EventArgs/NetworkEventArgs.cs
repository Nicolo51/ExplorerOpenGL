﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking.EventArgs
{
    public class NetworkEventArgs
    {
        public RequestType RequestType { get; set; }
        public Protocol Protocol { get; set; }
        public MessageType MessageType { get; set; }
        public string Message { get; set; }
        public Packet Packet { get; set; }
    }

    public enum RequestType : int
    {
        Send = 0, 
        Receive =1, 
    }

    public enum MessageType : int 
    {
        SendResponseWelcome = 0,
        SendUDPTest = 1,

        UdpUpdatePlayer = 2,
        SendTcpChatMessage = 3,
        RequestChangeName = 4,

        OnWelcomeReceive = 5,
        OnUdpTestReceive = 6,

        OnTcpMessage = 7,
        OnTcpPlayersSync = 8,
        OnChatMessage = 9,
        OnChangeNameResult = 10,
        OnTcpAddPlayer = 11,
        OnUdpUpdatePlayers = 12, 
    }

    public enum Protocol : int
    {
        TCP = 0, 
        UDP = 1, 
    }
}
