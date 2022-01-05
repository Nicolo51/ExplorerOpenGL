using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerTCP
{
    public class Client
    {
        public int id;
        public TCPData tcp;
        public UDPData udp; 

        public Client(int id)
        {
            this.id = id;
            tcp = new TCPData(id);
            udp = new UDPData(id);
            tcp.Disconnection += OnDisconnection; 
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (tcp.socket == null)
                    return;
                tcp.stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null); 
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OnDisconnection()
        {
            tcp = new TCPData(id);
            udp = new UDPData(id);
            GameData.Game.RemovePlayer(id); 
        }
    }
}
