using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Button : Sprite
    {
        public string Text { get; set; }
        public Color color { get; private set; }


        public Button(Texture2D Texture)
            : base()
        {
            _texture = Texture; 
        }

        public Button()
        : base()
        {

        }


        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            base.Update(gameTime, sprites);
        }

    }
}
