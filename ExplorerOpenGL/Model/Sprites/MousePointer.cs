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


        public MousePointer(Texture2D texture)
            : base(texture)
        {
            defaultType = MousePointerType.Arrow;
            _texture = texture;
            SourceRectangle = new Rectangle(300, 0, 75, 75);
            scale = .5f;
            layerDepth = 0f;
            InitMouseTypes();
            SetCursorIcon(MousePointerType.Arrow);
            IsHUD = true;
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
                case MousePointerType.Arrow: case MousePointerType.SmallCursor:
                    origin = Vector2.Zero;
                    break;
                case MousePointerType.Crosshair:case MousePointerType.Aim:case MousePointerType.SmallCrosshair:case MousePointerType.Text:
                    origin = new Vector2(SourceRectangle.Width /2, SourceRectangle.Height/2);
                    break;
                case MousePointerType.Pointer:
                    origin = new Vector2(SourceRectangle.Width / 2, 0);
                    break;
            }
        }

        public override void Update(Sprite[] sprites)
        {
            base.Update(sprites);
            if(currentMouseState != null)
            {
                prevMouseState = currentMouseState; 
            }
            currentMouseState = Mouse.GetState();
            SetPosition(new Vector2(currentMouseState.Position.X, currentMouseState.Position.Y), false);

            InGamePosition = new Vector2((gameManager.Camera.Position.X - gameManager.Camera.Bounds.X / 2 + currentMouseState.Position.X), (gameManager.Camera.Position.Y - gameManager.Camera.Bounds.Y / 2 + currentMouseState.Position.Y)); 

        }
        public override void Draw(SpriteBatch spriteBatch, float lerpAmount)
        {
            //spriteBatch.Draw(_texture, Position, SourceRectangle, Color.White * Opacity, Radian, origin, scale, Effects, layerDepth);
            base.Draw(spriteBatch, lerpAmount);
        }

        public override string ToString()
        {
            return "Position as HUD : " + Position.X + " / "+ Position.Y + "\nPosition as NonHUD : " + InGamePosition.X + " / " + InGamePosition.Y;
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
