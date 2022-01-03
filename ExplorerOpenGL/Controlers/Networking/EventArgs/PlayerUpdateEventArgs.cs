using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking.EventArgs
{
    public class PlayerUpdateEventArgs : NetworkEventArgs
    {
        public PlayerData[] PlayerData { get; set; }
    }
}
