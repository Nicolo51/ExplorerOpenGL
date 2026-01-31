using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking.EventArgs
{
    public class PongEventArgs :NetworkEventArgs
    {
        public double gameTime { get; set; }
        public PongType Type { get; set; }
        public PongEventArgs() { }
    }
    public enum PongType
    {
        Udp,
        Tcp, 
    }
}
