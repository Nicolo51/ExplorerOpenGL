using ExplorerOpenGL.Managers.Networking;
using ExplorerOpenGL.Managers.Networking.EventArgs;
using ExplorerOpenGL.Managers.Networking.NetworkObject;
using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class NetworkManager 
    {
        public ConnectionState ConnectionState{ get; private set; }
        public bool IsConnectedToAServer { get { return ConnectionState == ConnectionState.Connected; } }
        SocketAddress socketAddress;
        int serverTickRate; 
        double timer;
        double clock;
        private Client client; 
        GameManager gameManager;
        DebugManager debugManager;
        private int port;
        private static NetworkManager instance;
        public static event EventHandler Initialized;
        private string playerNameOnConnection;
        private GameTime gameTime;

        public int IDClient { get { return client.myId; } }

        delegate void RequestHandler(Packet packet);
        private Dictionary<RequestTypes, RequestHandler> requestHandler; 

        double elapsedTimeSinceLastUpdatePlayer;
        double lastUpdate; 



        private bool DoOnce; 

        public static NetworkManager Instance { get
            {
                if(instance == null)
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
            DoOnce = false;
            requestHandler = new Dictionary<RequestTypes, RequestHandler>() 
            {
                {RequestTypes.DeleteObject, DeleteObject },
                {RequestTypes.MoveObject, MoveObject },
                {RequestTypes.CreateObject, CreateObject },
                {RequestTypes.MovePlayer, MovePlayer },
                {RequestTypes.ModifyPlayerHealth, ModifyPlayerHealth },
            }; 
        }

        public void InitDependencies()
        {
            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance;
        }

        public bool Connect(string ip, string name) //port is 25789 by default
        {
            client = new Client(gameManager);
            playerNameOnConnection = name; 

            if (ip.IndexOf(':') != -1)
            {
                if(!Int32.TryParse(ip.Split(':')[1], out port))
                {
                    MessageBox.Show("Port unreadable.", "Error");
                    return false; 
                } 
            }
            if (ConnectionState == ConnectionState.NotConnected)
            {
                gameManager.Terminal.AddMessageToTerminal($"Connecting to {ip}...", "System", Color.White);
                socketAddress = new SocketAddress(ip, port);
                client.Start(new SocketAddress(ip, port));

                ConnectionState = ConnectionState.WaitingForTcp;
                client.OnPacketReceived += OnPacketReceived; 
                client.OnPacketSent += OnPacketSent;
                client.ConnectToServer();
                return true;
            }
            else
            {
                gameManager.Terminal.AddMessageToTerminal("You're already connected to a server." , "System", Color.Red);
                return false; 
            }
        }
        public void Disconnect()
        {
            client.SendMessage(null, (int)ClientPackets.Disconnect); 
            client.Disconnect();
            client.Dispose();
            client = null; 
            GC.Collect(); 
            ConnectionState = ConnectionState.NotConnected;
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
            if(!string.IsNullOrWhiteSpace(name))
                client.RequestNameChange(name); 
        }
        public void CreateBullet(Player player)
        {
            client.CreateBullet(player); 
        }

        public void OnPlayerUpdate(PlayerUpdateEventArgs e)
        {
            elapsedTimeSinceLastUpdatePlayer = gameTime.TotalGameTime.TotalMilliseconds - lastUpdate;
            lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;
            foreach (PlayerData pd in e.PlayerData)
            {
                if (pd.ID == IDClient)
                    continue; 
                PlayerData p = client.PlayersData[pd.ID];
                p.SetPosition(pd.ServerPosition, false); 
                p.Health = pd.Health;
                p.Play(pd.CurrentAnimationName);
                p.Effects = pd.Effects; 
            }
        }

        public void OnGameObjectUpdate(GameObjectsUpdateEventArgs e)
        {
            foreach (var ngo in e.networkGameObjects)
            {
                bool contains = false;
                lock (gameManager.NetworkObjects)
                    contains = gameManager.NetworkObjects.Keys.Contains(ngo.ID);

                if (contains)
                {
                    gameManager.UpdateNetworkObjects(ngo);
                    continue;
                }
                var b = ngo as NetworkBullet;
                Bullet bullet = new Bullet() { Direction = b.Direction, Velocity = b.Velocity, ID = b.ID, Position = b.Position, IdPlayer = b.IDPlayer };
                gameManager.AddNetworkObject(bullet);
            }
        }


        public void OnMessage(ChatMessageEventArgs e)
        {
            gameManager.Terminal.AddMessageToTerminal(e.Text, e.Sender, e.TextColor);

        }
        public void PlayerSync(PlayerSyncEventArgs e)
        {
            foreach (PlayerData pd in e.PlayerData)
            {
                PlayerData playerDataSync = new PlayerData(pd.ID, pd.Name);
                client.PlayersData.Add(pd.ID, playerDataSync);
                gameManager.AddSprite(playerDataSync, this);
            }
        }
        public void OnRequestResponse(RequestResponseEventArgs e)
        {
            gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
        }
        public void OnPlayerDisconnection(PlayerDisconnectionEventArgs e)
        {
            gameManager.RemoveSprite(client.PlayersData[e.ID]);
            client.PlayersData.Remove(e.ID);
            gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
        }
        public void OnPlayerConnection(PlayerConnectEventArgs e)
        {
            PlayerData playerDataCo = new PlayerData(e.ID, e.Name);
            client.PlayersData.Add(e.ID, playerDataCo);
            gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
            gameManager.AddSprite(playerDataCo, this);
        }

        public void OnPlayerChangeName(PlayerChangeNameEventArgs e)
        {
            string exName = client.PlayersData[e.IDPlayer].Name;
            if (e.IDPlayer == client.myId)
            {
                gameManager.AddActionToUIThread(gameManager.Player.ChangeName, e.Name);
                return;
            }
            client.PlayersData[e.IDPlayer].Name = e.Name;
            gameManager.Terminal.AddMessageToTerminal(exName + " is now known as " + e.Name, "System", Color.Green);
        }

        public void OnWelcome(WelcomeEventArgs e)
        {
            client.SendResponseWelcome(playerNameOnConnection);
            //client.ConnectUdp();
            serverTickRate = e.TickRate;
            ConnectionState = ConnectionState.WaitingForUdp;
        }


        public void OnPacketReceived(NetworkEventArgs e)
        {
            switch (e)
            {
                case PlayerUpdateEventArgs puea:
                    OnPlayerUpdate(puea);
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
                case GameObjectsUpdateEventArgs gouea:
                    OnGameObjectUpdate(gouea);
                    break;
                case UpdateSelfEventArgs usea:
                    OnUpdateSelf(usea); 
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

        private void ModifyPlayerHealth(Packet packet)
        {
            int newHealth = packet.ReadInt(); 
            gameManager.Player.Health = newHealth; 
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
        private void DeleteObject(Packet packet)
        {
            int id = packet.ReadInt();
            gameManager.RemoveNetworkObjects(id); 
        }
        private void MovePlayer(Packet packet)
        {
            Vector2 pos = new Vector2(packet.ReadFloat(), packet.ReadFloat());
            gameManager.Player.SetPosition(pos, false);
        }

        private void CreateObject(Packet packet)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;
            DoOnce = true; 
            if (gameManager.Player == null || gameManager.Terminal == null)
            {
                return;
            }
            if (IsConnectedToAServer)
            {
                if (clock > timer)
                {
                    
                    client.SendMessage(gameManager.Player, (int)ClientPackets.UdpUpdatePlayer);
                    clock = 0d;
                    return;
                }
                clock += gameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }
    }
    public enum ConnectionState {
        NotConnected, 
        Connected, 
        WaitingForUdp, 
        WaitingForTcp, 
    }
}
