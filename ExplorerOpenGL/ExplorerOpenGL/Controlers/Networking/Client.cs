using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class Client
    {
        public static int myId = 0;
        public static int serverTickRate = 0;

        public delegate void PacketHandler(Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static Dictionary<int, PlayerData> PlayersData;
        public static TCP tcp;
        public static UDP udp;
        public static Controler controler { get; set; } 
        public static SocketAddress socketAddress { get; private set; } 

        public static void Start(SocketAddress SocketAddress)
        {
            socketAddress = SocketAddress;
            PlayersData = new Dictionary<int, PlayerData>(); 
            tcp = new TCP(socketAddress);
            udp = new UDP(socketAddress);
        }
        public static void ConnectToServer()
        {
            tcp.Connect();
            InitClientData();
        }
        public static void SendMessage(object obj, int idHandler)
        {
            ClientSend.SendMessage(obj, idHandler);
        }
        public static void InitClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.welcome, ClientHandle.WelcomeResponse },
                { (int)ServerPackets.udpTest, ClientHandle.UdpTestResponse },
                { (int)ServerPackets.UdpUpdatePlayers, ClientHandle.UdpUpdatePlayers },
                { (int)ServerPackets.TcpAddPlayer, ClientHandle.TcpAddPlayer },
                { (int)ServerPackets.TcpPlayersSync, ClientHandle.TcpPlayersSync },
                { (int)ServerPackets.TcpMessage, ClientHandle.TcpMessageResponse },
                { (int)ServerPackets.UdpMessage, ClientHandle.UdpMessageResponse },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
