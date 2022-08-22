using ExplorerOpenGL.Managers.Networking.EventArgs;
using ExplorerOpenGL.Model.Sprites;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking
{
    public class Client : IDisposable
    {
        public int myId = 0;
        public int serverTickRate = 0;

        public delegate void PacketHandler(Packet packet);
        public Dictionary<int, PacketHandler> packetHandlers;
        public Dictionary<int, PlayerData> PlayersData;
        public TCP tcp;
        public UDP udp;
        private GameManager manager; 
        public SocketAddress socketAddress { get; private set; }
        private ClientHandle clientHandle;
        private ClientSend clientSend;
        
        public delegate void PacketReceivedEventHandler(NetworkEventArgs e);
        public event PacketReceivedEventHandler OnPacketReceived;

        public delegate void PacketSentEventHandler(NetworkEventArgs e);
        public event PacketSentEventHandler OnPacketSent;

        public Client(GameManager manager)
        {
            this.manager = manager; 
            clientSend = new ClientSend(this); 
            clientHandle = new ClientHandle(this, clientSend); 
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
        
        public bool Disconnect()
        {
            try
            {
                udp.Close();
                tcp.Close(); 
                return true; 
            }
            catch(Exception e)
            {
                DebugManager.Instance.AddEvent(e.Message);
                return false;
            }
        }

        public void ConnectUdp()
        {
            udp.Connect(((IPEndPoint)tcp.socket.Client.LocalEndPoint).Port);
        }

        public void SendResponseWelcome(string name)
        {
            clientSend.SendResponseWelcome(name);
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
                { (int)ServerPackets.Welcome, clientHandle.OnWelcomeReceive },
                { (int)ServerPackets.UdpTest, clientHandle.OnUdpTestReceive },
                { (int)ServerPackets.UdpUpdatePlayers, clientHandle.OnUdpUpdatePlayers },
                { (int)ServerPackets.TcpAddPlayer, clientHandle.OnTcpAddPlayer },
                { (int)ServerPackets.TcpPlayersSync, clientHandle.OnTcpPlayersSync },
                { (int)ServerPackets.TcpMessage, clientHandle.OnTcpMessage },
                { (int)ServerPackets.UdpMessage, clientHandle.OnUdpMessage },
                { (int)ServerPackets.TcpChatMessage, clientHandle.OnChatMessage },
                { (int)ServerPackets.ChangeNameResult, clientHandle.OnChangeNameResult },
                { (int)ServerPackets.DisconnectPlayer, clientHandle.OnDisconnectPlayer },
                {(int)ServerPackets.UpdateGameObject, clientHandle.OnUpdateGameObject },
                {(int)ServerPackets.Sync, clientHandle.OnResponse },
                {(int)ServerPackets.ServerRequest, clientHandle.OnServerRequest },
            };
            Console.WriteLine("Initialized packets.");
        }

        public void PacketReceived(NetworkEventArgs e)
        {
            OnPacketReceived?.Invoke(e);
        }

        public void PacketSent(NetworkEventArgs e)
        {
            OnPacketSent?.Invoke(e);
        }

        public void CreateBullet(Player player)
        {
            clientSend.CreateBullet(player); 
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //get rid of managed resources
                tcp.Dispose();
                udp.Dispose();
                manager = null; 
                socketAddress = null;
                packetHandlers.Clear(); 
                packetHandlers = null;
                PlayersData.Clear();
                PlayersData = null;
                clientSend.Dispose();
                clientHandle.Dispose(); 

                //OnPacketSent/Received maybe have to be disposed
            }
            //get rid of unmanaged resources
            //nothing for now
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
