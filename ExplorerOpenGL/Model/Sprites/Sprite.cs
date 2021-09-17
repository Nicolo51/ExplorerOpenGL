using ExplorerOpenGL.Controlers;
using ExplorerOpenGL.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Sprite
    {
        public bool IsRemove { get; set; }
        public Vector2 Position;
        protected Texture2D _texture;
        public float Radian { get; set; }
        public Rectangle HitBox { get {
                if (_texture != null) return new Rectangle((int)Position.X - (int)origin.X, (int)Position.Y - (int)origin.Y, _texture.Width, _texture.Height);
                else return new Rectangle((int)Position.X, (int)Position.Y, 1, 1); 
            } }
        protected Vector2 origin;
        public SpriteEffects Effects { get; set; }
        public float  layerDepth { get; set; }
        protected float scale { get; set; }
        protected float opacity { get; set; }
        public bool IsHUD { get; set; }

        public delegate void MouseOverEventHandler(object sender, List<Sprite> sprites, Controler controler);
        public event MouseOverEventHandler MouseOvered;

        public delegate void MouseLeaveEventHandler(object sender, List<Sprite> sprites, Controler controler);
        public event MouseLeaveEventHandler MouseLeft;

        public delegate void MouseClickEventHandler(object sender, List<Sprite> sprites, Controler controler);
        public event MouseClickEventHandler MouseClicked;

        protected bool isClicked;
        private bool isOver; 
        protected bool IsClickable; 

        public Sprite()
        {
            IsHUD = false; 
            scale = 1;
            opacity = 1f; 
        }

        public virtual void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            if (IsClickable)
                CheckMouseEvent(sprites, controler); 
        }

        private void CheckMouseEvent(List<Sprite> sprites, Controler controler)
        {
            if (this.HitBox.Intersects(controler.MousePointer.HitBox) && !this.IsHUD)
            {
                if (!isOver)
                    OnMouseOver(sprites, controler);

                isOver = true;
                if ((controler.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && controler.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    isClicked = true;
                    if (controler.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                    {
                        OnMouseClick(sprites, controler);
                    }
                }
                if (controler.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                {
                    isClicked = false;
                }
            }
            else if (this.IsHUD && new Rectangle((int)controler.MousePointer.InWindowPosition.X, (int)controler.MousePointer.InWindowPosition.Y, 1, 1).Intersects(this.HitBox))
            {
                if (!isOver)
                    OnMouseOver(sprites, controler);

                isOver = true;
                if ((controler.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && controler.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    isClicked = true;
                    if (controler.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                    {
                        OnMouseClick(sprites, controler);
                    }
                }
                if (controler.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                {
                    isClicked = false;
                }
            }
            else if (controler.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
            {
                if (isOver)
                {
                    isClicked = false;
                    OnMouseLeave(sprites, controler);
                    isOver = false;
                }
            }
        }

        private void OnMouseOver(List<Sprite> sprites, Controler controler)
        {
            MouseOvered?.Invoke(this, sprites, controler);
        }

        private void OnMouseLeave(List<Sprite> sprites, Controler controler)
        {
            MouseLeft?.Invoke(this, sprites, controler);
        }

        private void OnMouseClick(List<Sprite> sprites, Controler controler)
        {
            MouseClicked?.Invoke(this, sprites, controler);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if(_texture != null && !(this is MousePointer))
                spriteBatch.Draw(_texture, Position, null, Color.White * opacity * (isClicked ? .5f : 1f), Radian, origin, scale, Effects, layerDepth);
        }
    }
}
