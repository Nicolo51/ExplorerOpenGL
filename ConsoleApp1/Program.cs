using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Client
{
    class Program
    {
        static EventBasedNetListener listener;
        static NetManager client; 
        public static void Main(string[] args)
        {
            startClient();
        }

        private static void startClient()
        {
            listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();
            client.Connect("localhost" /* host ip or name */, 25789 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("Un client s'est co");
            };

            while (!Console.KeyAvailable)
            {
                client.PollEvents();
                Thread.Sleep(15);
                SendData("coucou", client.FirstPeer);
            }

            client.Stop();
        }

        private static void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine("We got: {0}", reader.GetString(100 /* max length of string */));
            reader.Recycle();
            
        }

        public static void SendData(string data, NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Reset(); 
            writer.Put(data);
            peer.Send(writer, DeliveryMethod.ReliableSequenced); 
        }
    }
}