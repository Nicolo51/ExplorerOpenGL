using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking.EventArgs
{
    public class PlayerConnectEventArgs : NetworkEventArgs
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
