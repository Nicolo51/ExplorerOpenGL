using ExplorerOpenGL.Controlers.Networking;
using ExplorerOpenGL.Controlers.Networking.EventArgs;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers
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
        Controler controler; 

        public NetworkManager(Controler controler, Terminal Terminal)
        {
            client = new Client(controler); 
            terminal = Terminal; 
            this.controler = controler;
            IsConnectedToAServer = false;
            timer = 0d;
            clock = 0d; 
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
                controler.DebugManager.AddEvent(e.Message);
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
                    client.controler.Terminal.AddMessageToTerminal(cmea.Text, cmea.Sender, cmea.TextColor);
                    break;
                case PlayerSyncEventArgs psea:
                    foreach(PlayerData pd in psea.PlayerData)
                    {
                        PlayerData playerDataSync = new PlayerData(pd.ID, pd.Name);
                        client.PlayersData.Add(pd.ID, playerDataSync);
                        controler.AddSprite(playerDataSync);
                    }
                    break;
                case RequestResponseEventArgs rrea:
                    terminal.AddMessageToTerminal(rrea.Message, "System", Color.White);
                    break;
                case PlayerDisconnectionEventArgs pdea:
                    controler.RemoveSprite(client.PlayersData[pdea.ID]);
                    client.PlayersData.Remove(pdea.ID);
                    terminal.AddMessageToTerminal(pdea.Message, "System", Color.White);
                    break;
                case PlayerConnectEventArgs pcea:
                    PlayerData playerDataCo = new PlayerData(pcea.ID, pcea.Name);
                    client.PlayersData.Add(pcea.ID, playerDataCo);
                    terminal.AddMessageToTerminal(pcea.Message, "System", Color.White);
                    controler.AddSprite(playerDataCo);
                    break;
                case PlayerChangeNameEventArgs pcnea:
                    if(pcnea.IDPlayer == client.myId)
                    {
                        controler.AddActionToUIThread(controler.Player.ChangeName, pcnea.Name);
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
