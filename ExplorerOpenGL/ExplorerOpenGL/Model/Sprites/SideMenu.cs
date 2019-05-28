using ExplorerOpenGL.Controlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class SideMenu : Sprite
    {
        private SpriteFont font; 
        
        public bool IsOpen { get; set; }
        public Color FontColor { get; set; }

        public string name { get; set; }

        public Sides Side { get; set; }

        public SideMenu(Texture2D texture, SpriteFont Font)
            :base()
        {
            name = string.Empty; 
            font = Font; 
            _texture = texture;
            Side = Sides.Left; 
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            base.Update(gameTime, sprites, controler);
        }

        public void OnClientSizeChanged(object sender, EventArgs e)
        {
            if (Side == Sides.Left || Side == Sides.Top)
                return; 
            ///TODO changer la posi si elle est pas a zero

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(font, name, Position + new Vector2(10, 20), Color.Black, Radian, Vector2.Zero, scale , Effects, layerDepth - .1f);

        }


    }
}
