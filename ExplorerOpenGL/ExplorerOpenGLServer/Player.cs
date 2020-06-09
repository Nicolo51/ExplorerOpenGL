using ExplorerOpenGLInterfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGLServer
{
    public class Player : IPlayer
    {
        public Vector2 Direction { get; set; }
        public Vector2 Position { get; set; }
        public int ID;
        public IPlayer GetOnlineInfo()
        {
            return null;
        }

        public Vector2 getDirection()
        {
            throw new NotImplementedException();
        }

        public Vector2 getPosition()
        {
            throw new NotImplementedException();
        }
    }
}
