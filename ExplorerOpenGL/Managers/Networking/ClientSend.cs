using ExplorerOpenGL2.Managers.Networking.EventArgs;
using ExplorerOpenGL2.Model.Sprites;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking
{
    public class ClientSend : IDisposable
    {
        private NetPeer peer; 
        public delegate void SendProtocol(object obj);
        public Dictionary<int, SendProtocol> protocolHandlers;
        private int clientId; 

        public ClientSend(NetPeer peer)
        {
            this.peer = peer;
            InitSendData(); 
        }

        public void SendResponseWelcome(string name, int myId)
        {
            clientId = myId;
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ClientPackets.WelcomeReceived);
            packet.Put(myId);
            packet.Put(name);
            packet.Put($"Hello client here{myId}");
            peer.Send(packet, DeliveryMethod.ReliableSequenced);
            
        }

        public void SendUDPTest()
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ClientPackets.UdpTestRecieved);
            packet.Put("Received a UDP packet.");
            peer.Send(packet, DeliveryMethod.Sequenced);
        }

        public void UdpUpdatePlayer(object obj)
        {
            if (obj is Player) 
            {
                Player player = (obj as Player);
                NetDataWriter packet = new NetDataWriter();
                packet.Put((int)ClientPackets.UdpUpdatePlayer); 
                packet.Put(player.Position.X);
                packet.Put(player.Position.Y);
                packet.Put(player.Health);
                packet.Put(player.CurrentAnimationName); 
                packet.Put((int)player.Effect);
                peer.Send(packet, DeliveryMethod.Sequenced);
            }
        }
        public void SendTcpChatMessage(object obj)
        {
            if(obj is string)
            {
                NetDataWriter packet = new NetDataWriter();
                packet.Put((int)ClientPackets.TcpChatMessage); 
                packet.Put(clientId);
                packet.Put(obj as string);
                peer.Send(packet, DeliveryMethod.ReliableOrdered);
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
                NetDataWriter packet = new NetDataWriter();
                packet.Put((int)ClientPackets.ChangeNameRequest);
                packet.Put(clientId);
                packet.Put(obj as string);
                peer.Send(packet, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                throw new Exception("RequestChangeName need a string as paramter");
            }
        }

        public void CreateBullet(object arg)
        {
            Player player = arg as Player;
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ClientPackets.CreateBullet);
            packet.Put(clientId);
            packet.Put(player.Position.X); 
            packet.Put(player.Position.Y); 
            packet.Put(player.Radian); 
            packet.Put(2f);
            packet.Put(clientId);
            peer.Send(packet, DeliveryMethod.Sequenced); 
        }

        public void SendMessage(object obj, int idHandler)
        {
            protocolHandlers[idHandler](obj);
        }

        public void Disconnect(object obj)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ClientPackets.Disconnect);
            packet.Put(clientId);
            peer.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        private void UpdateGameState(object obj)
        {
            NetDataWriter packet = obj as NetDataWriter;
            peer.Send(packet, DeliveryMethod.Sequenced);

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
                {(int)ClientPackets.UpdateGameState, UpdateGameState},
            };
        }

       

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //get rid of managed resources
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
