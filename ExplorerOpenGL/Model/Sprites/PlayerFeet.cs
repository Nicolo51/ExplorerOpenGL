using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    class PlayerFeet: Sprite
    {
        public PlayerFeet(Texture2D texture)
            :base(texture)
        {
            _texture = texture;
            origin = new Vector2(texture.Width / 2, texture.Height / 2);

        }

        public void SetDirection(float direction)
        {
            Radian = direction; 
        }
    }
}
