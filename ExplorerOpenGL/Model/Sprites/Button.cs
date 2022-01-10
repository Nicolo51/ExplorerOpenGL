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
        private Vector2 originMouseOver; 
        public bool isMouseOver; 
        
        public Button(Texture2D texture, Texture2D MouseOverTexture, SpriteFont font)
            : base(texture)
        {
            IsClickable = true;
            MouseClicked += OnMouseClick; 
            MouseOvered += OnMouseOver; 
            MouseLeft += OnMouseLeft;

            this.mouseOverTexture = MouseOverTexture;
            this.originMouseOver = new Vector2(mouseOverTexture.Width / 2, mouseOverTexture.Height / 2); 
            this.origin = new Vector2(texture.Width / 2, texture.Height / 2);
            this.font = font; 
            Text = String.Empty;
            layerDepth = 0.1f;
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
            controler.MousePointer.SourceRectangle = new Rectangle(300, 0, 75, 75);
            controler.DebugManager.AddEvent("Over"); 
        }
        private void OnMouseLeft(object sender, List<Sprite> sprites, Controler controler)
        {
            isMouseOver = false;
            controler.MousePointer.SourceRectangle = new Rectangle(0, 0, 75, 75);

            controler.DebugManager.AddEvent("Leave");
        }
        private void OnMouseClick(object sender, List<Sprite> sprites, Controler controler)
        {
            controler.DebugManager.AddEvent("Click");
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if(isMouseOver)
                spriteBatch.Draw(mouseOverTexture, Position, null, Color.White * Opacity * (isClicked ? .5f : 1f), Radian, originMouseOver, scale, Effects, layerDepth);
            else
                spriteBatch.Draw(_texture, Position, null, Color.White, Radian, origin, scale, Effects, layerDepth);

            if(!(font == null) || !string.IsNullOrWhiteSpace(Text))
                spriteBatch.DrawString(font, Text, Position, Color.White);
        }
    }
}
