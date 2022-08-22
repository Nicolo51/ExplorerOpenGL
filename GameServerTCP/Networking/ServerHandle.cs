using GameServerTCP.GameData;
using GameServerTCP.GameData.GameObjects;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerTCP
{
    public class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet) 
        {
            int clientIdCheck = packet.ReadInt();
            string name = packet.ReadString();
            string msg = packet.ReadString();
            //Console.WriteLine(msg);
            Console.WriteLine(GameServer.GetClient(fromClient).tcp.socket.Client.RemoteEndPoint + " successfully connected and is now connected as " + name + " with id : " + fromClient);
            if (clientIdCheck != fromClient)
            {
                Console.WriteLine("L'id is corrupted " + clientIdCheck);
                return; 
            }
            Game.AddPlayer(clientIdCheck, new Player(clientIdCheck, name));
            ServerSend.RequestAcknowledgeme(fromClient, ClientPackets.WelcomeReceived, true); 
        }

        public static void UdpMessageReceived(int fromClient, Packet packet)
        {
            int id = packet.ReadInt(); 
            if(id == fromClient)
                Console.WriteLine(fromClient + "-" + Game.GetPlayer(fromClient).Name + " : " + packet.ReadString()); 
        }

        public static void UDPTestReceived(int fromClient, Packet packet)
        {
            string msg = packet.ReadString();
            ServerSend.TcpPlayersSync(fromClient);
            ServerSend.TcpAddPlayer(fromClient);
            //Console.WriteLine($"Received packet via UDP. Contains message: {msg}");
        }

        public static void TcpCommandReceived(int idClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            string msg = packet.ReadString();
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
                            Console.WriteLine("Tp command issued");
                            break;
                        case "/w":
                            Console.WriteLine("Whisper command issued");
                            break;
                        default:
                            Console.WriteLine("Unrecognized command");
                            break;
                    }
                }
                return; 
            }

            if (clientIdCheck != idClient)
            {
                Console.WriteLine("L'id is corrupted " + clientIdCheck);
                return; 
            }
            Console.WriteLine($"[{Game.GetPlayer(idClient).Name }, {idClient}]: { msg }");
            ServerSend.TcpSpreadChatMessageToAll(Game.GetPlayer(idClient), msg); 
        }

        public static void UdpUpdatePlayer(int fromClient, Packet packet)
        {
            Vector2 position = new Vector2(packet.ReadFloat(), packet.ReadFloat());
            float feetRadian = packet.ReadFloat(); 
            float lookAtRadian = packet.ReadFloat();
            int health = packet.ReadInt(); 

            string msg = $"Posistion = {position.ToString()}, FeetRadian = {feetRadian.ToString("0.##")}, LookAtRadian = {lookAtRadian.ToString("0.##")}";
            Game.UpdatePlayer(fromClient, position, feetRadian, lookAtRadian, health);
            //ServerSend.UdpUpdatePlayers(fromClient);
            //Console.WriteLine($"Received packet via UDP from ID { fromClient }. Contains message: {msg}");
            //Game.PrintDebug(); 
        }

        public static void ChangeNameRequest(int fromClient, Packet packet)
        {
            if(!checkIdIntegrity(fromClient, packet))
                ServerSend.TcpChangeNameResult(fromClient, 403, string.Empty);

            string name = packet.ReadString();
            Game.GetPlayer(fromClient).Name = name;
            ServerSend.TcpChangeNameResult(fromClient, 200, name);
        }

        public static void CreateBullet(int fromClient, Packet packet)
        {
            if (!checkIdIntegrity(fromClient, packet))
                return;
            var position = new Vector2(packet.ReadFloat(), packet.ReadFloat());
            var direction = packet.ReadFloat();
            var velocity = packet.ReadFloat();
            var owner = Game.GetPlayer(packet.ReadInt());
            Bullet bullet = new Bullet(position)
            {
                Direction = direction,
                Velocity = velocity,
                Owner = owner, 
            };
            Game.AddGameObject(bullet);
            Console.WriteLine("Bullet created, id : " + bullet.ID);
        }

        public static void Disconnect(int fromClient, Packet packet)
        {
            if (!checkIdIntegrity(fromClient, packet))
                return;
            GameServer.GetClient(fromClient).tcp.DisconnectClient(); 
            Game.RemovePlayer(fromClient);
        }

        public static bool checkIdIntegrity(int fromClient, Packet packet)
        {
            int CheckID = packet.ReadInt();
            return fromClient == CheckID; 
        }
    }
}
