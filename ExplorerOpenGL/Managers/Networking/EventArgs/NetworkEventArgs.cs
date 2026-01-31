using LiteNetLib;
using LiteNetLib.Utils;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking.EventArgs
{
    public class NetworkEventArgs
    {
        public NetDataReader Packet { get; set; }
        public string Message { get; set; }
        public ServerPackets PacketType { get; set; }
        public NetworkEventArgs()
        {

        }
        public virtual void Read(NetPacketReader packet)
        {
            Packet = packet;
        }

        public virtual NetPacketReader Write()
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
