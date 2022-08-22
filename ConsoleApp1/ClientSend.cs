using GameServerTCP;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientSend
    {
        public delegate void SendProtocol(object obj, int idHandler);
        public static Dictionary<int, SendProtocol> protocolHandlers = new Dictionary<int, SendProtocol>()
        {
            {(int)ClientPackets.UdpUpdatePlayer, UdpUpdatePlayer},
        };

        private static void SendTcpData(Packet packet)
        {
            packet.WriteLength();
            Client.tcp.SendData(packet);
        }

        private static void SendUdpData(Packet packet)
        {
            packet.WriteLength();
            Client.udp.SendData(packet);
        }

        public static void WelcomeReceived()
        {
            using (Packet packet = new Packet((int)ClientPackets.WelcomeReceived))
            {
                packet.Write(Client.myId);
                packet.Write(Client.name);
                packet.Write("Hello from Client");
                SendTcpData(packet);
            }
        }

        public static void UdpMessage(object obj, int idHandler)
        {
            using (Packet _packet = new Packet(idHandler))
            {
                if(obj is string)
                {
                    _packet.Write(Client.myId);
                    _packet.Write((string)obj);
                    SendUdpData(_packet); 
                }
                else
                {
                    Console.WriteLine("You can't send object that are not strings with this methods"); 
                    return; 
                }
            }
        }

        public static void UDPTestReceived()
        {
            //using (Packet _packet = new Packet((int)ClientPackets.UdpTest))
            //{
            //    _packet.Write("Received a UDP packet.");

            //    SendUdpData(_packet);
            //}
        }

        public static void UdpUpdatePlayer(object obj, int idHandler)
        {
           
        }
        public static void SendTcpChatMessage(object obj, int idHandler)
        {
            if (obj is string)
            {
                //using (Packet packet = new Packet((int)ClientPackets.TcpIssuedCommand))
                //{
                //    packet.Write(Client.myId);
                //    packet.Write(obj as string);
                //    SendTcpData(packet);
                //}
            }
            else
            {
                throw new Exception("SendTcpMessage need a string as paramter");
            }

        }

        public static void SendMessage(object obj, int idHandler)
        {
            protocolHandlers[idHandler](obj, idHandler);
        }
    }
}
