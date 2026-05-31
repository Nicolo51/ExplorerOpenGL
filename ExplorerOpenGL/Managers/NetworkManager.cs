using ExplorerOpenGL2.Managers.Networking;
using ExplorerOpenGL2.Managers.Networking.EventArgs;
using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using ExplorerOpenGL2.View;
using GameServerTCP;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Model.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExplorerOpenGL2.Managers
{
    public class NetworkManager
    {
        public static ConnectionState ConnectionState { get; private set; }
        public static bool IsConnectedToAServer { get { return ConnectionState == ConnectionState.Connected; } }
        static SocketAddress socketAddress;
        static int serverTickRate;
        static double timer;
        static double clock;
        static Client client;
        static GameManager GameManager;
        static DebugManager DebugManager;
        static XmlManager XmlManager;
        static int port;

        static string playerNameOnConnection;
        static GameServer gameServer;

        static WelcomeEventArgs welcomeEventArgs;
        static int mapPacketCount = 0;
        static List<byte> mapData = new List<byte>();
        public static string  serverMap { get; private set; }

        public static int IDClient { get { return client.ID; } }
        public static bool IsServer { get; set; }

        public delegate void PacketReceivedHandler(NetworkEventArgs e); 
        public static event PacketReceivedHandler PacketReceived;

        static double elapsedTimeSinceLastUpdatePlayer;
        static double lastUpdate;


        public static void InitDependencies()
        {
            ConnectionState = ConnectionState.NotConnected;
            timer = 30;
            clock = 0d;
            port = 25789;

        }

        public static bool Connect(string ip, string name, bool isServer = false) //port is 25789 by default
        {
            if (isServer)
            {
                gameServer = new GameServer(port);
                gameServer.InitDependencies();
            }

            client = new Client(GameManager);
            playerNameOnConnection = name;

            if (ip.IndexOf(':') != -1)
            {
                if (!Int32.TryParse(ip.Split(':')[1], out port))
                {
                    MessageBoxIG.Show("Port unreadable.", "Error");
                    return false;
                }
            }
            if (ConnectionState == ConnectionState.NotConnected)
            {
                GameManager.Terminal.AddMessageToTerminal($"Connecting to {ip}...", "System", Color.White);
                socketAddress = new SocketAddress(ip, port);

                ConnectionState = ConnectionState.WaitingForServer;
                client.OnPacketReceived += OnPacketReceived;
                client.OnPacketSent += OnPacketSent;
                client.ConnectToServer(socketAddress);
                IsServer = isServer; 
                return true;
            }
            else
            {
                GameManager.Terminal.AddMessageToTerminal("You're already connected to a server.", "System", Color.Red);
                return false;
            }
        }

        public static void SendGameState(NetGameState netGameState)
        {
            client.SendMessage(netGameState, ClientPackets.UpdateGameState); 
        }

        public static void Disconnect()
        {
            mapPacketCount = 0;
            client.OnPacketReceived -= OnPacketReceived;
            client.OnPacketSent -= OnPacketSent;
            client.Disconnect();
            client.Dispose();
            client = null;
            if (IsServer)
            {
                gameServer.StopServer();
            }
            GC.Collect();
            ConnectionState = ConnectionState.NotConnected;
        }

        public static void SendMessageToServer(string message)
        {
            client.SendMessage(message, (int)ClientPackets.TcpChatMessage);
        }

        public static void RequestNameChange(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                client.RequestNameChange(name);
        }

        public static void OnMessage(ChatMessageEventArgs e)
        {
            GameManager.Terminal.AddMessageToTerminal(e.Text, e.Sender, e.TextColor);
        }

        public static void PlayerSync(PlayerSyncEventArgs e)
        {
            foreach (PlayerData pd in e.PlayerData)
            {
                var player = GameManager.CreateInstance(1) as Player;

                player.ID = pd.ID; 

                //Player playerDataSync = new Player(pd.ID, pd.Name);
                client.PlayersData.Add(pd.ID, player);
                GameManager.AddSprite(player);
            }
        }

        public static void OnRequestResponse(RequestResponseEventArgs e)
        {
            GameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
        }
        public static void OnPlayerDisconnection(PlayerDisconnectionEventArgs e)
        {
            if(e.ID == client.ID)
            {
                Disconnect(); 
                GameManager.StopGame();
                var msgb = MessageBoxIG.Show("You've disconnected by the server", "Error", MessageBoxIGType.Ok);

                msgb.Result += Msgb_Result;

                return; 
            }

            GameManager.RemoveSprite(client.PlayersData[e.ID]);
            client.PlayersData.Remove(e.ID);
            GameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
        }

        static void Msgb_Result(MessageBoxIG sender, MessageBoxIGResultEventArgs e)
        {
            GameManager.ToMainMenu(); 
        }

        public static void OnPlayerConnection(PlayerConnectEventArgs e)
        {
            //PlayerData playerDataCo = new PlayerData(e.ID, e.Name);
            //client.PlayersData.Add(e.ID, playerDataCo);
            //GameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
            //GameManager.AddSprite(playerDataCo, this);
        }

        public static void OnPlayerChangeName(PlayerChangeNameEventArgs e)
        {
            string exName = client.PlayersData[e.IDPlayer].Name;
            if (e.IDPlayer == client.ID)
            {
                GameManager.AddActionToUIThread(GameManager.Player.ChangeName, e.Name);
                return;
            }
            client.PlayersData[e.IDPlayer].ChangeName(e.Name);
            GameManager.Terminal.AddMessageToTerminal(exName + " is now known as " + e.Name, "System", Color.Green);
        }

        public static void OnWelcome(WelcomeEventArgs e)
        {
            welcomeEventArgs = e; 
            client.SendResponseWelcome(playerNameOnConnection, e.ID);
            serverTickRate = e.TickRate;
            ConnectionState = ConnectionState.Connected;
            if (IsServer)
                InitOnlineGame(); 
            //MapXml[] map = XmlManager.LoadMapFromString(e.Map);
            //Sprite[] sprites = XmlManager.GenerateSpritesFromXml(map);

            //foreach(var s in sprites)
            //    TextureManager.SaveTexture(s.Texture);

            
            //GetMapOfServer();

        }

        public static void InitOnlineGame()
        {
            Player player;
            if (IsServer)
                player = GameManager.GetSpriteById(welcomeEventArgs.ID) as Player;
            else
            {
                player = GameManager.CreateInstance(1) as Player;
                player.ID = welcomeEventArgs.ID;
            }
            player.ChangeName(playerNameOnConnection);
            player.input = new Input()
            {
                Down = Keys.S,
                Up = Keys.Z,
                Left = Keys.Q,
                Right = Keys.D,
                Run = Keys.LeftShift,
            };
            player.Position = Vector2.Zero;
            GameManager.AddSprite(player); 
        }

        static void GetMapOfServer()
        {
            serverMap = Encoding.UTF8.GetString(SendHttpRequest($"http://{socketAddress.IP}:8000/currentmap"));

            string mapDir= $"./maps/{serverMap}";
            string mapPath = $"./maps/{serverMap}.xml"; 

            if (!Directory.Exists(mapDir))
                Directory.CreateDirectory(mapDir);

            if(File.Exists(mapPath))
                File.Delete(mapPath);

            string mapXml = Encoding.UTF8.GetString(SendHttpRequest($"http://{socketAddress.IP}:8000/map/{serverMap}"));
            StreamWriter sw = File.CreateText(mapPath); 
            sw.Write(mapXml);
            sw.Close(); 

            string[] mapTextures = XmlManager.GetMapTextureNames(mapXml);
            DownloadMapTexture(serverMap, mapTextures);
        }

        static void DownloadMapTexture(string mapName, string[] textureNames)
        {
            foreach (var texture in textureNames) 
            {
                string texturePath = $"./maps/{mapName}/{texture}.png";
                if (File.Exists(texturePath))
                    File.Delete(texturePath); 

                byte[] data = SendHttpRequest($"http://{socketAddress.IP}:8000/texture/{mapName}/{texture}.png");
                MemoryStream stream = new MemoryStream(data);
                var textureStream = File.Create(texturePath); 
                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(stream);
                image.Save(textureStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                
                stream.Close();
                textureStream.Close();

                image.Dispose();
                stream.Dispose();
                textureStream.Dispose(); 
            }
        }

        static byte[] SendHttpRequest(string url)
        {
            HttpClient client = new HttpClient();
            var response = client.GetByteArrayAsync(url);
            response.Wait();
            return response.Result; 
        }

        public static void OnPacketReceived(NetworkEventArgs e)
        {
            switch (e)
            {
                case ChatMessageEventArgs cmea:
                    OnMessage(cmea);
                    break;
                case PlayerSyncEventArgs psea:
                    PlayerSync(psea);
                    break;
                case RequestResponseEventArgs rrea:
                    OnRequestResponse(rrea);
                    break;
                case PlayerDisconnectionEventArgs pdea:
                    OnPlayerDisconnection(pdea);
                    break;
                case PlayerConnectEventArgs pcea:
                    OnPlayerConnection(pcea);
                    break;
                case PlayerChangeNameEventArgs pcnea:
                    OnPlayerChangeName(pcnea);
                    break;
                case WelcomeEventArgs wea:
                    OnWelcome(wea);
                    break;
                case UpdateSelfEventArgs usea:
                    OnUpdateSelf(usea); 
                    break;
                case GameStateEventArgs gsea:
                    OnGameStateUpdate(gsea);
                    break;
                case MapEventArgs maea:
                    OnMapDataReceived(maea);
                    break;
                default:
                    if (e.PacketType == ServerPackets.UdpTest)
                    {
                        GameManager.Terminal.AddMessageToTerminal($"Connected !", "System", Color.Green);
                        ConnectionState = ConnectionState.Connected;
                        break;
                    }
                    GameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
                    break;
            }
            PacketReceived?.Invoke(e); 
        }

        static void OnMapDataReceived(MapEventArgs maea)
        {
            mapData.AddRange(maea.data); 
            mapPacketCount++;
            if (mapPacketCount > welcomeEventArgs.MapSize)
            {
                string text = Encoding.UTF8.GetString(mapData.ToArray());
                MapXml[] mapxml = XmlManager.ReadXml(text);
                foreach (var m in mapxml)
                {
                    GameManager.AddSprite(XmlManager.GenerateSpriteFromXml(m.node, m.mapName)); 
                }
                InitOnlineGame(); 
            }
        }

        public static void OnGameStateUpdate(GameStateEventArgs gsea)
        {
            if (GameManager.Player == null || (gsea.ID == GameManager.Player.ID && !gsea.GsForced))
                return;

            GameManager.UpdateSprite(gsea);
        }

        static void OnUpdateSelf(UpdateSelfEventArgs e)
        {
            if(e.PacketType == ServerPackets.Teleport)
                GameManager.Player.SetPosition(e.Position, false);
            if(e.PacketType == ServerPackets.ChangeHealth)
                GameManager.Player.Health = e.Health;
        }

        public static void OnPacketSent(NetworkEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Message))
            {
                GameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
            }
        }

        static void MoveObject(Packet packet)
        {
            int id = packet.ReadInt(); 
            Vector2 position = new Vector2(packet.ReadFloat(), packet.ReadFloat()); 
            Sprite s = GameManager.GetNetworkObject(id);
            if (s == null)
                return;
            s.Position = position; 
        }

        public static void Update(GameTime gameTime, NetGameState netGameState)
        {
            if (ConnectionState != ConnectionState.NotConnected)
            {
                if(IsServer)
                    gameServer.Update();

                client.PollEvents();
                if (ConnectionState == ConnectionState.Connected)
                {
                    if (clock > timer)
                    {
                        //client.SendMessage(GameManager.Player, (int)ClientPackets.UdpUpdatePlayer);
                        if (IsServer)
                        {
                            gameServer.SendGameStateToClients(netGameState);
                        }
                        if(!IsServer)
                            netGameState.SendGameState(client);
                        clock = 0d;
                        return;
                    }
                }
                clock += gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            netGameState.Clear();
        }
    }
    public enum ConnectionState {
        NotConnected, 
        Connected, 
        WaitingForServer,
        Disconnecting,
    }
}
