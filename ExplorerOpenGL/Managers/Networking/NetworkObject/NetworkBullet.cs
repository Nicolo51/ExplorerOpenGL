using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.NetworkObject
{
    public class NetworkBullet : NetworkGameObject
    {
        public float Velocity { get; set; }
        public int IDPlayer { get; set; }
        public override void ReadPacket(Packet packet)
        {
            base.ReadPacket(packet);
            Velocity = packet.ReadFloat();
            IDPlayer = packet.ReadInt(); 
        }
    }
}
