using ExplorerOpenGL.Managers;
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
        public Rectangle SourceRectangle { get; set; }
        public virtual Rectangle HitBox { get {
                if (_texture != null) return new Rectangle((int)Position.X - (int)origin.X, (int)Position.Y - (int)origin.Y, (int)(SourceRectangle.Width * scale), (int)(SourceRectangle.Height * scale));
                else return new Rectangle((int)Position.X, (int)Position.Y, 1, 1); 
            } }
        public Vector2 origin;
        public SpriteEffects Effects { get; set; }
        public float  layerDepth { get; set; }
        protected float scale { get; set; }
        public float Opacity { get; set; }
        public bool IsHUD { get; set; }

        public delegate void MouseOverEventHandler(object sender, MousePointer mousePointer, GameManager GameManager);
        public event MouseOverEventHandler MouseOvered;

        public delegate void MouseLeaveEventHandler(object sender, MousePointer mousePointer, GameManager GameManager);
        public event MouseLeaveEventHandler MouseLeft;

        public delegate void MouseClickEventHandler(object sender, MousePointer mousePointer, GameManager GameManager, Vector2 clickPosition);
        public event MouseClickEventHandler MouseClicked;

        protected GameManager gameManager; 

        protected bool isClicked;
        private bool isOver; 
        protected bool IsClickable;

        public Sprite()
        {
            IsHUD = false; 
            scale = 1;
            Opacity = 1f;
            gameManager = GameManager.Instance; 
        }

        public Sprite(Texture2D texture)
        {
            _texture = texture;
            SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            IsHUD = false;
            scale = 1;
            Opacity = 1f;
            gameManager = GameManager.Instance; 
        }

        public virtual void Update(GameTime gameTime, List<Sprite> sprites)
        {
            if (IsClickable)
                CheckMouseEvent(sprites); 
        }

        private void CheckMouseEvent(List<Sprite> sprites)
        {
            MousePointer mousePointer = gameManager.MousePointer; 
            if (this.HitBox.Intersects(new Rectangle((int)gameManager.MousePointer.InWindowPosition.X, (int)gameManager.MousePointer.InWindowPosition.Y, 1, 1)) && !this.IsHUD)
            {
                if (!isOver)
                    OnMouseOver(sprites, gameManager);

                isOver = true;
                if ((gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && gameManager.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    isClicked = true;
                    if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                    {
                        Vector2 ClickPosition = new Vector2(mousePointer.Position.X - Position.X - origin.X / 2, mousePointer.Position.Y - Position.Y - origin.Y / 2);
                        OnMouseClick(sprites, gameManager, ClickPosition);
                    }
                }
                if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                {
                    isClicked = false;
                }
            }
            else if (this.IsHUD && gameManager.MousePointer.HitBox.Intersects(this.HitBox))
            {
                if (!isOver)
                    OnMouseOver(sprites, gameManager);

                isOver = true;
                if ((gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && gameManager.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    isClicked = true;
                    if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                    {
                        Vector2 ClickPosition = new Vector2(mousePointer.InWindowPosition.X - Position.X - origin.X / 2, mousePointer.InWindowPosition.Y - Position.Y - origin.Y / 2);
                        OnMouseClick(sprites, gameManager, ClickPosition);
                    }
                }
                if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                {
                    isClicked = false;
                }
            }
            else if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
            {
                if (isOver)
                {
                    isClicked = false;
                    OnMouseLeave(sprites, gameManager);
                    isOver = false;
                }
            }
        }

        private void OnMouseOver(List<Sprite> sprites, GameManager GameManager)
        {
            MouseOvered?.Invoke(this, GameManager.MousePointer, GameManager);
        }

        private void OnMouseLeave(List<Sprite> sprites, GameManager GameManager)
        {
            MouseLeft?.Invoke(this, GameManager.MousePointer, GameManager);
        }

        private void OnMouseClick(List<Sprite> sprites, GameManager GameManager, Vector2 clickPosition)
        {
            MouseClicked?.Invoke(this, GameManager.MousePointer, GameManager, clickPosition);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if(_texture != null && !(this is MousePointer))
                spriteBatch.Draw(_texture, Position, null, Color.White * Opacity * (isClicked ? .5f : 1f), Radian, origin, scale, Effects, layerDepth);
        }
    }
}
