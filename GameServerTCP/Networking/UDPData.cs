using SharedClasses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GameServerTCP
{
    public class UDPData
    {
        public IPEndPoint endPoint;

        private int id;

        public UDPData(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
            ServerSend.UDPTest(id);
        }

        public void SendData(Packet _packet)
        {
            GameServer.SendUDPData(endPoint, _packet);
        }

        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            using (Packet _packet = new Packet(_packetBytes))
            {
                int _packetId = _packet.ReadInt();
                GameServer.packetHandlers[_packetId](id, _packet);
            }
        }
    }
}
