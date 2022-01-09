using ExplorerOpenGL.Controlers.Networking.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class Client
    {
        public int myId = 0;
        public int serverTickRate = 0;

        public delegate void PacketHandler(Packet packet);
        public Dictionary<int, PacketHandler> packetHandlers;
        public Dictionary<int, PlayerData> PlayersData;
        public TCP tcp;
        public UDP udp;
        public Controler controler; 
        public SocketAddress socketAddress { get; private set; }
        private ClientHandle clientHandle;
        private ClientSend clientSend;
        
        public delegate void PacketReceivedEventHandler(NetworkEventArgs e);
        public event PacketReceivedEventHandler OnPacketReceived;

        public delegate void PacketSentEventHandler(NetworkEventArgs e);
        public event PacketSentEventHandler OnPacketSent;

        public Client(Controler controler)
        {
            this.controler = controler; 
            clientSend = new ClientSend(this); 
            clientHandle = new ClientHandle(this, clientSend, controler); 
        }

        public void Start(SocketAddress SocketAddress)
        {
            socketAddress = SocketAddress;
            PlayersData = new Dictionary<int, PlayerData>(); 
            tcp = new TCP(socketAddress, this);
            udp = new UDP(socketAddress, this);
        }
        public void ConnectToServer()
        {
            tcp.Connect();
            InitClientData();
        }
        public void SendMessage(object obj, int idHandler)
        {
            clientSend.SendMessage(obj, idHandler);
        }

        public void RequestNameChange(string name)
        {
            clientSend.RequestChangeName(name); 
        }
        
        public void InitClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.welcome, clientHandle.OnWelcomeReceive },
                { (int)ServerPackets.udpTest, clientHandle.OnUdpTestReceive },
                { (int)ServerPackets.UdpUpdatePlayers, clientHandle.OnUdpUpdatePlayers },
                { (int)ServerPackets.TcpAddPlayer, clientHandle.OnTcpAddPlayer },
                { (int)ServerPackets.TcpPlayersSync, clientHandle.OnTcpPlayersSync },
                { (int)ServerPackets.TcpMessage, clientHandle.OnTcpMessage },
                { (int)ServerPackets.UdpMessage, clientHandle.OnUdpMessage },
                { (int)ServerPackets.TcpChatMessage, clientHandle.OnChatMessage },
                { (int)ServerPackets.ChangeNameResult, clientHandle.OnChangeNameResult }, 
                { (int)ServerPackets.DisconnectPlayer, clientHandle.OnDisconnectPlayer },
            };
            Console.WriteLine("Initialized packets.");
        }

        public void PacketReceived(NetworkEventArgs e)
        {
            OnPacketReceived?.Invoke(e);
        }
    }
}
