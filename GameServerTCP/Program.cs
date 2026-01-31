using GameServerTCP.GameData;
using GameServerTCP.HttpServer;
using LiteNetLib;
using LiteNetLib.Utils;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace GameServerTCP
{
    class Program
    {

        static Task server;  
        static void Main(string[] args)
        {

            server = Task.Factory.StartNew(() => {
                Game.Start();
                GameServer.Start(25789);
            });
            Console.CursorVisible = false;
            string input = string.Empty;
            ConsoleKeyInfo cki;
            
            while(GameServer.currentInput.ToLower().Trim() != "exit")
            {
                while ((cki = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                {
                    if (cki.Key == ConsoleKey.Backspace && GameServer.currentInput.Length > 0)
                    {
                        GameServer.currentInput = GameServer.currentInput.Substring(0, GameServer.currentInput.Length - 1);
                        continue; 
                    }
                    GameServer.currentInput += (cki.KeyChar.ToString());
                }
                string[] query = GameServer.currentInput.ToLower().Trim().Split(" ");
                NetDataWriter packet = new NetDataWriter();
                GameServer.currentInput = string.Empty;
                string commande = query[0];
                switch (commande.ToLower().Trim())
                {
                    case "mode":
                        if (query.Length < 2)
                        {           
                            GameServer.Log("To little arguements");
                            break;
                        }
                        string mode = query[1].Trim().ToLower();
                        int ms = 0; 
                        if( query.Length == 3)
                            int.TryParse(query[2], out ms);

                        int i = -1;
                        if (int.TryParse(query[1], out i))
                        {
                            GameServer.ChangeLogMode((LogMode)i, ms);
                            break; 
                        }
                        switch (mode)
                        {
                            case "classic": 
                                GameServer.ChangeLogMode(LogMode.classic, 0);
                                break;
                            case "direct":
                                GameServer.ChangeLogMode(LogMode.direct, ms);
                                break;
                            default:
                                GameServer.Log("Unknown argument for 'mode' function"); 
                                break;
                        }
                    break; 
                    case "tp":
                        int id = int.Parse(query[1]);
                        Vector2 pos = new Vector2(int.Parse(query[2]), int.Parse(query[3]));
                        ServerSend.RequestChangePlayerPosition(id, pos);
                    break;
                    case "udp":
                        int toClient = Int32.Parse(query[1]);
                        string udpmsg = query[2];
                        packet.Put((int)ServerPackets.UdpMessage); 
                        packet.Put(udpmsg);
                        ServerSend.SendData(toClient, packet, DeliveryMethod.Sequenced);
                        break;
                    case "tcp":
                        int tcpid = Int32.Parse(query[1]);
                        string tcpmsg = query[2];
                        packet.Put((int)ServerPackets.TcpMessage);
                        packet.Put(tcpmsg);
                        ServerSend.SendData(tcpid, packet, DeliveryMethod.ReliableOrdered);
                        break;
                    case "udpall":
                        string udpallmsg = query[1];
                        packet.Put((int)ServerPackets.UdpMessage); 
                        packet.Put(udpallmsg);
                        ServerSend.SendDataToAll(packet, DeliveryMethod.Sequenced);
                        break;
                    case "tcpall":
                        string tcpallmsg = query[1];
                        packet.Put((int)ServerPackets.TcpMessage); 
                        packet.Put(tcpallmsg);
                        ServerSend.SendDataToAll(packet, DeliveryMethod.ReliableOrdered);
                        break;
                    case "show":
                        var players = Game.GetPlayers();
                        
                        foreach (var p in players)
                        {
                            var player = Game.GetPlayer(p.ID); 
                            GameServer.Log($"{p.ID} : {p.Name}");
                        }
                        break;
                    case "changemap":
                        if(query.Length == 2)
                            ServerWeb.ChangeCurrentMap(query[1]); 
                        break;
                    default:
                        GameServer.Log($"{query[0]} unknown query.");
                        break;
                }
            }

            GameServer.StopServer();
            Thread.Sleep(1000);
        }

        private static void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine(reader.ToString());
        }
    }
}
