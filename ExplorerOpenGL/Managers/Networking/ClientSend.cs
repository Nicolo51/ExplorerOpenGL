﻿using ExplorerOpenGL.Managers.Networking.EventArgs;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking
{
    public class ClientSend : IDisposable
    {
        private Client client; 
        public delegate void SendProtocol(object obj);
        public Dictionary<int, SendProtocol> protocolHandlers;

        public ClientSend(Client client)
        {
            this.client = client;
            InitSendData(); 
        }

        private void SendTcpData(Packet packet)
        {
            packet.WriteLength();
            client.tcp.SendData(packet);
        }

        private void SendUdpData(Packet packet)
        {
            packet.WriteLength();
            client.udp.SendData(packet);
        }

        public void SendResponseWelcome(string name)
        {
            using (Packet packet = new Packet((int)ClientPackets.WelcomeReceived))
            {
                packet.Write(client.myId);
                packet.Write(name);
                packet.Write("Hello client here");
                SendTcpData(packet);
            }
        }

        public void SendUDPTest()
        {
            using (Packet _packet = new Packet((int)ClientPackets.UdpTestRecieved))
            {
                _packet.Write("Received a UDP packet.");

                SendUdpData(_packet);
            }
        }

        public void UdpUpdatePlayer(object obj)
        {
            if (obj is Player) 
            {
                Player player = (obj as Player); 
                using (Packet packet = new Packet((int)ClientPackets.UdpUpdatePlayer))
                {
                    packet.Write(player.Position.X);
                    packet.Write(player.Position.Y);
                    packet.Write(player.Health);
                    packet.Write(player.CurrentAnimationName); 
                    packet.Write((int)player.Effects);
                    SendUdpData(packet);
                }
            }
        }
        public void SendTcpChatMessage(object obj)
        {
            if(obj is string)
            {
                using (Packet packet = new Packet((int)ClientPackets.TcpChatMessage))
                {
                    packet.Write(client.myId);
                    packet.Write(obj as string);
                    SendTcpData(packet);
                }
            }
            else
            {
                throw new Exception("SendTcpMessage need a string as paramter"); 
            }
            
        }
        public void RequestChangeName(object obj)
        {
            if (obj is string)
            {
                using (Packet packet = new Packet((int)ClientPackets.ChangeNameRequest))
                {
                    packet.Write(client.myId);
                    packet.Write(obj as string);
                    SendTcpData(packet);
                }
            }
            else
            {
                throw new Exception("RequestChangeName need a string as paramter");
            }
        }

        public void CreateBullet(object arg)
        {
            Player player = arg as Player;
            using (Packet packet = new Packet((int)ClientPackets.CreateBullet))
            {
                packet.Write(client.myId);
                packet.Write(player.Position.X); 
                packet.Write(player.Position.Y); 
                packet.Write(player.Radian); 
                packet.Write(2f);
                packet.Write(client.myId);
                SendUdpData(packet); 
            }
        }

        public void SendMessage(object obj, int idHandler)
        {
            protocolHandlers[idHandler](obj);
        }

        private void InitSendData()
        {
            protocolHandlers = new Dictionary<int, SendProtocol>()
            {
                {(int)ClientPackets.TcpChatMessage, SendTcpChatMessage},
                {(int)ClientPackets.UdpUpdatePlayer, UdpUpdatePlayer},
                {(int)ClientPackets.ChangeNameRequest, RequestChangeName },
                {(int)ClientPackets.CreateBullet, CreateBullet },
                {(int)ClientPackets.Disconnect, Disconnect},
            };
        }

        public void Disconnect(object obj)
        {
            using (Packet packet = new Packet((int)ClientPackets.Disconnect))
            {
                packet.Write(client.myId);
                SendTcpData(packet); 
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //get rid of managed resources
                client = null;
                protocolHandlers.Clear();
                protocolHandlers = null;
            }
            //get rid of unmanaged resources
            //nothing for now
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
