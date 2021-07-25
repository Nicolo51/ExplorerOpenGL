using GameServerTCP.GameData;
using System;
using System.Threading;

namespace GameServerTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.Start(); 
            GameServer.Start(25789);
            string input; 
            while((input = Console.ReadLine()).ToLower() != "exit")
            {
                string[] query = input.Split(" ");
                string commande = query[0];
                switch (commande.ToLower())
                {
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
                }
            }
            Console.ReadLine();
        }
    }
}
