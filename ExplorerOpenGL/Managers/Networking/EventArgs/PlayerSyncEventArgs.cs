using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking.EventArgs
{
    public class PlayerSyncEventArgs : NetworkEventArgs
    {
        public int PlayerSyncedCount { get; set; }
        public List<PlayerData> PlayerData { get; set; }

    }
}
