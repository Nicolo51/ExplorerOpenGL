using GameServerTCP.GameData;
using GameServerTCP.GameData.GameObjects;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerTCP
{
    public class ServerSend
    {
        public static void Welcome(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.Welcome))
            {
                packet.Write($"Successfully connected to the server with the ID : {toClient}");
                packet.Write(toClient);
                packet.Write(Game.tickRate);
                SendTcpData(toClient, packet);
            }
        }

        public static void RequestAcknowledgeme(int toClient, ClientPackets clientPacket, bool status)
        {
            using (Packet packet = new Packet((int)ServerPackets.Sync))
            {
                packet.Write((int)clientPacket);
                packet.Write(status);
                SendTcpData(toClient, packet); 
            }
        }

        public static void TcpAddPlayer(int idClient)
        {
            using(Packet packet = new Packet((int)ServerPackets.TcpAddPlayer))
            {
                packet.Write(idClient);
                packet.Write(Game.GetPlayer(idClient).Name);
                packet.WriteLength();
                for (int i = 1; i <= GameServer.maxPlayer; i++)
                {
                    if (idClient == GameServer.GetClient(i).id)
                        continue; 
                    GameServer.GetClient(i).SendData(packet);
                }
            }
        }

        public static void RequestModifyHealth(int toClient, int health)
        {
            using(Packet packet = new Packet((int)ServerPackets.ServerRequest))
            {
                packet.Write((int)ServerRequestTypes.ModifyPlayerHealth);
                packet.Write(health);
                SendTcpData(toClient, packet);
            }
        }
        public static void UDPTest(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.UdpTest))
            {
                packet.Write("A test packet for UDP.");

                SendUDPData(toClient, packet);
            }
        }

        public static void TcpPlayersSync(int toClient)
        {
            using(Packet packet = new Packet((int)ServerPackets.TcpPlayersSync))
            {
                var players = Game.GetPlayers(); 
                foreach (var player in players)
                {
                    packet.Write(true);
                    packet.Write(player.ID);
                    packet.Write(player.Name);
                }
                packet.Write(false);
                SendTcpData(toClient, packet); 
            }
        }

        public static void DisconnectPlayer(int id, string playerName)
        {
            using (Packet packet = new Packet((int)ServerPackets.DisconnectPlayer))
            {
                packet.Write(id);
                packet.Write(playerName);
                SendTcpDataToAll(packet); 
            }
        }

        public static void UdpUpdatePlayers(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.UdpUpdatePlayers))
            {
                Player[] players = Game.GetPlayers();
                
                foreach (var entry in players)
                {
                    packet.Write(true);
                    packet.Write(entry.ID);
                    packet.Write(entry.Position.X); 
                    packet.Write(entry.Position.Y); 
                    packet.Write(entry.LookAtRadian);
                    packet.Write(entry.FeetRadian);
                    packet.Write(entry.Health); 
                }
                packet.Write(false);
                SendUDPData(toClient, packet);
            }
        }

        public static void UpdateGameObject(int toClient)
        {
            using(Packet packet = new Packet((int)ServerPackets.UpdateGameObject))
            {
                GameObject[] gameObjects = Game.GetGameObjects();
                foreach (var entry in gameObjects)
                {
                    packet.Write(true);
                    entry.WriteIntoPacket(packet); 
                }
                packet.Write(false);
                SendUDPData(toClient, packet);
            }
        }

        public static void UdpUpdatePlayers()
        {
            Player[] players = Game.GetPlayers(); 
            foreach(var entry in players)
            {
                UdpUpdatePlayers(entry.ID);
            }
        }

        public static void UpdateGameObject()
        {
            Player[] players = Game.GetPlayers();
            foreach (var player in players)
            {
                UpdateGameObject(player.ID);
            }
        }

        public static void TcpChangeNameResult(int toClient, int response, string name)
        {
            using (Packet packet = new Packet((int)ServerPackets.ChangeNameResult))
            {
                packet.Write(response);
                packet.Write(name);
                packet.Write(toClient);
                SendTcpDataToAll(packet);
            }
        }

        public static void TcpSpreadChatMessageToAll(Player player, string message)
        {
            using (Packet packet = new Packet((int)ServerPackets.TcpChatMessage))
            {
                packet.Write(player.Name);
                packet.Write(message);
                SendTcpDataToAll(packet);
            }
        }

        public static void UdpMessage(int toClient, Packet paquet)
        {
            SendUDPData(toClient, paquet);
        }

        public static void TcpMessage(int toClient, Packet paquet)
        {
            SendTcpData(toClient, paquet);
        }
        #region Low level function
        public static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= GameServer.maxPlayer; i++)
            {
                GameServer.GetClient(i).udp.SendData(_packet);
            }
        }

        private static void SendUDPData(int toClient, Packet _packet)
        {
            _packet.WriteLength();
            GameServer.GetClient(toClient).udp.SendData(_packet);
        }

        public static void SendTcpDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= GameServer.maxPlayer; i++)
            {
                GameServer.GetClient(i).SendData(packet);
            }
        }

        private static void SendTcpData(int toClient, Packet packet)
        {
            packet.WriteLength();
            GameServer.GetClient(toClient).SendData(packet); 
        }
        #endregion
    }
}
