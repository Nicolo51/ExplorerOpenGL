using ExplorerOpenGL2.Managers.Networking;
using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GameServerTCP
{
    public class ServerSend
    {
        private GameServer gameServer;

        public ServerSend(GameServer gameServer)
        {
            this.gameServer = gameServer;
        }
        public void Welcome(int toClient, string mapName, float packetNumber)
        {
            NetDataWriter packet = new NetDataWriter();

            packet.Put((int)ServerPackets.Welcome);
            packet.Put($"Successfully connected to the server with the ID : {toClient}");
            packet.Put(mapName); 
            packet.Put(packetNumber);
            packet.Put(toClient);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered);
        }

        public void RequestAcknowledgeme(int toClient, ClientPackets clientPacket, bool status)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.Sync); 
            packet.Put((int)clientPacket);
            packet.Put(status);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered); 
        }

        public void AddPlayer(int idClient)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.TcpAddPlayer); 
            packet.Put(idClient);
            packet.Put(gameServer.GetPlayer(idClient).Name);

            ServerClient[] clients = gameServer.GetClients(); 
            for (int i = 0; i < clients.Length; i++)
            {
                if (idClient == clients[i].id)
                    continue;
                clients[i].clientPeer.Send(packet, DeliveryMethod.ReliableUnordered); 
            }
        }

        public void RequestChangePlayerPosition(int toClient, Vector2 pos)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.Teleport); 
            packet.Put(pos.X);
            packet.Put(pos.Y);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered);
        }

        public void UDPTest(int toClient)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.UdpTest);
            SendData(toClient, packet, DeliveryMethod.Sequenced);
        }

        public void PlayersSync(int toClient)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.TcpPlayersSync);
            var players = gameServer.GetPlayers(); 
            foreach (var player in players)
            {
                packet.Put(true);
                packet.Put((player as Player).ID);
                packet.Put((player as Player).Name);
            }
            packet.Put(false);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered); 
        }
        public void DisconnectPlayer(int id, string playerName)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.DisconnectPlayer);
            packet.Put(id);
            packet.Put(playerName);
            SendDataToAll(packet, DeliveryMethod.ReliableOrdered); 
        }

        public void UpdatePlayers(int toClient, NetDataWriter packet)
        {
            SendData(toClient, packet, DeliveryMethod.Sequenced);
        }

        public void UpdateGameObject(int toClient, NetDataWriter packet)
        {
           SendData(toClient, packet, DeliveryMethod.Sequenced);
        }

        public void TcpChangeNameResult(int toClient, int response, string name)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.ChangeNameResult); 
            packet.Put(response);
            packet.Put(name);
            packet.Put(toClient);
            SendDataToAll(packet, DeliveryMethod.ReliableUnordered);
        }

        public void TcpSpreadChatMessageToAll(Player player, string message)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.TcpChatMessage);
            packet.Put(player.Name);
            packet.Put(message);
            SendDataToAll(packet, DeliveryMethod.ReliableOrdered);
        }
        public void SendGameState(NetDataWriter packet)
        {
            var clients = gameServer.GetAllClientsButServer();
            foreach (var client in clients)
            {
                client.clientPeer.Send(packet, DeliveryMethod.Unreliable);
            }
        }

        public void SendMap(byte[] data, int toClient)
        {
            NetDataWriter packet = new NetDataWriter();
            var client = gameServer.GetClient(toClient);

            packet.Put((int)ServerPackets.Map);
            packet.Put(data);

            client.clientPeer.Send(packet, DeliveryMethod.ReliableOrdered); 
        }
        #region Low level function
        public void SendDataToAll(NetDataWriter packet, DeliveryMethod dm)
        {
            var clients = gameServer.GetClients();
            foreach (var c in clients)
            {
                c.clientPeer.Send(packet, dm);
            }
        }

        public void SendData(int toClient, NetDataWriter packet, DeliveryMethod dm)
        {
            var client = gameServer.GetClient(toClient); 
            client.clientPeer.Send(packet, dm); 
        }

        
        #endregion
    }
}
