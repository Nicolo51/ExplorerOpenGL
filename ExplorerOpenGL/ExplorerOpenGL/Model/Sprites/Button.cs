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
        private float mouseOverOpacityTexture; 

        public delegate void MouseOverEventHandler(object sender, object triggerer);
        public event MouseOverEventHandler MouseOver;

        public delegate void ClickEventHandler(object sender, object triggerer);
        public event ClickEventHandler Click; 

        public Button(Texture2D Texture, Texture2D MouseOverTexture, SpriteFont font)
            : base()
        {

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
            for(int i = 0; i < sprites.Count; i++)
            {
                if(sprites[i] is MousePointer)
                {
                    if(sprites[i].HitBox.Intersects(this.HitBox))
                    {
                        OnMouseOver(sprites[i]);
                        if (mouseOverOpacityTexture < 1)
                        {
                            mouseOverOpacityTexture += .2f;
                        }

                        else if (mouseOverOpacityTexture > 1)
                        {
                            mouseOverOpacityTexture = 1f;
                        }

                        if ((sprites[i] as MousePointer).currentMouseState.LeftButton == ButtonState.Pressed)
                        {
                            mouseOverOpacityTexture = 2f; 
                        }
                    }
                    else if (mouseOverOpacityTexture > 0)
                    {
                        mouseOverOpacityTexture -= .2f;
                    }
                    if (mouseOverOpacityTexture < 0)
                    {
                        mouseOverOpacityTexture = 0f;
                    }
                }
            }

            base.Update(gameTime, sprites, controler);
        }

        protected virtual void OnMouseOver(object triggerer)
        {
            MouseOver?.Invoke(this, triggerer); 
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(mouseOverTexture, Position, null, Color.White * mouseOverOpacityTexture, Radian, origin, scale, Effects, layerDepth);
            spriteBatch.DrawString(font, Text, Position, Color.White);
        }
    }
}
