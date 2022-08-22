using Microsoft.Xna.Framework;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.NetworkObject
{
    public class NetworkGameObject: NetworkObject
    {
        public Vector2 Position { get; set; }
        public float Direction { get; set; }

        public override void ReadPacket(Packet packet)
        {
            base.ReadPacket(packet);
            Position = new Vector2(packet.ReadFloat(), packet.ReadFloat()); 
            Direction = packet.ReadFloat();
        }
    }
}
