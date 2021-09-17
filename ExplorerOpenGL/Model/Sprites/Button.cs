using ExplorerOpenGL.Controlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private SpriteFont font; 

        private Texture2D mouseOverTexture;
        public bool isMouseOver; 
        
        public Button(Texture2D Texture, Texture2D MouseOverTexture, SpriteFont font)
            : base()
        {
            IsClickable = true;
            MouseClicked += OnMouseClick; 
            MouseOvered += OnMouseOver; 
            MouseLeft += OnMouseLeft;

            this.mouseOverTexture = MouseOverTexture;
            this.font = font; 
            this._texture = Texture;
            Text = String.Empty;
        }

        public Button()
            : base()
        {
            Text = String.Empty; 
        }


        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
           
            base.Update(gameTime, sprites, controler);
        }

        private void OnMouseOver(object sender, List<Sprite> sprites, Controler controler)
        {
            isMouseOver = true;
            controler.DebugManager.AddEvent("Over"); 
        }
        private void OnMouseLeft(object sender, List<Sprite> sprites, Controler controler)
        {
            isMouseOver = false;
            controler.DebugManager.AddEvent("Leave");
        }
        private void OnMouseClick(object sender, List<Sprite> sprites, Controler controler)
        {
            controler.DebugManager.AddEvent("Click");
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if(isMouseOver)
                spriteBatch.Draw(mouseOverTexture, Position, null, Color.White * opacity * (isClicked ? .5f : 1f), Radian, origin, scale, Effects, layerDepth);
            else
                spriteBatch.Draw(_texture, Position, null, Color.White, Radian, origin, scale, Effects, layerDepth);

            spriteBatch.DrawString(font, Text, Position, Color.White);
        }
    }
}
