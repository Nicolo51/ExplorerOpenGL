using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking.EventArgs
{
    public class PlayerSyncEventArgs : NetworkEventArgs
    {
        public int PlayerSyncedCount { get; set; }
        public Dictionary<int, PlayerData> PlayerData { get; set; }

    }
}
