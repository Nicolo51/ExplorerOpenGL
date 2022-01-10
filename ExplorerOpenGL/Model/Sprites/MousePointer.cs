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
        }
        public MousePointer()
        {
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
    }
}
