using GameServerTCP.GameData;
using GameServerTCP.GameData.GameObjects;
using LiteNetLib;
using LiteNetLib.Utils;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GameServerTCP
{
    public class ServerSend
    {
        public static void Welcome(int toClient)
        {
            NetDataWriter packet = new NetDataWriter();

            packet.Put((int)ServerPackets.Welcome);
            packet.Put($"Successfully connected to the server with the ID : {toClient}");
            packet.Put(toClient);
            packet.Put(Game.tickRate);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered);
        }

        public static void RequestAcknowledgeme(int toClient, ClientPackets clientPacket, bool status)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.Sync); 
            packet.Put((int)clientPacket);
            packet.Put(status);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered); 
        }

        public static void AddPlayer(int idClient)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.TcpAddPlayer); 
            packet.Put(idClient);
            packet.Put(Game.GetPlayer(idClient).Name);

            ServerClient[] clients = GameServer.GetClients(); 
            for (int i = 0; i < clients.Length; i++)
            {
                if (idClient == clients[i].id)
                    continue;
                clients[i].clientPeer.Send(packet, DeliveryMethod.ReliableUnordered); 
            }
        }

        public static void RequestChangePlayerPosition(int toClient, Vector2 pos)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.Teleport); 
            packet.Put(pos.X);
            packet.Put(pos.Y);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered);
        }

        public static void UDPTest(int toClient)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.UdpTest);
            SendData(toClient, packet, DeliveryMethod.Sequenced);
        }

        public static void PlayersSync(int toClient)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.TcpPlayersSync);
            var players = Game.GetPlayers(); 
            foreach (var player in players)
            {
                packet.Put(true);
                packet.Put(player.ID);
                packet.Put(player.Name);
            }
            packet.Put(false);
            SendData(toClient, packet, DeliveryMethod.ReliableOrdered); 
        }
        public static void DisconnectPlayer(int id, string playerName)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.DisconnectPlayer);
            packet.Put(id);
            packet.Put(playerName);
            SendDataToAll(packet, DeliveryMethod.ReliableOrdered); 
        }

        public static void UpdatePlayers(int toClient, NetDataWriter packet)
        {
            SendData(toClient, packet, DeliveryMethod.Sequenced);
        }

        public static void UpdateGameObject(int toClient, NetDataWriter packet)
        {
           SendData(toClient, packet, DeliveryMethod.Sequenced);
        }

        public static void UdpUpdatePlayers()
        {
            NetDataWriter packet = new NetDataWriter(); 
            packet.Put((int)ServerPackets.UdpUpdatePlayers); 
            Player[] players = Game.GetPlayers();
            foreach (var entry in players)
            {
                packet.Put(true);
                packet.Put(entry.ID);
                packet.Put(entry.Position.X);
                packet.Put(entry.Position.Y);
                packet.Put(entry.Health);
                packet.Put(entry.CurrentAnimationName);
                packet.Put(entry.SpriteEffect);
            }
            packet.Put(false);
            foreach (var entry in players)
            {
                UpdatePlayers(entry.ID, packet);
            }
        }
        public static void TcpChangeNameResult(int toClient, int response, string name)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.ChangeNameResult); 
            packet.Put(response);
            packet.Put(name);
            packet.Put(toClient);
            SendDataToAll(packet, DeliveryMethod.ReliableUnordered);
        }

        public static void TcpSpreadChatMessageToAll(Player player, string message)
        {
            NetDataWriter packet = new NetDataWriter();
            packet.Put((int)ServerPackets.TcpChatMessage);
            packet.Put(player.Name);
            packet.Put(message);
            SendDataToAll(packet, DeliveryMethod.ReliableOrdered);
        }

        #region Low level function
        public static void SendDataToAll(NetDataWriter packet, DeliveryMethod dm)
        {
            for (int i = 0; i <= GameServer.maxPlayer; i++)
            {
                var c = GameServer.GetClient(i);
                if (c == null)
                    continue;
                c.clientPeer.Send(packet, dm);
            }
        }

        public static void SendData(int toClient, NetDataWriter packet, DeliveryMethod dm)
        {
            var client = GameServer.GetClient(toClient); 
            client.clientPeer.Send(packet, dm); 
        }
        #endregion
    }
}
