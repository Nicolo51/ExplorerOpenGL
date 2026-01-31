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
        public ConnectionState ConnectionState { get; private set; }
        public bool IsConnectedToAServer { get { return ConnectionState == ConnectionState.Connected; } }
        SocketAddress socketAddress;
        int serverTickRate;
        double timer;
        double clock;
        private Client client;
        GameManager gameManager;
        DebugManager debugManager;
        XmlManager xmlManager;
        private int port;
        private static NetworkManager instance;
        public static event EventHandler Initialized;
        private string playerNameOnConnection;
        private GameTime gameTime;
        GameServer gameServer;

        WelcomeEventArgs welcomeEventArgs;
        int mapPacketCount = 0;
        List<byte> mapData = new List<byte>();
        public string  serverMap { get; private set; }

        public int IDClient { get { return client.ID; } }
        public bool IsServer { get; set; }

        public delegate void PacketReceivedHandler(NetworkEventArgs e); 
        public event PacketReceivedHandler PacketReceived;
        
        double elapsedTimeSinceLastUpdatePlayer;
        double lastUpdate;

        public bool pingUdp { get; private set;  } = false; 
        public bool pingTcp { get; private set; } = false;

        public static NetworkManager Instance { get
            {
                if (instance == null)
                {
                    instance = new NetworkManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }

        private NetworkManager()
        {
            ConnectionState = ConnectionState.NotConnected;
            timer = 30;
            clock = 0d;
            port = 25789;
            InitDependencies(); 
        }

        public void InitDependencies()
        {
            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance;
            xmlManager = XmlManager.Instance;
            
        }

        public bool Connect(string ip, string name, bool isServer = false) //port is 25789 by default
        {
            if (isServer)
            {
                gameServer = new GameServer(port, this);
                gameServer.InitDependencies();
            }

            client = new Client(gameManager);
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
                gameManager.Terminal.AddMessageToTerminal($"Connecting to {ip}...", "System", Color.White);
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
                gameManager.Terminal.AddMessageToTerminal("You're already connected to a server.", "System", Color.Red);
                return false;
            }
        }

        public void SendGameState(NetGameState netGameState)
        {
            client.SendMessage(netGameState, ClientPackets.UpdateGameState); 
        }

        public void Disconnect()
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

        public Task ChangeMap(string mapName, string host)
        {
            Task t = new Task(() =>
            {
                HttpClient client = new HttpClient();
                StringContent content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        MapName = mapName,
                    }
                    ));
                if (string.IsNullOrWhiteSpace(host))
                    host = "http://localhost:8000";
                Task<HttpResponseMessage> task = client.PostAsync($"{host}/changemap", content);

                while (!task.IsCompleted)
                    Thread.Sleep(1);

            }); 
            t.Start();
            return t;
        }

        public Task UploadMap(string mapName, string path, string host, UploadScreen view)
        {
            Task t = new Task(() => {
                string filename = mapName + ".xml";
                view.UploadCompletion = 0;
                string[] imagesPath = Directory.GetFiles($"./maps/{mapName}");
                double completionStep = 100d / (imagesPath.Length + 1);

                HttpClient httpClient = new HttpClient();
                if (string.IsNullOrWhiteSpace(host))
                    host = "http://localhost:8000";
                //Send Xml
                string xml = File.ReadAllText($"./maps/{filename}");
                StringContent content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        Xml = xml,
                        FileName = filename,
                    }
                    ));
                Task<HttpResponseMessage> task = httpClient.PostAsync($"{host}/MapXml", content);

                while (!task.IsCompleted)
                    Thread.Sleep(1);
                if (task.Result.Content.ReadAsStringAsync().Result == "0")
                    throw new Exception("Failed to upload xml");

                view.UploadCompletion += completionStep;

                //Send Texture;


                foreach (string file in imagesPath)
                {
                    byte[] imageArray = System.IO.File.ReadAllBytes(file);
                    string base64Image = Convert.ToBase64String(imageArray);
                    StringContent imageContent = new StringContent(JsonSerializer.Serialize(
                        new
                        {
                            image = base64Image,
                            textureName = Path.GetFileName(file),
                            mapName = mapName,
                        }
                        ));
                    Task<HttpResponseMessage> imageTask = httpClient.PostAsync($"{host}/MapTexture", imageContent);

                    while (!imageTask.IsCompleted)
                        Thread.Sleep(1);
                    if (task.Result.Content.ReadAsStringAsync().Result == "0")
                        throw new Exception("Failed to upload image");
                    view.UploadCompletion += completionStep;
                }
                view.UploadCompletion = 100;
                return;
            });
            t.Start();
            return t;
        }

        public void SendMessageToServer(string message)
        {
            client.SendMessage(message, (int)ClientPackets.TcpChatMessage);
        }

        public void RequestNameChange(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                client.RequestNameChange(name);
        }
        public void CreateBullet(Player player)
        {
            client.CreateBullet(player);
        }

        public void OnMessage(ChatMessageEventArgs e)
        {
            gameManager.Terminal.AddMessageToTerminal(e.Text, e.Sender, e.TextColor);
        }

        public void PlayerSync(PlayerSyncEventArgs e)
        {
            foreach (PlayerData pd in e.PlayerData)
            {
                var player = gameManager.CreateInstance(1) as Player;

                player.ID = pd.ID; 

                //Player playerDataSync = new Player(pd.ID, pd.Name);
                client.PlayersData.Add(pd.ID, player);
                gameManager.AddSprite(player, this);
            }
        }

        public void OnRequestResponse(RequestResponseEventArgs e)
        {
            gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
        }
        public void OnPlayerDisconnection(PlayerDisconnectionEventArgs e)
        {
            if(e.ID == client.ID)
            {
                Disconnect(); 
                gameManager.StopGame();
                return; 
            }

            gameManager.RemoveSprite(client.PlayersData[e.ID]);
            client.PlayersData.Remove(e.ID);
            gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
        }
        public void OnPlayerConnection(PlayerConnectEventArgs e)
        {
            //PlayerData playerDataCo = new PlayerData(e.ID, e.Name);
            //client.PlayersData.Add(e.ID, playerDataCo);
            //gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
            //gameManager.AddSprite(playerDataCo, this);
        }

        public void OnPlayerChangeName(PlayerChangeNameEventArgs e)
        {
            string exName = client.PlayersData[e.IDPlayer].Name;
            if (e.IDPlayer == client.ID)
            {
                gameManager.AddActionToUIThread(gameManager.Player.ChangeName, e.Name);
                return;
            }
            client.PlayersData[e.IDPlayer].ChangeName(e.Name);
            gameManager.Terminal.AddMessageToTerminal(exName + " is now known as " + e.Name, "System", Color.Green);
        }

        public void OnWelcome(WelcomeEventArgs e)
        {
            welcomeEventArgs = e; 
            client.SendResponseWelcome(playerNameOnConnection, e.ID);
            serverTickRate = e.TickRate;
            ConnectionState = ConnectionState.Connected;
            if (IsServer)
                InitOnlineGame(); 
            //MapXml[] map = xmlManager.LoadMapFromString(e.Map);
            //Sprite[] sprites = xmlManager.GenerateSpritesFromXml(map);

            //foreach(var s in sprites)
            //    TextureManager.Instance.SaveTexture(s.Texture);

            
            //GetMapOfServer();

        }

        public void InitOnlineGame()
        {
            Player player;
            if (IsServer)
                player = gameManager.GetSpriteById(welcomeEventArgs.ID) as Player;
            else
            {
                player = gameManager.CreateInstance(1) as Player;
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
            gameManager.AddSprite(player, this); 
        }

        private void GetMapOfServer()
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

            string[] mapTextures = xmlManager.GetMapTextureNames(mapXml);
            DownloadMapTexture(serverMap, mapTextures);
        }

        private void DownloadMapTexture(string mapName, string[] textureNames)
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

        private byte[] SendHttpRequest(string url)
        {
            HttpClient client = new HttpClient();
            var response = client.GetByteArrayAsync(url);
            response.Wait();
            return response.Result; 
        }

        public void OnPacketReceived(NetworkEventArgs e)
        {
            switch (e)
            {
                case PongEventArgs pea:
                    OnPong(pea); 
                    break;
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
                        gameManager.Terminal.AddMessageToTerminal($"Connected !", "System", Color.Green);
                        ConnectionState = ConnectionState.Connected;
                        break;
                    }
                    gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
                    break;
            }
            PacketReceived?.Invoke(e); 
        }

        private void OnMapDataReceived(MapEventArgs maea)
        {
            mapData.AddRange(maea.data); 
            mapPacketCount++;
            if (mapPacketCount > welcomeEventArgs.MapSize)
            {
                string text = Encoding.UTF8.GetString(mapData.ToArray());
                MapXml[] mapxml = xmlManager.ReadXml(text);
                foreach (var m in mapxml)
                {
                    gameManager.AddSprite(xmlManager.GenerateSpriteFromXml(m.node, m.mapName), this); 
                }
                InitOnlineGame(); 
            }
        }

        public void OnGameStateUpdate(GameStateEventArgs gsea)
        {
            if (gameManager.Player == null || (gsea.ID == gameManager.Player.ID && !gsea.GsForced))
                return; 

            gameManager.UpdateSprite(gsea);
        }

        private void OnPong(PongEventArgs pea)
        {
            if (pea.Type == PongType.Udp)
                pingUdp = true;
            if(pea.Type == PongType.Tcp)
                pingTcp = true;
        }

        private void OnUpdateSelf(UpdateSelfEventArgs e)
        {
            if(e.PacketType == ServerPackets.Teleport)
                gameManager.Player.SetPosition(e.Position, false);
            if(e.PacketType == ServerPackets.ChangeHealth)
                gameManager.Player.Health = e.Health;
        }

        public void OnPacketSent(NetworkEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Message))
            {
                gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
            }
        }

        private void MoveObject(Packet packet)
        {
            int id = packet.ReadInt(); 
            Vector2 position = new Vector2(packet.ReadFloat(), packet.ReadFloat()); 
            Sprite s = gameManager.GetNetworkObject(id);
            if (s == null)
                return;
            s.Position = position; 
        }

        public void Update(GameTime gameTime, NetGameState netGameState)
        {
            this.gameTime = gameTime;
            if (ConnectionState != ConnectionState.NotConnected)
            {
                if(IsServer)
                    gameServer.Update();

                client.PollEvents();
                if (ConnectionState == ConnectionState.Connected)
                {
                    if (clock > timer)
                    {
                        //client.SendMessage(gameManager.Player, (int)ClientPackets.UdpUpdatePlayer);
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
