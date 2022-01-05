using ExplorerOpenGL.Controlers.Networking.EventArgs;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class ClientHandle
    {
        private Client client; 
        private ClientSend clientSend;
        private Controler controler;

        public ClientHandle(Client client, ClientSend clientSend, Controler controler)
        {
            this.client = client;
            this.controler = controler; 
            this.clientSend = clientSend; 
        }

        public void OnWelcomeReceive(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();
            int tickRate  = packet.ReadInt();
            client.myId = id;
            client.serverTickRate = tickRate;
            client.PlayersData.Add(id, new PlayerData(id)); 
            clientSend.SendResponseWelcome();
            client.udp.Connect(((IPEndPoint)client.tcp.socket.Client.LocalEndPoint).Port);

            NetworkEventArgs e = new NetworkEventArgs() { Message = msg + $".", MessageType = MessageType.OnWelcomeReceive, Protocol = Protocol.TCP, RequestType = RequestType.Receive };
            client.PacketReceived(e); 
        }

        public void OnUdpTestReceive(Packet packet)
        {
            string msg = packet.ReadString();

            clientSend.SendUDPTest();

            NetworkEventArgs e = new NetworkEventArgs() { Message = msg + $".", MessageType = MessageType.OnUdpTestReceive, Protocol = Protocol.UDP, RequestType = RequestType.Receive };
            client.PacketReceived(e);
        }

        public void OnTcpMessage(Packet packet)
        {
            string msg = packet.ReadString();

            NetworkEventArgs e = new NetworkEventArgs() { Message = msg + $".", MessageType = MessageType.OnTcpMessage, Protocol = Protocol.TCP, RequestType = RequestType.Receive, Packet = packet };
            client.PacketReceived(e);
        } 
        public void OnUdpMessage(Packet _packet)
        {
            string msg = _packet.ReadString();

            client.controler.DebugManager.AddEvent($"Received packet via UDP.Contains message: {msg}");
        }

        public void OnTcpPlayersSync(Packet packet)
        {
            List<PlayerData> playerData = new List<PlayerData>(); 
            while (packet.ReadBool())
            {
                int idPlayer = packet.ReadInt();
                string name = packet.ReadString();
                if (idPlayer != client.myId)
                {
                    playerData.Add(new PlayerData(idPlayer, name));
                }
            }
            int count = playerData.Count;
            PlayerSyncEventArgs e = new PlayerSyncEventArgs()
            {
                Message = $"{count} were synced to the game.",
                MessageType = MessageType.OnTcpPlayersSync,
                PlayerData = playerData,
                PlayerSyncedCount = count,
                Protocol = Protocol.TCP,
                RequestType = RequestType.Receive,
                Packet = packet, 
            };
            client.PacketReceived(e); 
        }

        public void OnDisconnectPlayer(Packet packet)
        {
            int idPlayer = packet.ReadInt();
            string playerName = packet.ReadString();
            PlayerDisconnectionEventArgs e = new PlayerDisconnectionEventArgs() {
                Message = $"{playerName} left the game.",
                MessageType = MessageType.OnDisconnection,
                ID = idPlayer, 
                Name = playerName, 
                Protocol = Protocol.TCP,
                RequestType = RequestType.Receive,
                Packet = packet,
            };
            client.PacketReceived(e); 
        }

        public void OnChatMessage(Packet packet)
        {
            string sender = packet.ReadString();
            string msg = packet.ReadString();
            ChatMessageEventArgs e = new ChatMessageEventArgs()
            {
                Sender = sender,
                Message = $"A message has been receive from {sender}.",
                Time = DateTime.Now,
                MessageType = MessageType.OnChatMessage,
                Protocol = Protocol.TCP,
                RequestType = RequestType.Receive,
                SenderColor = Color.Black,
                TextColor = Color.Black, 
                Text = msg, 
                Packet = packet, 
                
            };
            client.PacketReceived(e); 
        }

        public void OnChangeNameResult(Packet packet)
        {
            int result = packet.ReadInt();
            string name = packet.ReadString();
            if (result == 200 && !string.IsNullOrWhiteSpace(name))
            {
                RequestResponseEventArgs e = new RequestResponseEventArgs()
                {
                    Message = $"Successfuly change username to {name}.",
                    MessageType = MessageType.OnChangeNameResult, 
                    Packet = packet, 
                    Protocol = Protocol.TCP, 
                    RequestStatus = RequestStatus.Success, 
                    RequestType = RequestType.Receive, 
                    Request = "ChangeName", 
                    Response = "200", 
                    Arguments = new string[] {name}, 
                };
                client.PacketReceived(e);
            }
            else
            {
                RequestResponseEventArgs e = new RequestResponseEventArgs()
                {
                    Message = $"Failed change username to {name}.",
                    MessageType = MessageType.OnChangeNameResult,
                    Packet = packet,
                    Protocol = Protocol.TCP,
                    RequestStatus = RequestStatus.Failed,
                    RequestType = RequestType.Receive,
                    Request = "ChangeName",
                    Response = "403",
                    Arguments = new string[] { name },
                };
                client.PacketReceived(e); 
            }
        }

        public void OnTcpAddPlayer(Packet packet)
        {
            int idPlayer = packet.ReadInt();
            string name = packet.ReadString();
            if (!client.PlayersData.ContainsKey(idPlayer) && idPlayer != client.myId)
            {
                PlayerConnectEventArgs e = new PlayerConnectEventArgs()
                {
                    Message = $"{name} joined the game.",
                    MessageType = MessageType.OnTcpAddPlayer,
                    Packet = packet,
                    Name = name, 
                    ID = idPlayer, 
                    Protocol = Protocol.TCP,
                    RequestType = RequestType.Receive,
                };
                client.PacketReceived(e);
            }
        }

        public void OnUdpUpdatePlayers(Packet packet)
        {
            List<PlayerData> playerData = new List<PlayerData>(); 
            while (packet.ReadBool())
            {
                int idPlayer = packet.ReadInt();
                if (client.PlayersData.ContainsKey(idPlayer))
                {
                    playerData.Add(new PlayerData(idPlayer)
                    {
                        ServerPosition = new Vector2(packet.ReadFloat(),
                        packet.ReadFloat()),
                        LookAtRadian = packet.ReadFloat(),
                        FeetRadian = packet.ReadFloat()
                    }); 
                    //Debug.WriteLine(Client.PlayersData[idPlayer].ToString());
                }
                else
                {
                    //Debug.WriteLine("Un sync from the server, might lag a little while re syncing ");
                }
            }
            PlayerUpdateEventArgs e = new PlayerUpdateEventArgs()
            {
                MessageType = MessageType.OnUdpUpdatePlayers,
                Packet = packet,
                Protocol = Protocol.UDP,
                PlayerData = playerData.ToArray(),
                RequestType = RequestType.Receive,
            };
            client.PacketReceived(e); 
        }
    }
}