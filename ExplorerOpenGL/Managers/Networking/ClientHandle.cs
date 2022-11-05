using ExplorerOpenGL.Managers.Networking.EventArgs;
using ExplorerOpenGL.Managers.Networking.NetworkObject;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking
{
    public class ClientHandle
    {
        private Client client; 
        private ClientSend clientSend;
        private DebugManager debugManager;

        public ClientHandle(Client client, ClientSend clientSend)
        {
            this.client = client;
            this.debugManager = DebugManager.Instance; 
            this.clientSend = clientSend;
        }

        public void OnWelcomeReceive(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();
            int tickRate  = packet.ReadInt();
            client.myId = id;
            client.serverTickRate = tickRate;
            PlayerData playerData = new PlayerData(id);
            client.PlayersData.Add(id, playerData); 

            NetworkEventArgs e = new WelcomeEventArgs() { Message = msg + $".", PacketType = ServerPackets.Welcome,   ID = id, PlayerData = playerData, TickRate = tickRate};
            client.PacketReceived(e); 
        }

        public void OnUdpTestReceive(Packet packet)
        {
            string msg = packet.ReadString();

            clientSend.SendUDPTest();

            NetworkEventArgs e = new NetworkEventArgs() { Message = msg + $".", PacketType = ServerPackets.UdpTest };
            client.PacketReceived(e);
        }

        public void OnTcpMessage(Packet packet)
        {
            string msg = packet.ReadString();

            NetworkEventArgs e = new NetworkEventArgs() { Message = msg + $".", PacketType = ServerPackets.TcpMessage, Packet = packet };
            client.PacketReceived(e);
        } 
        public void OnUdpMessage(Packet _packet)
        {
            string msg = _packet.ReadString();

            debugManager.AddEvent($"Received packet via UDP.Contains message: {msg}");
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
                PacketType = ServerPackets.TcpPlayersSync,
                PlayerData = playerData,
                PlayerSyncedCount = count,
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
                PacketType = ServerPackets.DisconnectPlayer,
                ID = idPlayer, 
                Name = playerName, 
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
                PacketType = ServerPackets.TcpChatMessage,
                SenderColor = Color.White,
                TextColor = Color.White, 
                Text = msg, 
                Packet = packet, 
                
            };
            client.PacketReceived(e); 
        }

        public void OnChangeNameResult(Packet packet)
        {
            int result = packet.ReadInt();
            string name = packet.ReadString();
            int IDClient = packet.ReadInt(); 
            if (result == 200 && !string.IsNullOrWhiteSpace(name))
            {
                PlayerChangeNameEventArgs e = new PlayerChangeNameEventArgs()
                {
                    Message = $"Successfuly change username to {name}.",
                    PacketType = ServerPackets.ChangeNameResult,
                    Packet = packet,
                    IDPlayer = IDClient, 
                    Name = name,
                };
                client.PacketReceived(e);
            }
            else
            {
                RequestResponseEventArgs e = new RequestResponseEventArgs()
                {
                    Message = $"Failed change username to {name}.",
                    PacketType = ServerPackets.ChangeNameResult,
                    Packet = packet,
                    RequestStatus = RequestStatus.Failed,
                    Request = "ChangeName",
                    Response = "403",
                    Arguments = new string[] { name },
                };
                client.PacketReceived(e); 
            }
        }

        public void OnResponse(Packet packet)
        {
            ServerPackets packetType = (ServerPackets)packet.ReadInt();
            //pourquoi pas faire un switch pour handle les différent type de reponses
            bool success = packet.ReadBool();
            if (success)
                client.ConnectUdp(); 

            //a revoir; 
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
                    PacketType = ServerPackets.TcpAddPlayer,
                    Packet = packet,
                    Name = name, 
                    ID = idPlayer, 
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
                        ServerPosition = new Vector2(packet.ReadFloat(), packet.ReadFloat()),
                        Health = packet.ReadInt(),
                        CurrentAnimationName = packet.ReadString(),
                        Effects = (SpriteEffects)packet.ReadInt(),
                });
                }
                else
                {
                    //Debug.WriteLine("Unsync from the server, might lag a little while re syncing ");
                }
            }
            PlayerUpdateEventArgs e = new PlayerUpdateEventArgs()
            {
                PacketType = ServerPackets.UdpUpdatePlayers,
                Packet = packet,
                PlayerData = playerData.ToArray(),
            };
            client.PacketReceived(e); 
        }

        public void OnUpdateGameObject(Packet packet)
        {
            List<NetworkGameObject> gameObjectData = new List<NetworkGameObject>();
            while (packet.ReadBool())
            {
                //string typeGameObject = packet.ReadString();
                //int id = packet.ReadInt();
                //bool isRemove = packet.ReadBool(); 
                //Vector2 position = new Vector2(packet.ReadFloat(), packet.ReadFloat()); 
                //Vector2 direction = new Vector2(packet.ReadFloat(), packet.ReadFloat());
                //float velocity = packet.ReadFloat();
                //int idPlayer = packet.ReadInt();
                NetworkGameObject bullet = new NetworkBullet();
                bullet.ReadPacket(packet); 
                gameObjectData.Add(bullet); 
            }
            GameObjectsUpdateEventArgs e = new GameObjectsUpdateEventArgs()
            {
                PacketType = ServerPackets.UpdateGameObject,
                Packet = packet,
                networkGameObjects = gameObjectData.ToArray(), 
            };
            client.PacketReceived(e); 
        }

        public void OnChangeHealth(Packet packet)
        {
            int newHealth = packet.ReadInt();
            NetworkEventArgs e = new UpdateSelfEventArgs()
            {
                Packet = packet,
                PacketType = ServerPackets.ChangeHealth,
                Health = newHealth,
            };
            client.PacketReceived(e);
        }

        public void OnTeleport(Packet packet)
        {
            Vector2 pos = new Vector2(packet.ReadFloat(), packet.ReadFloat());
            NetworkEventArgs e = new UpdateSelfEventArgs()
            {
                Packet = packet,
                PacketType = ServerPackets.Teleport,
                Position = pos,
            };
            client.PacketReceived(e); 
        }

        public void OnMoveObject(Packet packet)
        {
            int id = packet.ReadInt();
            Vector2 position = new Vector2(packet.ReadFloat(), packet.ReadFloat());
        }

        public void OnCreateObject(Packet packet)
        {

        }

        public void OnRemoveObject(Packet packet)
        {
            int id = packet.ReadInt();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //get rid of managed resources
                client = null;
                clientSend = null;
                debugManager = null;
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