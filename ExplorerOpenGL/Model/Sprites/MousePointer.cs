using ExplorerOpenGL.Controlers;
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
        public Vector2 InWindowPosition { get; private set; }
            

        public MousePointer(Texture2D texture)
            : base(texture)
        {
            _texture = texture;
            SourceRectangle = new Rectangle(300, 0, 75, 75);
            scale = .5f;
            layerDepth = 0f;
            InitMouseTypes();
            SetCursorIcon(MousePointerType.Default);
        }
        public MousePointer()
        {
        }

        public void SetCursorIcon(MousePointerType type)
        {
            SourceRectangle = MousePointerTypes[type];
            switch (type)
            {
                case MousePointerType.Default: case MousePointerType.SmallCursor:
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

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            if(currentMouseState != null)
            {
                prevMouseState = currentMouseState; 
            }
            currentMouseState = Mouse.GetState();
            InWindowPosition = new Vector2(currentMouseState.Position.X, currentMouseState.Position.Y);
            Position = new Vector2((controler.Camera.Position.X - controler.Camera.Bounds.X / 2 + currentMouseState.Position.X), (controler.Camera.Position.Y - controler.Camera.Bounds.Y / 2 + currentMouseState.Position.Y)); 

            base.Update(gameTime, sprites, controler);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, SourceRectangle, Color.White * Opacity, Radian, origin, scale, Effects, layerDepth);
            base.Draw(spriteBatch);
        }

        public override string ToString()
        {
            return "InWindowPos : " + InWindowPosition.X + " / "+ InWindowPosition.Y + "\nInGamePos : " + Position.X + " / " + Position.Y;
        }
        private void InitMouseTypes()
        {
            MousePointerTypes = new Dictionary<MousePointerType, Rectangle>();
            MousePointerTypes.Add(MousePointerType.Default, new Rectangle(0, 0, 75, 75));
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
        Default,
        Pointer,
        Crosshair,
        Text,
        SmallCursor,
        SmallCrosshair,
        Aim,
    }
}
