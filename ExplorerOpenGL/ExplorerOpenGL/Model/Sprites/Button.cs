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
        public delegate void MouseLeaveEventHandler(object sender, object triggerer);
        public event MouseLeaveEventHandler MouseLeave;
        private bool isMouseOver;
        private bool isClicked; 
       
        public delegate void ClickEventHandler(object sender, object triggerer);
        public event ClickEventHandler Click; 

        public Button(Texture2D Texture, Texture2D MouseOverTexture, SpriteFont font)
            : base()
        {
            isMouseOver = false;
            isClicked = false; 
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
            for (int i = 0; i < sprites.Count && sprites[i] is MousePointer; i++)
            {
                if (sprites[i] is MousePointer)
                {
                    if (this.HitBox.Intersects((sprites[i] as MousePointer).HitBox))
                    {
                        if (((sprites[i] as MousePointer).currentMouseState.LeftButton == ButtonState.Pressed && (sprites[i] as MousePointer).prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                        {
                            isClicked = true;
                            opacity = 0.5f;
                            if ((sprites[i] as MousePointer).currentMouseState.LeftButton == ButtonState.Released)
                            {
                                //faire la commande au moment ou le bouton est clické et relaché
                                opacity = 1f; 
                            }
                        }
                        if ((sprites[i] as MousePointer).currentMouseState.LeftButton == ButtonState.Released)
                        {
                            isClicked = false;
                        }
                    }
                    else if ((sprites[i] as MousePointer).currentMouseState.LeftButton == ButtonState.Released && !(this.HitBox.Intersects((sprites[i] as MousePointer).HitBox)))
                    {
                        isClicked = false;
                    }
                    else
                    {
                        opacity = 1f;
                    }
                }
            }

            base.Update(gameTime, sprites, controler);
        }

        protected virtual void OnMouseOver(object triggerer)
        {
            MouseOver?.Invoke(this, triggerer);
            isMouseOver = true;
        }
        protected virtual void OnMouseLeave(object triggerer)
        {
            MouseLeave?.Invoke(this, triggerer);
            isMouseOver = false; 
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(mouseOverTexture, Position, null, Color.White * mouseOverOpacityTexture, Radian, origin, scale, Effects, layerDepth);
            spriteBatch.DrawString(font, Text, Position, Color.White);
        }
    }
}
