using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Model.Sprites
{
    public class Wall : Sprite
    {
        public Wall(Texture2D texture)
            :base(texture)
        {
            init(); 
        }


        public Wall()
            : base()
        {
            init();
        }

        private void init()
        {
            isCollidable = true;
            IsPartOfGameState = true;
            isDraggable = true;
        }
    }
}