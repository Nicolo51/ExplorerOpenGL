using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class ClientHandle
    {
        public static void OnWelcomeResponse(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();
            int tickRate  = packet.ReadInt();
            Console.WriteLine("Message from the server : " + msg);
            Client.myId = id;
            Client.serverTickRate = tickRate; 
            Client.PlayersData.Add(id, new PlayerData(id)); 
            ClientSend.WelcomeReceived();
            Client.udp.Connect(((IPEndPoint)Client.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void OnUdpTestResponse(Packet _packet)
        {
            string _msg = _packet.ReadString();

            Console.WriteLine($"Received test packet via UDP. Contains message: {_msg}");
            ClientSend.UDPTestReceived();
        }

        public static void OnTcpMessage(Packet _packet)
        {
            string _msg = _packet.ReadString();

            Client.controler.DebugManager.AddEvent($"Received packet via TCP. Contains message: {_msg}");
        } 
        public static void OnUdpMessage(Packet _packet)
        {
            string _msg = _packet.ReadString();

            Client.controler.DebugManager.AddEvent($"Received packet via UDP. Contains message: {_msg}");
        }

        public static void OnTcpPlayersSync(Packet packet)
        {
            while (packet.ReadBool())
            {
                int idPlayer = packet.ReadInt();
                string name = packet.ReadString();
                if (idPlayer != Client.myId)
                {
                    Client.PlayersData.Add(idPlayer, new PlayerData(idPlayer, name));
                }
            }
        }

        public static void OnChatMessage(Packet packet)
        {
            string name = packet.ReadString();
            string msg = packet.ReadString();
            Client.controler.Chat.AddMessageToChat(msg, name, Color.Black);
        }

        public static void OnTcpAddPlayer(Packet packet)
        {
            int idPlayer = packet.ReadInt();
            string name = packet.ReadString();
            if (!Client.PlayersData.ContainsKey(idPlayer) && idPlayer != Client.myId)
            {
                Client.PlayersData.Add(idPlayer, new PlayerData(idPlayer, name)); 
            }
        }

        public static void OnUdpUpdatePlayers(Packet packet)
        {
            while (packet.ReadBool())
            {
                int idPlayer = packet.ReadInt();
                if (Client.PlayersData.ContainsKey(idPlayer))
                {
                    Client.PlayersData[idPlayer].ServerPosition = new Vector2(packet.ReadFloat(), packet.ReadFloat());
                    Client.PlayersData[idPlayer].LookAtRadian = packet.ReadFloat();
                    Client.PlayersData[idPlayer].FeetRadian = packet.ReadFloat();
                    //Debug.WriteLine(Client.PlayersData[idPlayer].ToString());
                }
                else
                {
                    //Debug.WriteLine("Un sync from the server, might lag a little while re syncing ");
                }
            }
        }
    }
}