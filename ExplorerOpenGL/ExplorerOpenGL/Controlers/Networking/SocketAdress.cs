using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class SocketAddress
    {
        public string IP { get; private set; }
        public int Port { get; private set; }

        public SocketAddress(string ip, int port)
        {
            IP = ip;
            Port = port; 
        }

    }
}
