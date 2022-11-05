using GameServerTCP;
using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("What's your name?");
            //Client.name = Console.ReadLine();
            //Client.Start();
            //Console.WriteLine("Client started");
            //Client.ConnectToServer();
            //Console.WriteLine("Client Connected");

            //string input;
            //Console.Write(">");
            //while ((input = Console.ReadLine()) != "exit")
            //{
            //    string[] query = input.Split(" "); 
            //    if(query[0].ToLower() == "udp")
            //    {
            //        Client.SendMessage(input, (int)ClientPackets.UdpMessage); 
            //    }
            //    else if (query[0].ToLower() == "tcp")
            //    {
            //        Client.SendMessage(input, (int)ClientPackets.TcpIssuedCommand);
            //    }
            //    else
            //    {
            //        Client.SendMessage(input, (int)ClientPackets.TcpIssuedCommand);
            //    }
            //    Console.Write(">");
            A c = new C();
            TestA(c);
        }
        public static void TestA<T>(T a)
        {
        }
        public static void TestA(C c)
        {
            Console.WriteLine("c");
        }
        public static void TestA(B b)
        {
            Console.WriteLine("b");
        }

        public class A
        {

        }
        public class B : A
        {

        }
        public class C : A
        {

        }
    }
}
