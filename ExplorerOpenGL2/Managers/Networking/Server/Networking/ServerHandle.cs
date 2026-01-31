using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Managers.Networking.EventArgs;
using GameServerTCP;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;

namespace GameServerTCP
{
    public class ServerHandle
    {
        GameServer gameServer;
        ServerSend serverSend;

        public ServerHandle(GameServer gameServer, ServerSend serverSend)
        {
            this.gameServer = gameServer;
            this.serverSend = serverSend;
        }
        public void WelcomeReceived(int fromClient, NetPacketReader packet, NetPeer peer) 
        {
            int clientIdCheck = packet.GetInt();

            if (clientIdCheck != fromClient)
                throw new Exception("Data corruption...");

            string name = packet.GetString();
            string msg = packet.GetString();

            if(gameServer.ClientID != fromClient)
                gameServer.SendMap(fromClient); 

            gameServer.Log(peer.Address.ToString() + " successfully connected and is now connected as " + name + " with id : " + fromClient); 
        }

        public void UdpMessageReceived(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            int id = packet.GetInt(); 
            if(id == fromClient)
                gameServer.Log(fromClient + "-" + gameServer.GetPlayer(fromClient).Name + " : " + packet.GetString()); 
        }

        public void TcpCommandReceived(int idClient, NetPacketReader packet, NetPeer peer)
        {
            int clientIdCheck = packet.GetInt();
            string msg = packet.GetString();
            string[] cmd; 

            if (string.IsNullOrWhiteSpace(msg = msg.Trim()))
                return;
            if(msg[0] == '/')
            {
                cmd = msg.Split(' ');
                
                if(/*HasPriviliege(idClient)*/ true)
                {
                    switch (cmd[0].ToLower())
                    {
                        case "/tp": 
                            gameServer.Log("Tp command issued");
                            break;
                        case "/w":
                            gameServer.Log("Whisper command issued");
                            break;
                        default:
                            gameServer.Log("Unrecognized command");
                            break;
                    }
                }
                return; 
            }

            if (clientIdCheck != idClient)
            {
                gameServer.Log("L'id is corrupted " + clientIdCheck);
                return; 
            }
            gameServer.Log($"[{gameServer.GetPlayer(idClient).Name }, {idClient}]: { msg }");
            serverSend.TcpSpreadChatMessageToAll(gameServer.GetPlayer(idClient), msg); 
        }

        public void UdpUpdatePlayer(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            Vector2 position = new Vector2(packet.GetFloat(), packet.GetFloat());
            int health = packet.GetInt();
            string animationName = packet.GetString();
            int effect = packet.GetInt();

            string msg = $"Posistion = {position.ToString()}";
            //gameServer.UpdatePlayer(fromClient, position, health, animationName, effect);
            //ServerSend.UdpUpdatePlayers(fromClient);
            //gameServer.Log($"Received packet via UDP from ID { fromClient }. Contains message: {msg}");
            //Game.PrintDebug(); 
        }

        public void ChangeNameRequest(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            if(!checkIdIntegrity(fromClient, packet))
                serverSend.TcpChangeNameResult(fromClient, 403, string.Empty);

            string name = packet.GetString();
            gameServer.GetPlayer(fromClient).ChangeName(name);
            serverSend.TcpChangeNameResult(fromClient, 200, name);
        }

        public void Disconnect(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            //Thread.Sleep(2000); 
            if (!checkIdIntegrity(fromClient, packet))
                return;
            gameServer.CloseConnection(fromClient, peer);
        }

        public bool checkIdIntegrity(int fromClient, NetPacketReader packet)
        {
            int CheckID = packet.GetInt();
            return fromClient == CheckID; 
        }

        public void UpdateGameState(int fromClient, NetPacketReader packet, NetPeer peer)
        {
            gameServer.OnUpdateGameState(packet); 
        }
    }
}
