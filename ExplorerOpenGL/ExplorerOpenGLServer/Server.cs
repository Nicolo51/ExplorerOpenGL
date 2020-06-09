using ExplorerOpenGLInterfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;

namespace ExplorerOpenGLServer
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Server" à la fois dans le code et le fichier de configuration.
    public class Server : IServer
    {
        public List<Player> Players = new List<Player>();
        private int IDcount;

        public int Connect(IPlayer player)
        {
            IDcount++;
            Players.Add(new Player()
            {
                Direction = player.getDirection(),
                Position = player.getPosition(),
                ID = IDcount,
            }) ; 
            return IDcount;
        }

        public IPlayer[] GetPlayersInfo()
        {
            return Players.ToArray(); 
        }

        public void UpdatePlayer(IPlayer player, int ID)
        {
            for(int i = 0; i < Players.Count; i++)
            {
                if(Players[i].ID == ID)
                {
                    Players[i].Direction = player.getDirection();
                    Players[i].Position = player.getPosition(); 
                }
            }
        }
    }
}
