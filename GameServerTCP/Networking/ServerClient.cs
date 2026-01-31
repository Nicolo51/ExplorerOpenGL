using GameServerTCP.GameData;
using LiteNetLib;
using Model.Network;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerTCP
{
    public class ServerClient
    {
        public int id;
        public NetPeer clientPeer; 

        public ServerClient(int id, NetPeer peer)
        {
            this.id = id;
            this.clientPeer = peer;
        }
    }
}
