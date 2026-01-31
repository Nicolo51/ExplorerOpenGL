using GameServerTCP.GameData.GameObjects;
using GameServerTCP.HttpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace GameServerTCP.GameData
{
    public class Game
    {
        private static Dictionary<int, Player> Players;
        private static List<GameObject> GameObjects;
        private static int nextGameObjectID; 
        public static Timer timer;
        public static DateTime startTime;
        public static DateTime LastUpdate;
        public static TimeSpan  TimeSpanSinceGameStarted { get { return DateTime.Now - startTime; } }
        public const int tickRate = 30;
        private static int ticktime; 
        public static void Start()
        {
            nextGameObjectID = 0; 
            startTime = DateTime.Now;
            GameObjects = new List<GameObject>(); 
            timer = new Timer(new TimerCallback(gameTick));
            timer.Change(1000, tickRate);
            Players = new Dictionary<int, Player>();
            ServerWeb.Start(); 
        }

        public static void AddPlayer(Player player)
        {
            Players.Add(player.ID, player); 
        }

        public static void UpdatePlayer(int id, Vector2 postion, int health, string currentAnimationName, int effect)
        {
            lock (Players)
            {
                Player p = Players[id];
                p.Position = postion;
                p.Health = health;
                p.CurrentAnimationName = currentAnimationName;
                p.SpriteEffect = effect;
            }
        }

        public static void RemovePlayer(int id)
        {
            lock (Players)
            {
                if (!Players.ContainsKey(id))
                    return;
                string playerName = Players[id].Name;
                Players.Remove(id);
                ServerSend.DisconnectPlayer(id, playerName);
                GameServer.Log(playerName + " left the game");
            }
        }

        private static void gameTick(object state)
        {
            ticktime++; 
            UpdateGameObjects(DateTime.Now - LastUpdate); 
            LastUpdate = DateTime.Now; 
            ServerSend.UdpUpdatePlayers();
        }

        public static void PrintDebug()
        {
            Console.SetCursorPosition(0, 2);
            lock (Players)
            {
                foreach (KeyValuePair<int, Player> entry in Players)
                {
                    GameServer.Log($"\r{entry.Value.ToString()}    ");
                }
            }
        }

        private static void UpdateGameObjects(TimeSpan lastupdate)
        {
            lock (GameObjects)
            {
                for (int i = 0; i < GameObjects.Count; i++)
                {
                    if (!GameObjects[i].IsRemove)
                    {
                        GameObjects[i].Update(lastupdate, GetPlayers(), GetGameObjects());
                        continue; 
                    }
                    GameObjects[i].Remove();
                    if (GameObjects[i].RemoveTick > 300)
                    {
                        GameObjects.RemoveAt(i); 
                        if (i > 0)
                            i--;
                    }
                }
                GameServer.DisplayLogDirect(Players.Values.ToArray()); 
            }
        }

        public static Player[] GetPlayers()
        {
            Player[] players;
            lock (Players)
            {
                players = new Player[Players.Count];
                Players.Values.CopyTo(players, 0);
            }
            return players;
        }

        public static Player GetPlayer(int id)
        {
            lock (Players)
                return Players[id]; 
        }

        public static void AddPlayer(int id, Player player)
        {
            lock (Players)
                Players.Add(id, player); 
        }

        public static int RequestNewGameObjectID()
        {
            return nextGameObjectID++; 
        }

        public static GameObject[] GetGameObjects()
        {
            lock (GameObjects)
                return GameObjects.ToArray();
        }

        public static void AddGameObject(GameObject gameObject)
        {
            lock (GameObjects)
                GameObjects.Add(gameObject);
        }

        public static void RemoveGameObject(GameObject gameObject)
        {
            lock (GameObjects)
                GameObjects.Remove(gameObject); 
        }

    }
}
