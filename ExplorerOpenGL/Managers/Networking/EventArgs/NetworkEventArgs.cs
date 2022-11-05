using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.EventArgs
{
    public class NetworkEventArgs
    {
        public Packet Packet { get; set; }
        public string Message { get; set; }
        public ServerPackets PacketType { get; set; }
        public NetworkEventArgs()
        {

        }
        public virtual void Read(Packet packet)
        {
            Packet = packet;
        }

        public virtual Packet Write()
        {
            return default; 
        }
    }

    public enum Protocol : int
    {
        TCP = 0, 
        UDP = 1, 
    }

    
}
