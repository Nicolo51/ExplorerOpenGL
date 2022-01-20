using GameServerTCP.GameData;
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

        public static void TcpAddPlayer(int idClient)
        {
            using(Packet packet = new Packet((int)ServerPackets.TcpAddPlayer))
            {
                packet.Write(idClient);
                packet.Write(Game.Players[idClient].Name);
                packet.WriteLength();
                for (int i = 1; i <= GameServer.maxPlayer; i++)
                {
                    if (idClient == GameServer.clients[i].id)
                        continue; 
                    GameServer.clients[i].SendData(packet);
                }
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
                lock (Game.Players)
                {
                    foreach (KeyValuePair<int, Player> entry in Game.Players)
                    {
                        packet.Write(true);
                        packet.Write(entry.Value.ID);
                        packet.Write(entry.Value.Name);
                    }
                    packet.Write(false);
                }
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
                lock (Game.Players)
                {
                    foreach(KeyValuePair<int, Player> entry in Game.Players)
                    {
                        packet.Write(true);
                        packet.Write(entry.Value.ID);
                        packet.Write(entry.Value.Position.X); 
                        packet.Write(entry.Value.Position.Y); 
                        packet.Write(entry.Value.LookAtRadian);
                        packet.Write(entry.Value.FeetRadian);
                    }
                }
                packet.Write(false);
                SendUDPData(toClient, packet);
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
                GameServer.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPData(int toClient, Packet _packet)
        {
            _packet.WriteLength();
            GameServer.clients[toClient].udp.SendData(_packet);
        }

        public static void SendTcpDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= GameServer.maxPlayer; i++)
            {
                GameServer.clients[i].SendData(packet);
            }
        }

        private static void SendTcpData(int toClient, Packet packet)
        {
            packet.WriteLength();
            GameServer.clients[toClient].SendData(packet); 
        }
        #endregion
    }
}
