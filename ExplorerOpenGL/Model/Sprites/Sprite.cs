using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Managers.Networking;
using ExplorerOpenGL.Managers.Networking.NetworkObject;
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
        public int ID { get; set; }
        public Vector2 Position;
        public Vector2 LastPosition;

        protected Texture2D _texture;
        protected AnimationManager _animation;
        public float Radian { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public virtual Rectangle HitBox { get {
                if (_texture != null && SourceRectangle != null) return new Rectangle((int)Position.X - (int)origin.X, (int)Position.Y - (int)origin.Y, (int)(SourceRectangle.Width * scale), (int)(SourceRectangle.Height * scale));
                else return new Rectangle((int)Position.X, (int)Position.Y, 1, 1);
            } }
        public Vector2 origin;
        protected AlignOption alignOption;
        public Rectangle Bounds { get { if (_texture != null) return _texture.Bounds; return Rectangle.Empty; } }
        public SpriteEffects Effects { get; set; }
        public float layerDepth { get; set; }
        protected float scale { get; set; }
        public float Opacity { get; set; }

        public bool IsHUD { get; set; }
        public bool IsDisplayed { get; set; }
        public bool IsClickable { get; set; }
        public bool isDraggable { get; set; }

        protected bool isClicked;
        protected bool isOver;
        protected bool isDragged;


        public delegate void MouseOverEventHandler(object sender, MousePointer mousePointer);
        public event MouseOverEventHandler MouseOvered;

        public delegate void MouseLeaveEventHandler(object sender, MousePointer mousePointer);
        public event MouseLeaveEventHandler MouseLeft;

        public delegate void MouseClickEventHandler(object sender, MousePointer mousePointer, Vector2 clickPosition);
        public event MouseClickEventHandler MouseClicked;

        protected GameManager gameManager;
        protected TimeManager timeManager;
        protected DebugManager debugManager;

        Vector2 ClickPosition;

        public Sprite()
        {
            alignOption = AlignOption.None;
            isDraggable = false;
            IsHUD = false;
            scale = 1;
            Opacity = 1f;
            layerDepth = 1f;
            gameManager = GameManager.Instance;
            timeManager = TimeManager.Instance;
            IsDisplayed = true;
        }

        public Sprite(params Animation[] animations) 
        {
            _animation = new AnimationManager();
            foreach (var a in animations)
                _animation.Add(a.Name, a); 
            alignOption = AlignOption.None;
            isDraggable = false;
            IsDisplayed = true;
            IsHUD = false;
            scale = 1;
            Opacity = 1f;
            layerDepth = 1f;
            timeManager = TimeManager.Instance;
            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance;

        }

        public Sprite(Texture2D texture)
        {
            SetTexture(texture);
            alignOption = AlignOption.None;
            isDraggable = false;
            IsDisplayed = true; 
            IsHUD = false;
            scale = 1;
            Opacity = 1f;
            layerDepth = 1f; 
            timeManager = TimeManager.Instance;
            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance; 
        }

        public virtual void NetworkUpdate(NetworkGameObject ngo)
        {
            if (ID != ngo.ID)
                return; 
            SetPosition(ngo.Position, false);
            if (ngo.IsRemove)
                Remove();
            Radian = ngo.Direction;
        }

        public virtual void Update(Sprite[] sprites)
        {
            if (!IsDisplayed)
                return; 
            LastPosition = Position; 
             if (IsClickable || isDraggable)
                CheckMouseEvent(sprites);
        }

        public virtual void SetTexture(Texture2D texture)
        {
            _texture = texture;
            SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        private void CheckMouseEvent(Sprite[] sprites)
        {
            MousePointer mousePointer = gameManager.MousePointer; 
            if (!this.IsHUD && (this.HitBox.Intersects(new Rectangle((int)gameManager.MousePointer.InGamePosition.X, (int)gameManager.MousePointer.InGamePosition.Y, 1, 1)) || isDragged))
            {
                if (!isOver)
                    OnMouseOver(sprites, mousePointer);

                isOver = true;
                if ((gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && gameManager.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    if (!isClicked)
                        ClickPosition = new Vector2(mousePointer.InGamePosition.X - Position.X + origin.X, mousePointer.InGamePosition.Y - Position.Y + origin.Y);
                    isClicked = true;
                    if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released && !isDragged)
                    {
                        OnMouseClick(sprites, ClickPosition, mousePointer);
                        isDragged = false;
                    }
                }
                if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                {
                    isClicked = false;
                    isDragged = false;
                }
                if (isClicked && isOver && isDraggable)
                {
                    if (!isDragged)
                    {
                        Vector2 currentClickPosition = new Vector2(mousePointer.InGamePosition.X - Position.X + origin.X, mousePointer.InGamePosition.Y - Position.Y + origin.Y);
                        float distance = Vector2.Distance(ClickPosition, currentClickPosition);
                        if (distance > 3)
                        {
                            isDragged = true;
                        }
                    }
                    else
                    {
                        Position = mousePointer.InGamePosition + origin - ClickPosition;
                        DebugManager.Instance.AddEvent(ClickPosition);
                    }
                }
            }
            else if (this.IsHUD && (gameManager.MousePointer.HitBox.Intersects(this.HitBox) || isDragged))
            {
                if (!isOver)
                    OnMouseOver(sprites, mousePointer);

                isOver = true;
                if ((gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && gameManager.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    if (!isClicked)
                        ClickPosition = new Vector2(mousePointer.Position.X - Position.X + origin.X, mousePointer.Position.Y - Position.Y + origin.Y);
                    isClicked = true;
                    if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released && !isDragged)
                    {
                        OnMouseClick(sprites, ClickPosition, mousePointer);
                        isDragged = false;
                    }
                }
                if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
                {
                    isClicked = false;
                    isDragged = false;
                }
                if (isClicked && isOver && isDraggable)
                {
                    if (!isDragged)
                    {
                        Vector2 currentClickPosition = new Vector2(mousePointer.Position.X - Position.X + origin.X, mousePointer.Position.Y - Position.Y + origin.Y);
                        float distance = Vector2.Distance(ClickPosition, currentClickPosition);
                        if (distance > 3)
                        {
                            isDragged = true;
                        }
                    }
                    else
                    {
                        DebugManager.Instance.AddEvent(ClickPosition);
                        Position = mousePointer.Position + origin - ClickPosition;
                    }
                }
            }
            else if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
            {
                if (isOver && !isDragged)
                {
                    isClicked = false;
                    OnMouseLeave(sprites, mousePointer);
                    isOver = false;
                }
            }
        }

        public virtual void Remove()
        {
            IsRemove = true; 
        }

        public virtual void SetPosition(Vector2 newPos, bool instant = true)
        {
            if (instant)
                LastPosition = newPos;
            Position = newPos;
        }

        public virtual void SetAlignOption(AlignOption ao)
        {
            if (ao == alignOption)
                return;
            Vector2 bounds = new Vector2(HitBox.Width, HitBox.Height);
            switch (ao)
            {
                case AlignOption.Left:
                    origin = new Vector2(0, bounds.Y / 2);
                    break;
                case AlignOption.Right:
                    origin = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOption.Top:
                    origin = new Vector2(bounds.X / 2, 0);
                    break;
                case AlignOption.Bottom:
                    origin = new Vector2(bounds.X / 2, bounds.Y);
                    break;
                case AlignOption.BottomLeft:
                    origin = new Vector2(0, bounds.Y);
                    break;
                case AlignOption.BottomRight:
                    origin = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOption.TopLeft:
                    origin = new Vector2(0, 0);
                    break;
                case AlignOption.TopRight:
                    origin = new Vector2(bounds.X, 0);
                    break;
                case AlignOption.Center:
                    origin = new Vector2(bounds.X / 2, bounds.Y / 2);
                    break;
            }
            alignOption = ao;
        }

        public void SetPadding(Vector2 pad)
        {
            SetAlignOption(alignOption);
            origin += pad;
        }

        public virtual void OnMouseOver(Sprite[] sprites, MousePointer mousePointer)
        {
            MouseOvered?.Invoke(this, gameManager.MousePointer);
        }

        public virtual void OnMouseLeave(Sprite[] sprites, MousePointer mousePointer)
        {
            MouseLeft?.Invoke(this, gameManager.MousePointer);
        }

        public virtual void OnMouseClick(Sprite[] sprites, Vector2 clickPosition, MousePointer mousePointer)
        {
            MouseClicked?.Invoke(this, gameManager.MousePointer, clickPosition);
        }

        public virtual void Draw(SpriteBatch spriteBatch,  GameTime gameTime, float lerpAmount)
        {
            if ((_texture != null || _animation != null) && IsDisplayed)
            {
                if(_animation != null)
                    SourceRectangle = _animation.GetRectangle(gameTime); 
                if(this is MessageBox)
                    spriteBatch.Draw(_texture, Position , SourceRectangle, Color.White * Opacity * (isClicked && IsClickable ? .5f : 1f), Radian, origin, scale, Effects, layerDepth);
                else
                    spriteBatch.Draw(_texture ?? _animation.Texture, Vector2.Lerp(LastPosition, Position, lerpAmount), SourceRectangle, Color.White * Opacity * (isClicked && IsClickable ? .5f : 1f), Radian, origin, scale, Effects, layerDepth);
            }
        }
    }

    public enum AlignOption 
    {
        None,
        Left,
        Right,
        Center,
        Top,
        Bottom,
        BottomRight,
        BottomLeft,
        TopRight,
        TopLeft
    }

}
