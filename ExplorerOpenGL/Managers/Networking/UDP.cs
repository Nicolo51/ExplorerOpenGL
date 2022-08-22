using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking
{
    public class UDP : IDisposable
    {
        public UdpClient socket;
        public IPEndPoint endPoint;
        private SocketAddress socketAddress;
        private Client client;
        private bool IsClosed;

        public UDP(SocketAddress socketAddress, Client client)
        {
            IsClosed = false; 
            this.client = client; 
            this.socketAddress = socketAddress; 
            endPoint = new IPEndPoint(IPAddress.Parse(this.socketAddress.IP), socketAddress.Port);
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
                _packet.InsertInt(client.myId);
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
                if (IsClosed)
                    return; 
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

        internal void Close()
        {
            IsClosed = true; 
            socket.Client.Shutdown(SocketShutdown.Both);
            socket.Client.Close();
            socket.Close();
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
                client.packetHandlers[_packetId](_packet);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //get rid of managed resources
                socket.Dispose();
                socketAddress = null;
                client = null;
                endPoint = null; 
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
