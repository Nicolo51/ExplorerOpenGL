using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Managers.Networking.EventArgs;
using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using LiteNetLib;
using LiteNetLib.Utils;
using Model.Network;
using NVorbis.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerTCP
{
    public class GameServer
    {
        public int port ;
        private LogMode _logMode = LogMode.classic; 
        public int maxPlayer = 20 ; 
        private Dictionary<int, ServerClient> clients;
        public delegate void PacketHandler(int fromClient, NetPacketReader packet, NetPeer peer);
        public Dictionary<int, PacketHandler> packetHandlers;
        public List<string> logs = new List<string>();
        public int lastLogCount = 0; 
        private Stopwatch sw = new Stopwatch();
        private long lastLogTime = 0;
        private int tickRate = 0;
        public string currentInput = string.Empty;
        private EventBasedNetListener listener;
        private NetManager server;
        private bool run;

        private GameManager gameManager;
        private DebugManager debugManager;
        private NetworkManager networkManager;
        private ServerSend  serverSend;
        private ServerHandle serverHandle;
        
        private byte[] mapData; 

        public int ClientID { get; set; }
        public GameServer(int port, NetworkManager networkManager)
        {
            serverSend = new ServerSend(this);
            serverHandle = new ServerHandle(this, serverSend);
            this.networkManager = networkManager;

            ClientID = -1; 
            run = true; 
            InitServerData();
            sw.Start(); 
            this.port = port;

            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            server.Start(25789);

            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;

            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;

            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
        }

        public void Update()
        {
            if (run)
                server.PollEvents();
        }

        public void OnUpdateGameState(NetDataReader r)
        {
            GameStateEventArgs gs = new GameStateEventArgs()
            {
                Type = r.GetInt(),
                ID = r.GetInt(),
                GsForced = r.GetBool(),
                Packet = r,
            };
            networkManager.OnGameStateUpdate(gs);
        }

        public void InitDependencies()
        {
            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance;
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var clientId = GetIdByPeer(peer);
            if (clientId == -1)
                return; 
            Log($"Client {GetPlayer(clientId).Name} timedout abruptly !");
            CloseConnection(clientId, peer);
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            ClientPackets handler = (ClientPackets)reader.GetInt();
            int id = GetIdByPeer(peer);
            packetHandlers[(int)handler](id, reader, peer); 
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Console.WriteLine("We got connection: {0}", peer);
            int id = AddPlayer().ID;
            if (ClientID == -1)
                ClientID = id;

            clients.Add(id, new ServerClient(id, peer));
            mapData = gameManager.GetMap();
            float packetNumber = mapData.Length / 1000; 
            serverSend.Welcome(id, gameManager.CurrentMap, packetNumber); 
        }

        public void SendMap(int toclient)
        {
            int splitSize = 1000; 
            List<byte[]> data = new List<byte[]>();

            for (int i = 0; i < mapData.Length; i += splitSize)
            {
                int size = Math.Min(splitSize, mapData.Length - i);
                byte[] chunk = new byte[size];
                Array.Copy(mapData, i, chunk, 0, size);
                data.Add(chunk);
            }

            foreach (var d in data)
            {
                serverSend.SendMap(d, toclient); 
            }
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            if (server.ConnectedPeersCount < maxPlayer)
                request.AcceptIfKey("pass");
            else
                request.Reject();
        }

        public int GetIdByPeer(NetPeer peer)
        {
            if(clients.Any(e => e.Value.clientPeer.Id == peer.Id))
                return clients.FirstOrDefault(e => e.Value.clientPeer.Id == peer.Id).Key;
            return -1;
        }
        public int GetId()
        {
            for(int i = 0; i < maxPlayer; i++)
            {
                if (!clients.Any(c => c.Key == i))
                    return i; 
            }
            return -1; 
        }

        public void ChangeLogMode(LogMode logMode, int tickRateLog)
        {
            Log($"Log mode changed to {logMode.ToString()}");
            _logMode = logMode;
            tickRate = tickRateLog;
            if(_logMode == LogMode.direct)
            {
                Console.Clear(); 
            }
            if(_logMode == LogMode.classic)
            {
                Console.Clear(); 
                foreach(var l in logs)
                    Console.WriteLine(l);
            }
        }

        public void StopServer()
        {
            serverSend = null; 
            serverHandle = null; 
            ClientID = -1;
            sw.Stop();
            server.Stop();
            server.DisconnectAll(); 
            listener.ConnectionRequestEvent -= Listener_ConnectionRequestEvent;
            listener.PeerConnectedEvent -= Listener_PeerConnectedEvent;
            listener.NetworkReceiveEvent -= Listener_NetworkReceiveEvent;
            listener.PeerDisconnectedEvent -= Listener_PeerDisconnectedEvent;
            listener = null;
            run = false; 
        }

        public void SendGameStateToClients(NetGameState gs)
        {
            gs.SendGameState(serverSend);
        }

        public void Log(string message)
        {

            if(gameManager != null && debugManager.IsDebuging)
                gameManager.Terminal.AddMessageToTerminal(message);

            //if (_logMode == LogMode.classic)
            //{
            //    logs.Add(message);
            //    ClearConsoleLine(); 
            //    Console.WriteLine(message);
            //}
            //if (_logMode == LogMode.direct)
            //{
            //    logs.Add(message);
            //}
        }

        public void DisplayLogDirect(Player[] players)
        {
            //if (_logMode == LogMode.classic)
            //{
            //    int cursorLeft = Console.CursorLeft;
            //    int cursorTop = logs.Count;
            //    ClearConsoleLine(); 
            //    Console.Write(currentInput);
            //    Console.SetCursorPosition(0, cursorTop);
            //}
            //else if (lastLogTime + tickRate < sw.ElapsedMilliseconds)
            //{
            //    lastLogTime = sw.ElapsedMilliseconds;
            //    int cursorLeft = Console.CursorLeft;
            //    Console.SetCursorPosition(0, 0);
            //    ClearConsoleLine(); 
            //    Console.WriteLine($"Online Players : {players.Length}");
            //    foreach (var p in players)
            //    {
            //        ClearConsoleLine(); 
            //        Console.WriteLine(p.ToString());
            //    }
            //    Console.WriteLine("**************************");
            //    if (lastLogCount < logs.Count && logs.Count > 0)
            //    {
            //        int i = Console.WindowHeight - players.Length - 3;
            //        for (int j = logs.Count - 1; j > logs.Count - i && j >= 0; j--)
            //        {
            //            ClearConsoleLine(); 
            //            Console.WriteLine("> " + logs[j]);
            //        }
            //    }
            //    Console.SetCursorPosition(0, Console.WindowHeight - 1);
            //    ClearConsoleLine(); 
            //    Console.Write(currentInput.Substring(0, Console.WindowWidth > currentInput.Length ? currentInput.Length : Console.WindowWidth));
            //}
        }

        public ServerClient[] GetClients()
        {
            return clients.Values.Where(c => c != null).ToArray(); 
        }

        public ServerClient[] GetAllClientsButServer()
        {
            return clients.Values.Where(c => c != null && c.id != ClientID).ToArray();
        }

        public ServerClient GetClient(int i)
        {
            if(clients.ContainsKey(i))
                return clients[i]; 
            return null; 
        }

        private void InitServerData()
        {
            clients = new Dictionary<int, ServerClient>(); 
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.WelcomeReceived, serverHandle.WelcomeReceived },
                {(int)ClientPackets.ChangeNameRequest, serverHandle.ChangeNameRequest },
                {(int)ClientPackets.TcpChatMessage, serverHandle.TcpCommandReceived },
                {(int)ClientPackets.UdpUpdatePlayer, serverHandle.UdpUpdatePlayer },
                {(int)ClientPackets.UdpMessageRecieved, serverHandle.UdpMessageReceived },
                {(int)ClientPackets.Disconnect, serverHandle.Disconnect },
                {(int)ClientPackets.UpdateGameState, serverHandle.UpdateGameState },
            };
        }

        public void CloseConnection(int fromClient, NetPeer peer)
        {
            if (ClientID == fromClient)
            {
                foreach(var sc in clients.Values)
                {
                    sc.clientPeer.Disconnect(); 
                }
            }
            else
            {
                ServerClient c = clients[fromClient];
                c.clientPeer.Disconnect();
                gameManager.RemoveSprite(fromClient);
                clients.Remove(fromClient);
            }
        }

        internal Player GetPlayer(int fromClient)
        {
            return (gameManager.GetSpriteById(fromClient) as Player);
        }

        public Player AddPlayer()
        {
            return gameManager.AddPlayer(); 
        }

        public Sprite[] GetPlayers()
        {
            return gameManager.GetPlayers();
        }
    }
    public enum LogMode : int
    {
        classic = 0, 
        direct = 1,
    }
}
