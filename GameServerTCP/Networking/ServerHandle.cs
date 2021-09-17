using GameServerTCP.GameData;
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
            Console.WriteLine(GameServer.clients[fromClient].tcp.socket.Client.RemoteEndPoint + " successfully connected and is now connected as " + name + " with id : " + fromClient);
            if (clientIdCheck != fromClient)
            {
                Console.WriteLine("L'id is corrupted " + clientIdCheck);
                return; 
            }
            Game.AddPlayer(new Player(clientIdCheck, name));
        }

        public static void UdpMessageReceived(int fromClient, Packet packet)
        {
            int id = packet.ReadInt(); 
            if(id == fromClient)
                Console.WriteLine(fromClient + "-" + Game.Players[fromClient].Name + " : " + packet.ReadString()); 

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
            Console.WriteLine($"[{Game.Players[idClient].Name }, {idClient}]: { msg }");
            ServerSend.TcpSpreadChatMessageToAll(Game.Players[idClient], msg); 
        }

        public static void UdpUpdatePlayer(int fromClient, Packet packet)
        {
            Vector2 Position = new Vector2(packet.ReadFloat(), packet.ReadFloat());
            float FeetRadian = packet.ReadFloat(); 
            float LookAtRadian = packet.ReadFloat();

            string msg = $"Posistion = {Position.ToString()}, FeetRadian = {FeetRadian.ToString("0.##")}, LookAtRadian = {LookAtRadian.ToString("0.##")}";
            Game.UpdatePlayer(fromClient, Position, FeetRadian, LookAtRadian);
            //ServerSend.UdpUpdatePlayers(fromClient);
            //Console.WriteLine($"Received packet via UDP from ID { fromClient }. Contains message: {msg}");
            //Game.PrintDebug(); 
        }

        public static void ChangeNameRequest(int fromClient, Packet packet)
        {
            if(checkIdIntegrity(fromClient, packet))
            {
                string name = packet.ReadString();
                Game.Players[fromClient].Name = name;
                ServerSend.TcpChangeNameResult(fromClient, true, name);
            }
            else
                ServerSend.TcpChangeNameResult(fromClient, false, string.Empty);
        }

        public static bool checkIdIntegrity(int fromClient, Packet packet)
        {
            int CheckID = packet.ReadInt();
            return fromClient == CheckID; 
        }
    }
}
