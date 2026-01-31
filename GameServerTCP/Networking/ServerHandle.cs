using GameServerTCP.GameData;
using GameServerTCP.GameData.GameObjects;
using LiteNetLib;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;

namespace GameServerTCP
{
    public class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, NetPacketReader packet, NetPeer peer) 
        {
            int clientIdCheck = packet.GetInt();
            string name = packet.GetString();
            string msg = packet.GetString();
            //GameServer.Log(msg);
            GameServer.Log(peer.Address.ToString() + " successfully connected and is now connected as " + name + " with id : " + fromClient); 
            if (clientIdCheck != fromClient)
            {
                    GameServer.Log("L'id is corrupted " + clientIdCheck);
                return; 
            }
            Game.AddPlayer(clientIdCheck, new Player(clientIdCheck, name));
            ServerSend.AddPlayer(fromClient);
            ServerSend.PlayersSync(fromClient);
        }

        public static void UdpMessageReceived(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            int id = packet.GetInt(); 
            if(id == fromClient)
                GameServer.Log(fromClient + "-" + Game.GetPlayer(fromClient).Name + " : " + packet.GetString()); 
        }

        public static void TcpCommandReceived(int idClient, NetPacketReader packet, NetPeer peer)
        {
            int clientIdCheck = packet.GetInt();
            string msg = packet.GetString();
            string[] cmd; 

            if (string.IsNullOrWhiteSpace(msg = msg.Trim()))
                return;
            if(msg[0] == '/')
            {
                cmd = msg.Split(' ');
                
                if(/*HasPriviliege(idClient)*/ true)
                {
                    switch (cmd[0].ToLower())
                    {
                        case "/tp": 
                            GameServer.Log("Tp command issued");
                            break;
                        case "/w":
                            GameServer.Log("Whisper command issued");
                            break;
                        default:
                            GameServer.Log("Unrecognized command");
                            break;
                    }
                }
                return; 
            }

            if (clientIdCheck != idClient)
            {
                GameServer.Log("L'id is corrupted " + clientIdCheck);
                return; 
            }
            GameServer.Log($"[{Game.GetPlayer(idClient).Name }, {idClient}]: { msg }");
            ServerSend.TcpSpreadChatMessageToAll(Game.GetPlayer(idClient), msg); 
        }

        public static void UdpUpdatePlayer(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            Vector2 position = new Vector2(packet.GetFloat(), packet.GetFloat());
            int health = packet.GetInt();
            string animationName = packet.GetString();
            int effect = packet.GetInt();

            string msg = $"Posistion = {position.ToString()}";
            Game.UpdatePlayer(fromClient, position, health, animationName, effect);
            //ServerSend.UdpUpdatePlayers(fromClient);
            //GameServer.Log($"Received packet via UDP from ID { fromClient }. Contains message: {msg}");
            //Game.PrintDebug(); 
        }

        public static void ChangeNameRequest(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            if(!checkIdIntegrity(fromClient, packet))
                ServerSend.TcpChangeNameResult(fromClient, 403, string.Empty);

            string name = packet.GetString();
            Game.GetPlayer(fromClient).Name = name;
            ServerSend.TcpChangeNameResult(fromClient, 200, name);
        }

        public static void CreateBullet(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            if (!checkIdIntegrity(fromClient, packet))
                return;
            var position = new Vector2(packet.GetFloat(), packet.GetFloat());
            var direction = packet.GetFloat();
            var velocity = packet.GetFloat();
            var owner = Game.GetPlayer(packet.GetInt());
            Bullet bullet = new Bullet(position)
            {
                Direction = direction,
                Velocity = velocity,
                Owner = owner, 
            };
            Game.AddGameObject(bullet);
            GameServer.Log("Bullet created, id : " + bullet.ID);
        }

        public static void Disconnect(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            Thread.Sleep(2000); 
            if (!checkIdIntegrity(fromClient, packet))
                return;
            GameServer.CloseConnection(fromClient, peer);
        }

        public static bool checkIdIntegrity(int fromClient, NetPacketReader packet)
        {
            int CheckID = packet.GetInt();
            return fromClient == CheckID; 
        }

        public static void UpdateGameState(int fromClient, NetPacketReader packet, NetPeer peer)
        {

        }
    }
}
