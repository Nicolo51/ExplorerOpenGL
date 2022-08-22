using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.NetworkObject
{
    public class NetworkObject
    {
        public string RefType { get; set; }
        public int ID { get; set; }
        public bool IsRemove { get; set; }
        public virtual void ReadPacket(Packet packet)
        {
            RefType = packet.ReadString();
            ID = packet.ReadInt();
            IsRemove = packet.ReadBool(); 
        }
    }
}
