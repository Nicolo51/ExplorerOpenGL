using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.EventArgs
{
    public class ServerRequestEventArgs : NetworkEventArgs
    {
        public ServerRequestTypes ServerRequestType { get; set; }
        public ServerRequestEventArgs()
        {
            
        }
    }

}
