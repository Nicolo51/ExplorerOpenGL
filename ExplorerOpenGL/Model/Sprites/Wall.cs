using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Wall : Sprite
    {
        public Wall(Texture2D texture)
            :base(texture)
        {
            //_texture = texture;
            isCollidable = true; 
        }

        public Wall(params Animation[] animations)
            :base(animations)
        {
            isDraggable = true;
            Play("run"); 
        }

        public Wall()
            : base()
        {

        }
    }
}