using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking.EventArgs
{
    public class GameStateEventArgs : NetworkEventArgs
    {
        public int ID { get; set; }
        public int Type { get; set; }
        public bool GsForced { get; set; }
    }
}
