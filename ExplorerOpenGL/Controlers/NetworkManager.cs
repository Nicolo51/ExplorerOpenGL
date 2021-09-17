using ExplorerOpenGL.Controlers.Networking;
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
                controler.DebugManager.AddEvent($"Connecting to {ip}");
                socketAddress = new SocketAddress(ip, 25789);
                client.Start(new SocketAddress(ip, 25789));
                client.ConnectToServer();
                IsConnectedToAServer = true;
                controler.DebugManager.AddEvent("Connected !");
            }
            else
            {
                controler.DebugManager.AddEvent("You're already connected to a server.");
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

        public void Update(GameTime gametime, Player player)
        {
            if(clock > timer)
            {
                client.SendMessage(player, (int)ClientPackets.UdpUpdatePlayer); 
                clock = 0d;
                return; 
            }
            clock += gametime.ElapsedGameTime.TotalMilliseconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsConnectedToAServer)
                return; 
            foreach(KeyValuePair<int, PlayerData> entry in client.PlayersData)
            {
                if(entry.Value.playerFeetTexture == null || entry.Value.playerTexture == null)
                {
                    entry.Value.playerFeetTexture = controler.TextureManager.LoadedTextures["playerfeet"]; 
                    entry.Value.playerTexture = controler.TextureManager.LoadedTextures["player"];
                }
                if (client.myId == entry.Key)
                    continue; 
                entry.Value.Draw(spriteBatch); 
            }
        }
    }
}
