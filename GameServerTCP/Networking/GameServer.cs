using SharedClasses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServerTCP
{
    public class GameServer
    {
        public static int port ;
        public static int maxPlayer = 20 ; 
        private static TcpListener tcpListener;
        private static UdpClient udpListener; 
        private static Dictionary<int, Client> clients;
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        
        public static void Start(int port)
        {
            InitServerData(); 
            GameServer.port = port; 
            tcpListener = new TcpListener(IPAddress.Any, port); 
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);

            udpListener = new UdpClient(port);
            udpListener.Client.IOControl(-1744830452, new byte[] { 0, 0, 0, 0 }, null); 
            udpListener.BeginReceive(OnUDPReceived, null);

            Console.WriteLine("Server started on port {0}", port); 
        }

        private static void OnUDPReceived(IAsyncResult ar)
        {
            byte[] _data = null;
            IPEndPoint _clientEndPoint = null; 
            try
            {
                _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                _data = udpListener.EndReceive(ar, ref _clientEndPoint);
                udpListener.BeginReceive(OnUDPReceived, null);
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}");
                //udpListener = new UdpClient(port);
                //udpListener.BeginReceive(OnUDPReceived, null);
                Console.WriteLine($"The UDP service has been reset");

            }

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                if (GetClient(_clientId).udp.endPoint == null)
                {
                    GetClient(_clientId).udp.Connect(_clientEndPoint);
                    Console.WriteLine("A client has connect to udp stream");
                    return;
                }

                if (GetClient(_clientId).udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    GetClient(_clientId).udp.HandleData(_packet);
                }
            }
        }

        private static void OnClientConnect(IAsyncResult ar)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(ar);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
            Console.WriteLine("Incoming connection from : " + client.Client.RemoteEndPoint + " ...");

            for (int i = 1; i <= maxPlayer; i++)
            {
                lock (clients)
                {
                    if (clients[i].tcp.socket == null)
                    {
                            clients[i].tcp.Connect(client);
                            return;
                    }
                }
            }
        }
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        public static Client GetClient(int i)
        {
            lock (clients)
            {
                if(clients.ContainsKey(i))
                    return clients[i]; 
                Console.WriteLine("it does not contain the key");
                return null; 
            }
        }

        public static Client[] GetClients()
        {
            Client[] clientsArray;
            lock (clients)
            {
                clientsArray = new Client[clients.Count];
                clients.Values.CopyTo(clientsArray, 0); 
            }
            return clientsArray; 
        }

        public static void setClient(int i, Client client)
        {

        }

        private static void InitServerData()
        {
            clients = new Dictionary<int, Client>(); 
            for (int i = 0; i <= maxPlayer; i++)
            {
                clients.Add(i, new Client(i));
            }
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.WelcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.ChangeNameRequest, ServerHandle.ChangeNameRequest },
                {(int)ClientPackets.UdpTestRecieved, ServerHandle.UDPTestReceived },
                {(int)ClientPackets.TcpChatMessage, ServerHandle.TcpCommandReceived },
                {(int)ClientPackets.UdpUpdatePlayer, ServerHandle.UdpUpdatePlayer },
                {(int)ClientPackets.UdpMessageRecieved, ServerHandle.UdpMessageReceived },
                {(int)ClientPackets.CreateBullet, ServerHandle.CreateBullet }, 
                {(int)ClientPackets.Disconnect, ServerHandle.Disconnect },
            };
        }
    }
}
