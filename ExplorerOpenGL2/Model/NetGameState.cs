using ExplorerOpenGL2.Managers.Networking;
using ExplorerOpenGL2.Model.Sprites;
using GameServerTCP;
using LiteNetLib;
using LiteNetLib.Utils;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Model
{
    public class NetGameState
    {
        List<NetDataWriter> netDataWriters; 

        public NetGameState()
        {
            netDataWriters = new List<NetDataWriter>();
        }

        public void Clear()
        {
            netDataWriters.Clear();
        }

        public NetDataWriter GetDataWriter()
        {
            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put((int)ClientPackets.UpdateGameState); 
            netDataWriters.Add(netDataWriter);
            return netDataWriter;
        }

        public void SendGameState(Client client)
        {
            foreach (var dw in netDataWriters)
            {
                client.SendMessage(dw, ClientPackets.UpdateGameState);
            }
        }

        

        public void SendGameState(ServerSend ss)
        {
            foreach (var dw in netDataWriters)
            {
                ss.SendGameState(dw);
            }
        }
    }
}
