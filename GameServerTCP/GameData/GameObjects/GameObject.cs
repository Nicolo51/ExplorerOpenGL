using SharedClasses;
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

        public virtual void WriteIntoPacket(Packet packet)
        {
            packet.Write(this.GetType().Name);
            packet.Write(ID);
            packet.Write(IsRemove);
            packet.Write(Position.X); 
            packet.Write(Position.Y);
        }
    }
}
