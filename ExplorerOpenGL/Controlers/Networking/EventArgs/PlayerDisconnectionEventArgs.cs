using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking.EventArgs
{
    public class PlayerDisconnectionEventArgs : NetworkEventArgs
    {
        public int ID { get; set; }
        public string Name{ get; set; }
    }
}
