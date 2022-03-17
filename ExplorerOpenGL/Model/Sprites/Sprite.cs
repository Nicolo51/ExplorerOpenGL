﻿using ExplorerOpenGL.Managers;
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
        public Vector2 LastPosition; 

        protected Texture2D _texture;
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
        public float  layerDepth { get; set; }
        protected float scale { get; set; }
        public float Opacity { get; set; }
        public bool IsHUD { get; set; }

        public delegate void MouseOverEventHandler(object sender, MousePointer mousePointer);
        public event MouseOverEventHandler MouseOvered;

        public delegate void MouseLeaveEventHandler(object sender, MousePointer mousePointer);
        public event MouseLeaveEventHandler MouseLeft;

        public delegate void MouseClickEventHandler(object sender, MousePointer mousePointer,  Vector2 clickPosition);
        public event MouseClickEventHandler MouseClicked;

        protected GameManager gameManager;
        protected TimeManager timeManager; 

        protected bool isClicked;
        private bool isOver; 
        public bool IsClickable;
        public  bool isDraggable;
        private bool isDragged;
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
        }

        public Sprite(Texture2D texture)
        {
            _texture = texture;
            SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            alignOption = AlignOption.None;
            isDraggable = false;
            IsHUD = false;
            scale = 1;
            Opacity = 1f;
            layerDepth = 1f; 
            timeManager = TimeManager.Instance;
            gameManager = GameManager.Instance;
        }

        public virtual void Update(Sprite[] sprites)
        {
           LastPosition = Position; 
            if (IsClickable || isDraggable)
                CheckMouseEvent(sprites);
        }

        public virtual void SetTextureAsync(Texture2D texture)
        {
            _texture = texture;
            if(SourceRectangle == null)
                SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        private void CheckMouseEvent(Sprite[] sprites)
        {
            MousePointer mousePointer = gameManager.MousePointer; 
            if ((!this.IsHUD && this.HitBox.Intersects(new Rectangle((int)gameManager.MousePointer.InWindowPosition.X, (int)gameManager.MousePointer.InWindowPosition.Y, 1, 1))) || isDragged)
            {
                if (!isOver)
                    OnMouseOver(sprites);

                isOver = true;
                if ((gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && gameManager.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    if (!isClicked)
                        ClickPosition = new Vector2(mousePointer.InWindowPosition.X - Position.X + origin.X, mousePointer.InWindowPosition.Y - Position.Y + origin.Y);
                    isClicked = true;
                    if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released && !isDragged)
                    {
                        OnMouseClick(sprites, ClickPosition);
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
                        Vector2 currentClickPosition = new Vector2(mousePointer.InWindowPosition.X - Position.X + origin.X, mousePointer.InWindowPosition.Y - Position.Y + origin.Y);
                        float distance = Vector2.Distance(ClickPosition, currentClickPosition);
                        if (distance > 3)
                        {
                            isDragged = true;
                        }
                    }
                    else
                    {
                        Position = mousePointer.InWindowPosition + origin - ClickPosition;
                    }
                }
            }
            else if ((this.IsHUD && gameManager.MousePointer.HitBox.Intersects(this.HitBox)) || isDragged)
            {
                if (!isOver)
                    OnMouseOver(sprites);

                isOver = true;
                if ((gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Pressed && gameManager.MousePointer.prevMouseState.LeftButton == ButtonState.Released) || isClicked)
                {
                    if (!isClicked)
                        ClickPosition = new Vector2(mousePointer.Position.X - Position.X + origin.X, mousePointer.Position.Y - Position.Y + origin.Y);
                    isClicked = true;
                    if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released && !isDragged)
                    {
                        OnMouseClick(sprites, ClickPosition);
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
                        Position = mousePointer.Position + origin - ClickPosition;
                    }
                }
            }
            else if (gameManager.MousePointer.currentMouseState.LeftButton == ButtonState.Released)
            {
                if (isOver && !isDragged)
                {
                    isClicked = false;
                    OnMouseLeave(sprites);
                    isOver = false;
                }
            }
        }

        public virtual void Remove()
        {
            IsRemove = true; 
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

        private void OnMouseOver(Sprite[] sprites)
        {
            MouseOvered?.Invoke(this, gameManager.MousePointer);
        }

        private void OnMouseLeave(Sprite[] sprites)
        {
            MouseLeft?.Invoke(this, gameManager.MousePointer);
        }

        private void OnMouseClick(Sprite[] sprites, Vector2 clickPosition)
        {
            MouseClicked?.Invoke(this, gameManager.MousePointer, clickPosition);
        }

        public virtual void Draw(SpriteBatch spriteBatch, float lerpAmount)
        {
            if(_texture != null)
                spriteBatch.Draw(_texture, Vector2.Lerp(LastPosition, Position, lerpAmount), SourceRectangle, Color.White * Opacity * (isClicked && IsClickable ? .5f : 1f), Radian, origin, scale, Effects, layerDepth);
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
