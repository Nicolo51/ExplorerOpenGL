using GameServerTCP;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public class Client
    {
        public int dataBufferSize = 4096;
        public string ip = "127.0.0.1";
        public int port = 25789; 
        public static int myId = 0;
        public static string name = "???"; 

        public delegate void PacketHandler(Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static TCPData tcp;
        public static UDPData udp; 

        public static void Start()
        {
            tcp = new TCPData();
            udp = new UDPData(); 
        }
        public static void ConnectToServer()
        {
            tcp.Connect();
            InitClientData(); 
        }
        public static void SendMessage(string msg, int idHandler)
        {
            ClientSend.SendMessage(msg, idHandler);
        }
        public static void InitClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.Welcome, ClientHandle.OnWelcomeResponse },
                { (int)ServerPackets.UdpTest, ClientHandle.OnUdpTestResponse },
                { (int)ServerPackets.UdpUpdatePlayers, ClientHandle.OnUdpUpdatePlayers },
                { (int)ServerPackets.TcpAddPlayer, ClientHandle.OnTcpAddPlayer },
                { (int)ServerPackets.TcpPlayersSync, ClientHandle.OnTcpPlayersSync },
                { (int)ServerPackets.TcpMessage, ClientHandle.OnTcpMessage },
                { (int)ServerPackets.UdpMessage, ClientHandle.OnUdpMessage },
                { (int)ServerPackets.TcpChatMessage, ClientHandle.OnChatMessage },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
    
}
