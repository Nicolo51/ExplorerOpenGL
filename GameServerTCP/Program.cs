using GameServerTCP.GameData;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Web.Http;

namespace GameServerTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine(3%0.3);
            Game.Start(); 
            GameServer.Start(25789);
            string input; 
            while((input = Console.ReadLine()).ToLower().Trim() != "exit")
            {
                string[] query = input.Split(" ");
                string commande = query[0];
                switch (commande.ToLower().Trim())
                {
                    case "tp":
                        int id = int.Parse(query[1]);
                        Vector2 pos = new Vector2(int.Parse(query[2]), int.Parse(query[3]));
                        ServerSend.RequestChangePlayerPosition(id, pos);
                    break;
                    case "fire":
                        ServerSend.UpdateGameObject();
                        break;
                    case "udp":
                        int udpid = Int32.Parse(query[1]);
                        string udpmsg = query[2]; 
                        using(Packet packet = new Packet((int)ServerPackets.UdpMessage))
                        {
                            packet.Write(udpmsg);
                            ServerSend.UdpMessage(udpid, packet);
                        }
                        break;
                    case "tcp":
                        int tcpid = Int32.Parse(query[1]);
                        string tcpmsg = query[2];
                        using (Packet packet = new Packet((int)ServerPackets.TcpMessage))
                        {
                            packet.Write(tcpmsg);
                            ServerSend.TcpMessage(tcpid, packet);
                        }
                        break;
                    case "udpall":
                        string udpallmsg = query[1];
                        using (Packet packet = new Packet((int)ServerPackets.UdpMessage))
                        {
                            packet.Write(udpallmsg);
                            ServerSend.SendUDPDataToAll(packet);
                        }
                        break;
                    case "tcpall":
                        string tcpallmsg = query[1];
                        using (Packet packet = new Packet((int)ServerPackets.TcpMessage))
                        {
                            packet.Write(tcpallmsg);
                            ServerSend.SendTcpDataToAll(packet);
                        }
                        break;
                    case "show":
                        var players = Game.GetPlayers();
                        
                        foreach (var p in players)
                        {
                            var player = Game.GetPlayer(p.ID); 
                            Console.WriteLine($"{p.ID} : {p.Name}");
                        }
                        break; 
                }
            }
        }
    }
}
