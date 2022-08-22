using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerTCP.GameData.GameObjects
{
    public class Player
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public float LookAtRadian { get; set; }
        public float FeetRadian { get; set; }
        public int Health { get; set; }
        public Player(int id, string name)
        {
            ID = id;
            Name = name;
            Position = new Vector2(0, 0);
            LookAtRadian = 0f;
            FeetRadian = 0f;
            Health = 100; 
        }

        public Player(int id)
        {
            ID = id;
            Position = new Vector2(0, 0);
            LookAtRadian = 0f;
            FeetRadian = 0f;
            Health = 100; 
        }

        public override string ToString()
        {
            return $"ID: {ID}, Position: {Position.ToString()}, LookAtRadian: {LookAtRadian.ToString("#.##")}, FeetRadian:{FeetRadian.ToString("#.##")}"; 
        }
    }
}
