using SharedClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking
{
    public class TCP : IDisposable
    {
        public TcpClient socket;
        public int dataBufferSize = 4096;
        private Packet receiveData;

        private NetworkStream stream;
        private byte[] receiveBuffer;
        private SocketAddress socketAddress;
        private Client client;
        private bool IsClose; 

       
        public TCP(SocketAddress socketAddress, Client client)
        {
            IsClose = false; 
            this.client = client; 
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

        internal void Close()
        {
            IsClose = true; 
            socket.Client.Shutdown(SocketShutdown.Both);
            socket.Client.Close(); 
            socket.Close(); 
        }

        private void onReceive(IAsyncResult ar)
        {
            try
            {
                if (IsClose)
                    return; 
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
                    Debug.WriteLine(packetId);
                    client.packetHandlers[packetId](packet);
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


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //get rid of managed resources
                stream.Dispose();
                socket.Dispose();
                receiveData.Dispose();
                receiveBuffer = null;
                socketAddress = null;
                client = null;
            }
            //get rid of unmanaged resources
            //nothing for now
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }
    }
}
