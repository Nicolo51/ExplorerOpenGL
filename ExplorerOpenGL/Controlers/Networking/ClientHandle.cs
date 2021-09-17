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
        private Client client; 
        private ClientSend clientSend;
        private Controler controler; 

        public ClientHandle(Client client, ClientSend clientSend, Controler controler)
        {
            this.client = client;
            this.controler = controler; 
            this.clientSend = clientSend; 
        }

        public void OnWelcomeResponse(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();
            int tickRate  = packet.ReadInt();
            Console.WriteLine("Message from the server : " + msg);
            client.myId = id;
            client.serverTickRate = tickRate;
            client.PlayersData.Add(id, new PlayerData(id)); 
            clientSend.WelcomeReceived();
            client.udp.Connect(((IPEndPoint)client.tcp.socket.Client.LocalEndPoint).Port);
        }

        public void OnUdpTestResponse(Packet _packet)
        {
            string _msg = _packet.ReadString();

            Console.WriteLine($"Received test packet via UDP. Contains message: {_msg}");
            clientSend.UDPTestReceived();
        }

        public void OnTcpMessage(Packet _packet)
        {
            string _msg = _packet.ReadString();

            client.controler.DebugManager.AddEvent($"Received packet via TCP. Contains message: {_msg}");
        } 
        public void OnUdpMessage(Packet _packet)
        {
            string _msg = _packet.ReadString();

            client.controler.DebugManager.AddEvent($"Received packet via UDP. Contains message: {_msg}");
        }

        public void OnTcpPlayersSync(Packet packet)
        {
            while (packet.ReadBool())
            {
                int idPlayer = packet.ReadInt();
                string name = packet.ReadString();
                if (idPlayer != client.myId)
                {
                    client.PlayersData.Add(idPlayer, new PlayerData(idPlayer, name));
                }
            }
        }

        public void OnChatMessage(Packet packet)
        {
            string name = packet.ReadString();
            string msg = packet.ReadString();
            client.controler.Terminal.AddMessageToTerminal(msg, name, Color.Black);
        }

        public void OnChangeNameResult(Packet packet)
        {
            bool result = packet.ReadBool();
            string name = packet.ReadString();
            if (result && !string.IsNullOrWhiteSpace(name))
                controler.Terminal.AddMessageToTerminal("Successfuly change username to : " + name, "Info", Color.Green); 
            else
                controler.Terminal.AddMessageToTerminal("Failed to change username", "Error", new Color(181, 22, 11));
        }

        public void OnTcpAddPlayer(Packet packet)
        {
            int idPlayer = packet.ReadInt();
            string name = packet.ReadString();
            if (!client.PlayersData.ContainsKey(idPlayer) && idPlayer != client.myId)
            {
                client.PlayersData.Add(idPlayer, new PlayerData(idPlayer, name)); 
            }
        }

        public void OnUdpUpdatePlayers(Packet packet)
        {
            while (packet.ReadBool())
            {
                int idPlayer = packet.ReadInt();
                if (client.PlayersData.ContainsKey(idPlayer))
                {
                    client.PlayersData[idPlayer].ServerPosition = new Vector2(packet.ReadFloat(), packet.ReadFloat());
                    client.PlayersData[idPlayer].LookAtRadian = packet.ReadFloat();
                    client.PlayersData[idPlayer].FeetRadian = packet.ReadFloat();
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