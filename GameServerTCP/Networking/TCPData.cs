using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GameServerTCP
{
    public class TCPData
    {
        public TcpClient socket;
        private int id;
        private int dataBufferSize = 4096;
        private byte[] receivebuffer;
        public NetworkStream stream;
        private Packet receivedData;

        public delegate void DisconnectionEventHandler();
        public event DisconnectionEventHandler Disconnection; 

        public TCPData(int id)
        {
            this.id = id; 
        }

        public void Connect(TcpClient socket)
        {
            this.socket = socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;
            receivedData = new Packet(); 
            stream = socket.GetStream();
            receivebuffer = new byte[dataBufferSize];
            stream.BeginRead(receivebuffer, 0, dataBufferSize, OnReceive, null);
            ServerSend.Welcome(id);
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int bytesLength = stream.EndRead(ar);
                if (bytesLength <= 0)
                    return;

                byte[] data = new byte[bytesLength];
                Array.Copy(receivebuffer, data, bytesLength);
                receivedData.Reset(HandlleData(data)); 
                stream.BeginRead(receivebuffer, 0, dataBufferSize, OnReceive, null);
            }
            catch (Exception e)
            {
                //socket.Close();
                if(e.Source == "System.Net.Sockets")
                {
                    socket.Close();
                    stream.Dispose();
                    socket.Dispose();
                    Disconnection?.Invoke();
                    return; 
                }
                Console.WriteLine("lala");
                Console.WriteLine(e.Message);
            }
        }
        private bool HandlleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }
            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    GameServer.packetHandlers[packetId](id, packet);
                }
            }
            packetLength = 0;

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            if (packetLength <= 1)
            {
                return true;
            }
            return false;
        }
    }
}
