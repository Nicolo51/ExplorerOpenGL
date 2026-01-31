using LiteNetLib;
using Model.Network;
using System.Security.Cryptography;

namespace ExplorerOpenGL2.Managers.Networking.EventArgs
{
    internal class MapEventArgs : NetworkEventArgs
    {
        public NetPacketReader Packet { get; set; }
        public ServerPackets PacketType { get; set; }
        public byte[] data { get; set; }
    }
}