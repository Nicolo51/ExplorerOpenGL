using SharedClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerTCP.GameData.GameObjects
{
    public class Bullet : GameObject
    {
        public float Velocity;
        public float Direction;
        public Player Owner;
        public float Range;
        private Vector2 initialPosition; 
        public Bullet(Vector2 position)
        {
            Velocity = 4;
            Position = position;
            initialPosition = position; 
            Range = 400; 
        }

        public override void Update(TimeSpan lastUpdate, Player[] players, GameObject[] gameObjects)
        {
            Position += new Vector2(Direction) * 10;
            if(Vector2.Distance(initialPosition, Position) > Range)
            {
                Remove(); 
            }
            CheckCollision(players, gameObjects); 
            base.Update(lastUpdate, players, gameObjects);
        }

        public void CheckCollision(Player[] players, GameObject[] gameObjects)
        {
            foreach(var g in gameObjects)
            {
                if (g == this || g.IsRemove || g is Bullet)
                    continue;
                if (Vector2.Distance(this.Position, g.Position) < 25)
                    this.Remove(); 
            }
            foreach (var p in players)
            {
                if (Vector2.Distance(this.Position, p.Position) < 25 && !(this.Owner == p))
                {
                    this.Remove();
                    p.Health--;
                    ServerSend.RequestModifyHealth(p.ID, p.Health); 
                }
            }
        }

        public override void WriteIntoPacket(Packet packet)
        {
            base.WriteIntoPacket(packet);
            packet.Write(Direction); 
            packet.Write(Velocity);
            packet.Write(Owner.ID); 
        }
    }
}
