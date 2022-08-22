using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Managers.Networking;
using Microsoft.Xna.Framework;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Bullet : Sprite 
    {
        public float Direction{ get; set; }
        public float Velocity { get; set; }
        public int IdPlayer { get; set; }


        public Bullet()
            :base()
        {
            SetTexture(TextureManager.Instance.CreateTexture(20, 20, paint => Color.Black)); 
        }

    }
}
