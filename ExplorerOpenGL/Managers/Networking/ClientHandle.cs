using ExplorerOpenGL2.Managers.Networking.EventArgs;
using ExplorerOpenGL2.Model.Sprites;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking
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

        public void OnWelcomeReceive(NetPacketReader packet)
        {
            string msg = packet.GetString();
            string map = packet.GetString();
            float mapSize = packet.GetFloat();
            int id = packet.GetInt();
            client.ID = id;

            //PlayerData playerData = new PlayerData(id);
            //client.PlayersData.Add(id, playerData); 

            NetworkEventArgs e = new WelcomeEventArgs() { Message = msg + $".", PacketType = ServerPackets.Welcome,   ID = id, PlayerData = null, MapName = map, MapSize = mapSize};
            client.PacketReceived(e); 
        }

        public void OnUdpTestReceive(NetPacketReader packet)
        {
            string msg = packet.GetString();

            clientSend.SendUDPTest();

            NetworkEventArgs e = new NetworkEventArgs() { Message = msg + $".", PacketType = ServerPackets.UdpTest };
            client.PacketReceived(e);
        }

        public void OnTcpMessage(NetPacketReader packet)
        {
            string msg = packet.GetString();

            NetworkEventArgs e = new NetworkEventArgs() { Message = msg + $".", PacketType = ServerPackets.TcpMessage, Packet = packet };
            client.PacketReceived(e);
        } 
        public void OnUdpMessage(NetPacketReader _packet)
        {
            string msg = _packet.GetString();

            debugManager.AddEventToTerminal($"Received packet via UDP.Contains message: {msg}");
        }

        public void OnTcpPlayersSync(NetPacketReader packet)
        {
            List<PlayerData> playerData = new List<PlayerData>(); 
            while (packet.GetBool())
            {
                int idPlayer = packet.GetInt();
                string name = packet.GetString();
                if (idPlayer != client.ID)
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

        public void OnDisconnectPlayer(NetPacketReader packet)
        {
            int idPlayer = packet.GetInt();
            string playerName = packet.GetString();
            PlayerDisconnectionEventArgs e = new PlayerDisconnectionEventArgs() {
                Message = $"{playerName} left the game.",
                PacketType = ServerPackets.DisconnectPlayer,
                ID = idPlayer, 
                Name = playerName, 
                Packet = packet,
            };
            client.PacketReceived(e); 
        }

        public void OnChatMessage(NetPacketReader packet)
        {
            string sender = packet.GetString();
            string msg = packet.GetString();
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

        public void OnChangeNameResult(NetPacketReader packet)
        {
            int result = packet.GetInt();
            string name = packet.GetString();
            int IDClient = packet.GetInt(); 
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

        public void OnResponse(NetPacketReader packet)
        {
            //Aknoledge bool true or false if server say yes to a request
            ServerPackets packetType = (ServerPackets)packet.GetInt();
            bool success = packet.GetBool();
        }

        public void OnTcpAddPlayer(NetPacketReader packet)
        {
            int idPlayer = packet.GetInt();
            string name = packet.GetString();
            if (!client.PlayersData.ContainsKey(idPlayer) && idPlayer != client.ID)
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

        public void OnUdpUpdatePlayers(NetPacketReader packet)
        {
            List<PlayerData> playerData = new List<PlayerData>(); 
            while (packet.GetBool())
            {
                int idPlayer = packet.GetInt();
                if (client.PlayersData.ContainsKey(idPlayer))
                {
                    playerData.Add(new PlayerData(idPlayer)
                    {
                        ServerPosition = new Vector2(packet.GetFloat(), packet.GetFloat()),
                        Health = packet.GetInt(),
                        CurrentAnimationName = packet.GetString(),
                        Effect = (SpriteEffects)packet.GetInt(),
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

        public void OnChangeHealth(NetPacketReader packet)
        {
            int newHealth = packet.GetInt();
            NetworkEventArgs e = new UpdateSelfEventArgs()
            {
                Packet = packet,
                PacketType = ServerPackets.ChangeHealth,
                Health = newHealth,
            };
            client.PacketReceived(e);
        }

        public void OnTeleport(NetPacketReader packet)
        {
            Vector2 pos = new Vector2(packet.GetFloat(), packet.GetFloat());
            NetworkEventArgs e = new UpdateSelfEventArgs()
            {
                Packet = packet,
                PacketType = ServerPackets.Teleport,
                Position = pos,
            };
            client.PacketReceived(e); 
        }

        public void OnMoveObject(NetPacketReader packet)
        {
            int id = packet.GetInt();
            Vector2 position = new Vector2(packet.GetFloat(), packet.GetFloat());
        }

        public void OnCreateObject(NetPacketReader packet)
        {

        }

        public void OnRemoveObject(NetPacketReader packet)
        {
            int id = packet.GetInt();
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

        public void OnUpdateGameState(NetPacketReader packet)
        {
            NetworkEventArgs e = new GameStateEventArgs()
            {
                Packet = packet,
                PacketType = ServerPackets.UpdateGameState,
                Type = packet.GetInt(),
                ID = packet.GetInt(),
                GsForced = packet.GetBool(),
            };
            client.PacketReceived(e);
        }

        public void GetMaps(NetPacketReader packet)
        {
            NetworkEventArgs e = new MapEventArgs()
            {
                Packet = packet,
                PacketType = ServerPackets.Map,
                data = new byte[1000]
            }; 
            packet.GetBytes((e as MapEventArgs).data, 1000); 

            client.PacketReceived(e);
        }
    }
}