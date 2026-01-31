using LiteNetLib.Utils;
using Model.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerTCP.GameData.GameObjects
{
    public class GameObject
    {
        public Vector2 Position;
        public int ID;
        public bool IsRemove { get; private set; }
        public int RemoveTick; 
        public GameObject()
        {
            IsRemove = false; 
            ID = Game.RequestNewGameObjectID(); 
        }
        public virtual void Update(TimeSpan lastUpdate, Player[] players, GameObject[] gameObjects)
        {

        }

        public virtual void Remove()
        {
            RemoveTick++; 
            IsRemove = true; 
        }

        public virtual void WriteIntoPacket(NetDataWriter packet)
        {
            packet.Put(this.GetType().Name);
            packet.Put(ID);
            packet.Put(IsRemove);
            packet.Put(Position.X); 
            packet.Put(Position.Y);
        }
    }
}
