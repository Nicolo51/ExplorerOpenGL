using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameServerTCP.GameData
{
    public class Game
    {
        public static Dictionary<int, Player> Players;
        public static Timer timer;
        public const int tickRate = 16; 
        public static void Start()
        {
            timer = new Timer(new TimerCallback(gameTick));
            timer.Change(1000, tickRate);
            Players = new Dictionary<int, Player>();
        }

        public static void AddPlayer(Player player)
        {
            Players.Add(player.ID, player); 
        }

        public static void UpdatePlayer(int id, Vector2 postion, float feetRadian, float lootAtRadian)
        {
            
            Players[id].Position = postion;
            Players[id].FeetRadian = feetRadian;
            Players[id].LookAtRadian = lootAtRadian;
        }
        public static void movePlayer(int id, Vector2 position)
        {
            //Send Tcp request to client... 
            //Change position on the server...
        }

        public static void RemovePlayer(int id)
        {
            string playerName = Players[id].Name;  
            Players.Remove(id);
            ServerSend.DisconnectPlayer(id);
            Console.WriteLine(playerName + " left the game");
        }

        private static void gameTick(object state)
        {
            //Console.WriteLine("tik tok");
            using (Packet packet = new Packet((int)ServerPackets.UdpUpdatePlayers))
            {
                lock (Game.Players)
                {
                    foreach (KeyValuePair<int, Player> entry in Game.Players)
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
                ServerSend.SendUDPDataToAll(packet);
            }
        }

        public static void PrintDebug()
        {
            Console.SetCursorPosition(0, 2); 
            foreach(KeyValuePair<int, Player> entry in Players)
            {
                Console.WriteLine($"\r{entry.Value.ToString()}    ");
            }
        }
    }
}
