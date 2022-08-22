using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GameServerTCP.GameData
{
    public struct Vector2
    {
        public float X { get; }
        public float Y { get; }
        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2(float radian)
        {
            this.X = (float)Math.Cos(radian); 
            this.Y = (float)Math.Sin(radian); 
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[8];
            Array.Copy(BitConverter.GetBytes(X), 0, bytes , 0, 4);
            Array.Copy(BitConverter.GetBytes(Y), 0, bytes, 4, 4);
            return bytes;
        }
        public override string ToString()
        {
            return "{X:" + X.ToString("0.##") + ", Y:" + Y.ToString("0.##") + "}";
        }

        public static Vector2 One { get { return new Vector2(1f, 1f); } }
        public static Vector2 Zero { get { return new Vector2(0f,0f); } }

        public static float Distance(Vector2 vector1, Vector2 vector2)
        {
            return (float)Math.Sqrt((Math.Pow(vector1.X - vector2.X, 2) + Math.Pow(vector1.Y - vector2.Y, 2)));
        }

        public Point ToPoint()
        {
            return new Point((int)X, (int)Y);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2 vector &&
                   X == vector.X &&
                   Y == vector.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Vector2 vector1, Vector2 vector2)
        {
            if(vector1.X == vector2.X && vector1.Y == vector2.Y)
            {
                return true;
            }
            return false;
        }
        public static bool operator !=(Vector2 vector1, Vector2 vector2)
        {
            if (vector1.X != vector2.X || vector1.Y != vector2.Y)
            {
                return true;
            }
            return false;
        }
        public static Vector2 operator +(Vector2 vector1, Vector2 vector2)
        {
            return new Vector2(vector1.X + vector2.X, vector1.Y + vector2.Y);
        }
        public static Vector2 operator -(Vector2 vector1, Vector2 vector2)
        {
            return new Vector2(vector1.X - vector2.X, vector1.Y - vector2.Y);
        }
        public static Vector2 operator *(Vector2 vector1, Vector2 vector2)
        {
            return new Vector2(vector1.X * vector2.X, vector1.Y * vector2.Y);
        }
        public static Vector2 operator *(Vector2 vector1, float value)
        {
            return new Vector2(vector1.X * value, vector1.Y * value);
        }
        public static Vector2 operator /(Vector2 vector1, Vector2 vector2)
        {
            return new Vector2(vector1.X / vector2.X, vector1.Y / vector2.Y);
        }
        public static Vector2 operator /(Vector2 vector1, float value)
        {
            return new Vector2(vector1.X / value, vector1.Y / value);
        }
    }
}
