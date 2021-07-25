using GameServerTCP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class UDPData
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDPData()
        {
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25789);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(Client.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    // TODO: disconnect
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                // TODO: disconnect
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }
            using (Packet _packet = new Packet(_data))
            {
                int _packetId = _packet.ReadInt();
                Client.packetHandlers[_packetId](_packet);
            }
    }

    
}
}
