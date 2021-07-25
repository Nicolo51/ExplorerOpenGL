﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class TCP
    {
        public TcpClient socket;
        public int dataBufferSize = 4096;
        private Packet receiveData;

        private NetworkStream stream;
        private byte[] receiveBuffer;
        private SocketAddress socketAddress; 
        
        public TCP(SocketAddress socketAddress)
        {
            this.socketAddress = socketAddress; 
        }
        public void Connect()
        {
            socket = new TcpClient();
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(socketAddress.IP, socketAddress.Port, onConnect, socket);
        }
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void onConnect(IAsyncResult ar)
        {
            socket.EndConnect(ar);

            if (!socket.Connected)
                return;

            stream = socket.GetStream();

            receiveData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, onReceive, null);
        }

        private void onReceive(IAsyncResult ar)
        {
            try
            {
                int bytesLength = stream.EndRead(ar);
                if (bytesLength <= 0)
                    return;

                byte[] data = new byte[bytesLength];
                Array.Copy(receiveBuffer, data, bytesLength);

                receiveData.Reset(HandlleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, onReceive, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private bool HandlleData(byte[] data)
        {
            int packetLength = 0;

            receiveData.SetBytes(data);

            if (receiveData.UnreadLength() >= 4)
            {
                packetLength = receiveData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }
            while (packetLength > 0 && packetLength <= receiveData.UnreadLength())
            {
                byte[] packetBytes = receiveData.ReadBytes(packetLength);
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Client.packetHandlers[packetId](packet);
                }
            }
            packetLength = 0;

            if (receiveData.UnreadLength() >= 4)
            {
                packetLength = receiveData.ReadInt();
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
