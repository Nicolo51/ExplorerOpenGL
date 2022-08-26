using ExplorerOpenGL.Managers.Networking;
using ExplorerOpenGL.Managers.Networking.EventArgs;
using ExplorerOpenGL.Managers.Networking.NetworkObject;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    MessageBox.Show("Error", "Port unreadable.");
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

        public void OnPacketReceived(NetworkEventArgs e)
        {
            switch (e)
            {
                case PlayerUpdateEventArgs puea:
                    elapsedTimeSinceLastUpdatePlayer = gameTime.TotalGameTime.TotalMilliseconds - lastUpdate;
                    lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    foreach(PlayerData pd in puea.PlayerData)
                    {
                        client.PlayersData[pd.ID].ServerPosition = pd.ServerPosition; 
                        client.PlayersData[pd.ID].LookAtRadian = pd.LookAtRadian; 
                        client.PlayersData[pd.ID].FeetRadian = pd.FeetRadian;
                        client.PlayersData[pd.ID].SetTimeToTravel(elapsedTimeSinceLastUpdatePlayer, gameTime);
                        client.PlayersData[pd.ID].Health = pd.Health;
                    }
                    break;
                case ChatMessageEventArgs cmea:
                    gameManager.Terminal.AddMessageToTerminal(cmea.Text, cmea.Sender, cmea.TextColor);
                    break;
                case PlayerSyncEventArgs psea:
                    foreach(PlayerData pd in psea.PlayerData)
                    {
                        PlayerData playerDataSync = new PlayerData(pd.ID, pd.Name);
                        client.PlayersData.Add(pd.ID, playerDataSync);
                        gameManager.AddSprite(playerDataSync, this);
                    }
                    break;
                case RequestResponseEventArgs rrea:
                    gameManager.Terminal.AddMessageToTerminal(rrea.Message, "System", Color.White);
                    break;
                case PlayerDisconnectionEventArgs pdea:
                    gameManager.RemoveSprite(client.PlayersData[pdea.ID]);
                    client.PlayersData.Remove(pdea.ID);
                    gameManager.Terminal.AddMessageToTerminal(pdea.Message, "System", Color.White);
                    break;
                case PlayerConnectEventArgs pcea:
                    PlayerData playerDataCo = new PlayerData(pcea.ID, pcea.Name);
                    client.PlayersData.Add(pcea.ID, playerDataCo);
                    gameManager.Terminal.AddMessageToTerminal(pcea.Message, "System", Color.White);
                    gameManager.AddSprite(playerDataCo, this);
                    break;
                case PlayerChangeNameEventArgs pcnea:
                    string exName = client.PlayersData[pcnea.IDPlayer].Name; 
                    if (pcnea.IDPlayer == client.myId)
                    {
                        gameManager.AddActionToUIThread(gameManager.Player.ChangeName, pcnea.Name);
                        return; 
                    }
                    client.PlayersData[pcnea.IDPlayer].Name = pcnea.Name;
                    gameManager.Terminal.AddMessageToTerminal(exName + " is now known as " + pcnea.Name, "System", Color.Green);
                    break;
                case WelcomeEventArgs wea:
                    client.SendResponseWelcome(playerNameOnConnection);
                    //client.ConnectUdp();
                    serverTickRate = wea.TickRate; 
                    ConnectionState = ConnectionState.WaitingForUdp;
                    break;
                case GameObjectsUpdateEventArgs gouea:
                    foreach (var ngo in gouea.networkGameObjects)
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
                    break;
                case RequestEventArgs srea:
                    requestHandler[srea.RequestType](srea.Packet);
                    break; 
                default:
                    if (e.MessageType == MessageType.OnUdpTestReceive)
                    {
                        gameManager.Terminal.AddMessageToTerminal($"Connected !", "System", Color.Green);
                        ConnectionState = ConnectionState.Connected;
                        break;
                    }
                    gameManager.Terminal.AddMessageToTerminal(e.Message, "System", Color.White);
                    break;
            }
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
