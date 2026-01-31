using ExplorerOpenGL2.Managers.Networking.EventArgs;
using ExplorerOpenGL2.Model.Sprites;
using LiteNetLib;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking
{
    public class Client : IDisposable
    {
        public int ID { get; set; }
        public int serverTickRate = 0;

        public delegate void PacketHandler(NetPacketReader packet);
        public Dictionary<int, PacketHandler> packetHandlers;
        public Dictionary<int, Player> PlayersData { get; set; }
        private GameManager manager; 
        public SocketAddress socketAddress { get; private set; }
        private ClientHandle clientHandle;
        private ClientSend clientSend;

        EventBasedNetListener listener;
        NetManager netManager;
        NetPeer peer; 

        public delegate void PacketReceivedEventHandler(NetworkEventArgs e);
        public event PacketReceivedEventHandler OnPacketReceived;

        public delegate void PacketSentEventHandler(NetworkEventArgs e);
        public event PacketSentEventHandler OnPacketSent;

        public Client(GameManager manager)
        {
            this.manager = manager; 
            clientHandle = new ClientHandle(this, clientSend); 
        }

        public void ConnectToServer(SocketAddress socketAddress)
        {
            this.socketAddress = socketAddress;
            PlayersData = new Dictionary<int, Player>();
            InitClientData();
            listener = new EventBasedNetListener();
            netManager = new NetManager(listener);

            netManager.Start();
            netManager.Connect(socketAddress.IP, socketAddress.Port, "pass");

            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;

        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            DebugManager.Instance.AddEventToTerminal("Peer disconected");    
        }

        public void PollEvents()
        {
            netManager.PollEvents(); 
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            int handleId = reader.GetInt();
            packetHandlers[handleId].Invoke(reader);
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            this.peer = peer;
            clientSend = new ClientSend(peer);
        }

        public bool Disconnect()
        {
            try
            {
                peer.Disconnect(); 
                return true; 
            }
            catch(Exception e)
            {
                DebugManager.Instance.AddEventToTerminal("Disconect failed : " + e.Message);
                return false;
            }
        }

        public void SendResponseWelcome(string name, int myId)
        {
            clientSend.SendResponseWelcome(name, myId);
        }

        public void SendMessage(object obj, ClientPackets idHandler)
        {
            clientSend.SendMessage(obj, (int)idHandler);
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
                { (int)ServerPackets.Sync, clientHandle.OnResponse },
                { (int)ServerPackets.ChangeHealth, clientHandle.OnChangeHealth },
                { (int)ServerPackets.Teleport, clientHandle.OnTeleport },
                { (int)ServerPackets.MoveObject, clientHandle.OnMoveObject },
                { (int)ServerPackets.RemoveObject, clientHandle.OnRemoveObject },
                { (int)ServerPackets.CreateObject, clientHandle.OnCreateObject},
                { (int)ServerPackets.UpdateGameState, clientHandle.OnUpdateGameState },
                { (int)ServerPackets.Map, clientHandle.GetMaps },
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
