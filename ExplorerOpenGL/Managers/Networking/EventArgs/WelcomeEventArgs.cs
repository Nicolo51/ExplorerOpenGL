using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.EventArgs
{
    public class WelcomeEventArgs : NetworkEventArgs
    {
        public int ID { get; set; }
        public int TickRate { get; set; }
        public PlayerData PlayerData { get; set; }

    }
}
