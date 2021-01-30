using GameServerTCP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientHandle
    {
        public static void OnWelcomeResponse(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();
            int tickRate = packet.ReadInt();
            Console.WriteLine("Message from the server : " + msg);
            Client.myId = id;
            ClientSend.WelcomeReceived();
            Client.udp.Connect(((IPEndPoint)Client.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void OnUdpTestResponse(Packet _packet)
        {
            string _msg = _packet.ReadString();

            Console.WriteLine($"Received test packet via UDP. Contains message: {_msg}");
            ClientSend.UDPTestReceived();
            Console.WriteLine($"You are now connected with id { Client.myId } and the name {Client.name }");
        }

        public static void OnTcpMessage(Packet _packet)
        {
            string _msg = _packet.ReadString();

            Console.WriteLine($"Received packet via TCP. Contains message: {_msg}");
        }
        public static void OnUdpMessage(Packet _packet)
        {
            string _msg = _packet.ReadString();

            Console.WriteLine($"Received packet via UDP. Contains message: {_msg}");
        }

        public static void OnTcpPlayersSync(Packet packet)
        {
            while (packet.ReadBool())
            {
                int idPlayer = packet.ReadInt();
                string name = packet.ReadString();
                if (idPlayer != Client.myId)
                {
                    Console.WriteLine(idPlayer + " : " + name);
                }
            }
        }

        public static void OnChatMessage(Packet packet)
        {
            string _name = packet.ReadString();
            string msg = packet.ReadString();
            Console.WriteLine("["+ _name +"] : " + msg); 
        }

        public static void OnTcpAddPlayer(Packet packet)
        {
            int idPlayer = packet.ReadInt();
            string name = packet.ReadString();
          
        }

        public static void OnUdpUpdatePlayers(Packet packet)
        {
            
        }
    }
}