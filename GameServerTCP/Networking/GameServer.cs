using GameServerTCP.GameData;
using GameServerTCP.GameData.GameObjects;
using LiteNetLib;
using LiteNetLib.Utils;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerTCP
{
    public class GameServer
    {
        public static int port ;
        private static LogMode _logMode = LogMode.classic; 
        public static int maxPlayer = 20 ; 
        private static Dictionary<int, ServerClient> clients;
        public delegate void PacketHandler(int fromClient, NetPacketReader packet, NetPeer peer);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static List<string> logs = new List<string>();
        public static int lastLogCount = 0; 
        private static Stopwatch sw = new Stopwatch();
        private static long lastLogTime = 0;
        private static int tickRate = 0;
        public static string currentInput = string.Empty;
        private static EventBasedNetListener listener;
        private static NetManager server;
        private static bool run; 
        public static void Start(int port)
        {
            run = true; 
            InitServerData();
            sw.Start(); 
            GameServer.port = port;


            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            server.Start(25789);

            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;

            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;

            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;

            Log($"Server started on port {port}"); 
            while (run)
            {
                server.PollEvents();
                Thread.Sleep(15);
            }
            server.Stop();
            Log("Server Stopped"); 
        }

        private static void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var clientId = GameServer.GetIdByPeer(peer);
            if (clientId == -1)
                return; 
            Log($"Client {Game.GetPlayer(clientId).Name} timedout abruptly !");
            CloseConnection(clientId, peer);
        }

        private static void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            ClientPackets handler = (ClientPackets)reader.GetInt();
            int id = GetIdByPeer(peer);
            packetHandlers[(int)handler](id, reader, peer); 
        }

        private static void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Console.WriteLine("We got connection: {0}", peer);
            int id = GetId();
            if (id == -1)
                throw new Exception("Error : No id found");
            clients.Add(id, new ServerClient(id, peer)); 
            ServerSend.Welcome(id); 
        }

        private static void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            if (server.ConnectedPeersCount < maxPlayer)
                request.AcceptIfKey("pass");
            else
                request.Reject();
        }

        public static int GetIdByPeer(NetPeer peer)
        {
            if(clients.Any(e => e.Value.clientPeer.Id == peer.Id))
                return clients.FirstOrDefault(e => e.Value.clientPeer.Id == peer.Id).Key;
            return -1;
        }
        public static int GetId()
        {
            for(int i = 0; i < maxPlayer; i++)
            {
                if (!clients.Any(c => c.Key == i))
                    return i; 
            }
            return -1; 
        }

        public static void ChangeLogMode(LogMode logMode, int tickRateLog)
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

        public static void StopServer()
        {
            run = false; 
        }

        public static void Log(string message)
        {
            if (_logMode == LogMode.classic)
            {
                logs.Add(message);
                ClearConsoleLine(); 
                Console.WriteLine(message);
            }
            if (_logMode == LogMode.direct)
            {
                logs.Add(message);
            }
        }

        public static void ClearConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void DisplayLogDirect(Player[] players)
        {
            if (_logMode == LogMode.classic)
            {
                int cursorLeft = Console.CursorLeft;
                int cursorTop = logs.Count;
                ClearConsoleLine(); 
                Console.Write(currentInput);
                Console.SetCursorPosition(0, cursorTop);
            }
            else if (lastLogTime + tickRate < sw.ElapsedMilliseconds)
            {
                lastLogTime = sw.ElapsedMilliseconds;
                int cursorLeft = Console.CursorLeft;
                Console.SetCursorPosition(0, 0);
                ClearConsoleLine(); 
                Console.WriteLine($"Online Players : {players.Length}");
                foreach (var p in players)
                {
                    ClearConsoleLine(); 
                    Console.WriteLine(p.ToString());
                }
                Console.WriteLine("**************************");
                if (lastLogCount < logs.Count && logs.Count > 0)
                {
                    int i = Console.WindowHeight - players.Length - 3;
                    for (int j = logs.Count - 1; j > logs.Count - i && j >= 0; j--)
                    {
                        ClearConsoleLine(); 
                        Console.WriteLine("> " + logs[j]);
                    }
                }
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                ClearConsoleLine(); 
                Console.Write(currentInput.Substring(0, Console.WindowWidth > currentInput.Length ? currentInput.Length : Console.WindowWidth));
            }
        }

        public static ServerClient[] GetClients()
        {
            return clients.Values.Where(c => c != null).ToArray(); 
        }

        public static ServerClient GetClient(int i)
        {
            if(clients.ContainsKey(i))
                return clients[i]; 
            return null; 
        }

        private static void InitServerData()
        {
            clients = new Dictionary<int, ServerClient>(); 
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.WelcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.ChangeNameRequest, ServerHandle.ChangeNameRequest },
                {(int)ClientPackets.TcpChatMessage, ServerHandle.TcpCommandReceived },
                {(int)ClientPackets.UdpUpdatePlayer, ServerHandle.UdpUpdatePlayer },
                {(int)ClientPackets.UdpMessageRecieved, ServerHandle.UdpMessageReceived },
                {(int)ClientPackets.CreateBullet, ServerHandle.CreateBullet }, 
                {(int)ClientPackets.Disconnect, ServerHandle.Disconnect },
                {(int)ClientPackets.UpdateGameState, ServerHandle.UpdateGameState },
            };
        }

        public static void CloseConnection(int fromClient, NetPeer peer)
        {
            ServerClient c = clients[fromClient];
            Game.RemovePlayer(fromClient);
            clients.Remove(fromClient);

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                c.clientPeer.Disconnect();
            });
        }
    }
    public enum LogMode : int
    {
        classic = 0, 
        direct = 1,
    }
}
