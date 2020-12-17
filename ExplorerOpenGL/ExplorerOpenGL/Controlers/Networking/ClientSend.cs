using ExplorerOpenGL.Model.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class ClientSend
    {
        public delegate void SendProtocol(object obj, int idHandler);
        public static Dictionary<int, SendProtocol> protocolHandlers = new Dictionary<int, SendProtocol>()
        {
            {(int)ClientPackets.TcpMessageRecieved, SendTcpMessage},
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
            using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                packet.Write(Client.myId);
                packet.Write("Hello from server");
                SendTcpData(packet);
            }
        }

        public static void UDPTestReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.udpTestRecieved))
            {
                _packet.Write("Received a UDP packet.");

                SendUdpData(_packet);
            }
        }

        public static void UdpUpdatePlayer(object obj, int idHandler)
        {
            if (obj is Player) 
            {
                Player player = (obj as Player); 
                using (Packet packet = new Packet((int)ClientPackets.UdpUpdatePlayer))
                {
                    packet.Write(player.Position.X);
                    packet.Write(player.Position.Y);
                    packet.Write(player.PlayerFeetRadian);
                    packet.Write(player.Radian);
                    SendUdpData(packet);
                }
            }
        }
        public static void SendTcpMessage(object obj, int idHandler)
        {
            if(obj is string)
            {
                using (Packet packet = new Packet((int)ClientPackets.TcpMessageRecieved))
                {
                    packet.Write(Client.myId);
                    packet.Write(obj as string);
                    SendTcpData(packet);
                }
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
