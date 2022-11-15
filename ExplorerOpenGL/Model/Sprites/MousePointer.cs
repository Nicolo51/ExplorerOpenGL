using ExplorerOpenGL.Managers;
using ExplorerOpenGL.View;
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
    public class MousePointer : Sprite
    {
        Dictionary<MousePointerType, Rectangle> MousePointerTypes;
        public MouseState currentMouseState { get; private set; }
        public MouseState prevMouseState { get; private set; }
        public Vector2 InGamePosition { get; private set; }
        public override Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, 1, 1); } }
        private MousePointerType defaultType;

        public Sprite OverSprite { get; private set; }
        public Sprite LastOverSprite { get; private set; }
        private Vector2 ClickPosition;
        private bool isDragging;
        private bool isClicking;

        public MousePointer(Texture2D texture)
            : base(texture)
        {
            defaultType = MousePointerType.Arrow;
            //_texture = texture;
            SourceRectangle = new Rectangle(300, 0, 75, 75);
            Scale = .5f;
            LayerDepth = 0f;
            InitMouseTypes();
            SetCursorIcon(MousePointerType.Arrow);
            IsHUD = true;
            Bounds = new Vector2(75, 75);
        }
        public MousePointer()
        {
        }

        public void SetDefaultIcon(MousePointerType type)
        {
            defaultType = type;
        }
        public void SetCursorIcon(MousePointerType type)
        {
            if (type == MousePointerType.Default)
                type = defaultType;
            SourceRectangle = MousePointerTypes[type];
            switch (type)
            {
                case MousePointerType.Arrow:
                case MousePointerType.SmallCursor:
                    origin = Vector2.Zero;
                    break;
                case MousePointerType.Crosshair:
                case MousePointerType.Aim:
                case MousePointerType.SmallCrosshair:
                case MousePointerType.Text:
                    origin = new Vector2(SourceRectangle.Width / 2, SourceRectangle.Height / 2);
                    break;
                case MousePointerType.Pointer:
                    origin = new Vector2(SourceRectangle.Width / 2, 0);
                    break;
            }
        }

        public override void Update(Sprite[] sprites)
        {
            base.Update(sprites);
            if (currentMouseState != null)
            {
                prevMouseState = currentMouseState;
            }
            currentMouseState = Mouse.GetState();
            SetPosition(new Vector2(currentMouseState.Position.X, currentMouseState.Position.Y), false);

            InGamePosition = new Vector2((gameManager.Camera.Position.X - gameManager.Camera.Bounds.X / 2 + currentMouseState.Position.X), (gameManager.Camera.Position.Y - gameManager.Camera.Bounds.Y / 2 + currentMouseState.Position.Y));

            if (!isDragging)
            {
                LastOverSprite = OverSprite;
                OverSprite = null;
            }

            for (int i = 0; i < sprites.Length && !isDragging; i++)
            {
                Sprite sprite = sprites[i];
                if ((!sprite.IsClickable && !sprite.isDraggable) || !sprite.IsDisplayed)
                    continue;
                lock (sprite)
                {
                    if ((sprite.IsHUD && sprite.HitBox.Intersects(this.HitBox) && OverSprite == null) ||
                        (!sprite.IsHUD && sprite.HitBox.Intersects(new Rectangle((int)InGamePosition.X, (int)InGamePosition.Y, 1, 1)) && OverSprite == null) ||
                        (sprite.IsHUD && sprite.HitBox.Intersects(this.HitBox) && OverSprite.LayerDepth >= sprite.LayerDepth) ||
                        (!sprite.IsHUD && sprite.HitBox.Intersects(new Rectangle((int)InGamePosition.X, (int)InGamePosition.Y, 1, 1)) && OverSprite.LayerDepth >= sprite.LayerDepth))
                    {
                        OverSprite = sprite;
                    }
                }
            }
            if (OverSprite != null || LastOverSprite != null)
                ProcessMouseOver(sprites);
            else
            {
                isClicking = false;
                isDragging = false; 
            }
        }

        private void ProcessMouseOver(Sprite[] sprites)
        {
            if (OverSprite == null)
            {
                LastOverSprite.OnMouseLeave(sprites, this);
                return;
            }
            if (OverSprite != null && OverSprite != LastOverSprite)
            {
                if (LastOverSprite != null)
                    LastOverSprite.OnMouseLeave(sprites, this);
                OverSprite.OnMouseOver(sprites, this);
            }

            if (isClicking || (this.currentMouseState.LeftButton == ButtonState.Pressed && this.prevMouseState.LeftButton == ButtonState.Released))
            {
                if (!isClicking)
                    ClickPosition = OverSprite.IsHUD ? new Vector2(Position.X - OverSprite.Position.X + OverSprite.Origin.X, Position.Y - OverSprite.Position.Y + OverSprite.Origin.Y) :
                                                       new Vector2(InGamePosition.X - OverSprite.Position.X + OverSprite.Origin.X, InGamePosition.Y - OverSprite.Position.Y + OverSprite.Origin.Y);
                isClicking = true;
                if (this.currentMouseState.LeftButton == ButtonState.Released && !isDragging)
                {
                    OverSprite.OnMouseClick(sprites, ClickPosition, this);
                    isDragging = false;
                }
            }
            if (this.currentMouseState.LeftButton == ButtonState.Released)
            {
                isClicking = false;
                isDragging = false;
            }
            if (isClicking && OverSprite != null && LastOverSprite != null && OverSprite.isDraggable)
            {

                if (!isDragging)
                {
                    Vector2 currentClickPosition = OverSprite.IsHUD ? new Vector2(Position.X - OverSprite.Position.X + OverSprite.Origin.X, Position.Y - OverSprite.Position.Y + OverSprite.Origin.Y) :
                                                                      new Vector2(InGamePosition.X - OverSprite.Position.X + OverSprite.Origin.X, InGamePosition.Y - OverSprite.Position.Y + OverSprite.Origin.Y);
                    float distance = Vector2.Distance(ClickPosition, currentClickPosition);
                    if (distance > 3)
                    {
                        if(OverSprite != LastOverSprite)
                            ClickPosition = OverSprite.IsHUD ? new Vector2(Position.X - OverSprite.Position.X + OverSprite.Origin.X, Position.Y - OverSprite.Position.Y + OverSprite.Origin.Y) :
                                                       new Vector2(InGamePosition.X - OverSprite.Position.X + OverSprite.Origin.X, InGamePosition.Y - OverSprite.Position.Y + OverSprite.Origin.Y);
                        isDragging = true;
                    }
                }
                else
                {
                    OverSprite.Position = OverSprite.IsHUD ? Position + OverSprite.Origin - ClickPosition :
                                                             InGamePosition + OverSprite.Origin - ClickPosition;
                }
            }
            if (currentMouseState.LeftButton == ButtonState.Released && OverSprite == null && LastOverSprite != null)
            {
                if (!isDragging)
                {
                    isClicking = false;
                    LastOverSprite.OnMouseLeave(sprites, this);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float lerpAmount, params ShaderArgument[] shaderArgs)
        {
            //spriteBatch.Draw(_texture, Position, SourceRectangle, Color.White * Opacity, Radian, origin, scale, Effects, layerDepth);
            base.Draw(spriteBatch, gameTime, lerpAmount);
        }

        public override string ToString()
        {
            return "Position as HUD : " + Position.X + " / " + Position.Y + "\nPosition as NonHUD : " + InGamePosition.X + " / " + InGamePosition.Y;
        }
        private void InitMouseTypes()
        {
            MousePointerTypes = new Dictionary<MousePointerType, Rectangle>();
            MousePointerTypes.Add(MousePointerType.Arrow, new Rectangle(0, 0, 75, 75));
            MousePointerTypes.Add(MousePointerType.Crosshair, new Rectangle(75, 0, 75, 75));
            MousePointerTypes.Add(MousePointerType.SmallCursor, new Rectangle(150, 0, 75, 75));
            MousePointerTypes.Add(MousePointerType.Aim, new Rectangle(225, 0, 75, 75));
            MousePointerTypes.Add(MousePointerType.Pointer, new Rectangle(300, 0, 75, 75));
            MousePointerTypes.Add(MousePointerType.SmallCrosshair, new Rectangle(375, 0, 75, 75));
            MousePointerTypes.Add(MousePointerType.Text, new Rectangle(450, 0, 75, 75));
        }
    }

    public enum MousePointerType
    {
        Arrow,
        Pointer,
        Crosshair,
        Text,
        SmallCursor,
        SmallCrosshair,
        Aim,
        Default
    }
}
