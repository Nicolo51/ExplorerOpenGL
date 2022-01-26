using ExplorerOpenGL.Managers.Networking;
using ExplorerOpenGL.Managers.Networking.EventArgs;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class NetworkManager 
    {
        public bool IsConnectedToAServer { get; private set; }
        SocketAddress socketAddress;
        int serverTickRate; 
        double timer;
        double clock;
        Terminal terminal;
        private Client client; 
        GameManager gameManager;
        DebugManager debugManager;

        private static NetworkManager instance;
        public static event EventHandler Initialized;

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

        public NetworkManager()
        {
            IsConnectedToAServer = false;
            timer = 0d;
            clock = 0d; 
        }

        public void InitDependencies()
        {
            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance;
            client = new Client(gameManager);
        }

        public void Connect(string ip) //port is 25789
        {
            if (!IsConnectedToAServer)
            {
                terminal.AddMessageToTerminal($"Connecting to {ip}...", "System", Color.White);
                socketAddress = new SocketAddress(ip, 25789);
                client.Start(new SocketAddress(ip, 25789));
                client.ConnectToServer();
                IsConnectedToAServer = true;
                client.OnPacketReceived += OnPacketReceived; 
                client.OnPacketSent += OnPacketSent;
                terminal.AddMessageToTerminal($"Connected !", "System", Color.Green);
            }
            else
            {
                terminal.AddMessageToTerminal("You're already connected to a server." , "System", Color.Red);
            }
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

        public void OnPacketReceived(NetworkEventArgs e)
        {
            if(!(e is PlayerUpdateEventArgs))
                debugManager.AddEvent(e.Message);
            switch (e)
            {
                case PlayerUpdateEventArgs puea:
                    foreach(PlayerData pd in puea.PlayerData)
                    {
                        client.PlayersData[pd.ID].ServerPosition = pd.ServerPosition; 
                        client.PlayersData[pd.ID].LookAtRadian = pd.LookAtRadian; 
                        client.PlayersData[pd.ID].FeetRadian = pd.FeetRadian;
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
                    terminal.AddMessageToTerminal(rrea.Message, "System", Color.White);
                    break;
                case PlayerDisconnectionEventArgs pdea:
                    gameManager.RemoveSprite(client.PlayersData[pdea.ID]);
                    client.PlayersData.Remove(pdea.ID);
                    terminal.AddMessageToTerminal(pdea.Message, "System", Color.White);
                    break;
                case PlayerConnectEventArgs pcea:
                    PlayerData playerDataCo = new PlayerData(pcea.ID, pcea.Name);
                    client.PlayersData.Add(pcea.ID, playerDataCo);
                    terminal.AddMessageToTerminal(pcea.Message, "System", Color.White);
                    gameManager.AddSprite(playerDataCo, this);
                    break;
                case PlayerChangeNameEventArgs pcnea:
                    if(pcnea.IDPlayer == client.myId)
                    {
                        gameManager.AddActionToUIThread(gameManager.Player.ChangeName, pcnea.Name);
                        return; 
                    }
                    client.PlayersData[pcnea.IDPlayer].Name = pcnea.Name;
                    break; 
                default:
                    terminal.AddMessageToTerminal(e.Message, "System", Color.White);
                    break; 
            }
        }

        

        public void OnPacketSent(NetworkEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Message))
            {
                terminal.AddMessageToTerminal(e.Message, "System", Color.White);
            }
        }

        public void Update(GameTime gametime, Player player)
        {
            if (clock > timer)
            {
                client.SendMessage(player, (int)ClientPackets.UdpUpdatePlayer); 
                clock = 0d;
                return; 
            }
            clock += gametime.ElapsedGameTime.TotalMilliseconds;
        }
    }
}
