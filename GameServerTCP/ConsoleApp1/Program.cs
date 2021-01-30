using GameServerTCP;
using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("What's your name?");
            Client.name = Console.ReadLine();
            Client.Start();
            Console.WriteLine("Client started");
            Client.ConnectToServer();
            Console.WriteLine("Client Connected");
            
            string input;
            Console.Write(">");
            while ((input = Console.ReadLine()) != "exit")
            {
                string[] query = input.Split(" "); 
                if(query[0].ToLower() == "udp")
                {
                    Client.SendMessage(input, (int)ClientPackets.UdpMessageRecieved); 
                }
                if (query[0].ToLower() == "tcp")
                {
                    Client.SendMessage(input, (int)ClientPackets.TcpIssuedCommand);
                }
                else
                {
                    Client.SendMessage(input, (int)ClientPackets.TcpIssuedCommand);
                }
                Console.Write(">");
            } 

        }
    }
}
