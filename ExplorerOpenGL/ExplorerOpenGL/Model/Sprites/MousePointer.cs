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
        private Camera _camera;
        public Vector2 InWindowPosition { get; private set; }
            

        public MousePointer(Texture2D texture, Camera camera)
            : base()
        {
            _camera = camera; 
            _texture = texture;
        }
        public MousePointer(Camera camera)
        {
            _camera = camera; 
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            if(currentMouseState != null)
            {
                prevMouseState = currentMouseState; 
            }
            currentMouseState = Mouse.GetState();
            InWindowPosition = new Vector2(currentMouseState.Position.X, currentMouseState.Position.Y);
            Position = new Vector2((_camera.Position.X - _camera.Bounds.X / 2 + currentMouseState.Position.X), (_camera.Position.Y - _camera.Bounds.Y / 2 + currentMouseState.Position.Y)); 

            base.Update(gameTime, sprites, controler);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, InWindowPosition, null, Color.White * opacity, Radian, origin, scale, Effects, layerDepth);
            base.Draw(spriteBatch);
        }

        public override string ToString()
        {
            return "Position X : " + Position.X + " / "+ InWindowPosition.X + "\nPosition Y : " + Position.Y + " / " + InWindowPosition.Y;
        }
    }
}
