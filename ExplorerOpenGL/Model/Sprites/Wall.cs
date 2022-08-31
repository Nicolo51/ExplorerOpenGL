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
            _texture = texture;
        }

        public Wall(params Animation[] animations)
            :base(animations)
        {
            isDraggable = true;
            _animation.Play("run"); 
        }

        public override void OnMouseOver(Sprite[] sprites, MousePointer mousePointer)
        {
            debugManager.AddEvent("over wall"); 
            base.OnMouseOver(sprites, mousePointer);
        }
        public override void OnMouseLeave(Sprite[] sprites, MousePointer mousePointer)
        {
            debugManager.AddEvent("leave wall");
            base.OnMouseOver(sprites, mousePointer);
        }
    }
}