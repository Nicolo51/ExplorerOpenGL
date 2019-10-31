using ExplorerOpenGL.Controlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Player : Sprite 
    {
        private MousePointer mousePointer;
        public Input input;
        private PlayerFeet playerFeet;
        private float Velocity; 

        public Player(Texture2D texture, Texture2D playerFeetTexture, MousePointer mousepointer)
            :base ()
        {
            playerFeet = new PlayerFeet(playerFeetTexture); 
            this.mousePointer = mousepointer;
            _texture = texture;
            origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Velocity = 5;
            scale = .5f;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            Radian = CalculateAngle(Position, mousePointer.Position);
            //Radian = (float)Math.PI; 
            Move(controler); 
            
            base.Update(gameTime, sprites, controler);
        }

        private float CalculateAngle(Vector2 A, Vector2 B)
        {
            Vector2 triangle = B - A; 
            float output = ((float)Math.Atan((double)((triangle.Y) / (triangle.X))));
            if(triangle.X < 0)
            {
                output += (float)Math.PI; 
            }
            Debug.WriteLine(triangle);
            return output; 
        }
        private void Move(Controler controler)
        {
            Vector2 Direction = new Vector2();
            if (controler.KeyboardUtils.IsKeyDown(input.Down))
            {
                Direction.Y += 1;
            }
            if (controler.KeyboardUtils.IsKeyDown(input.Up))
            {
                Direction.Y -= 1;
            }
            if (controler.KeyboardUtils.IsKeyDown(input.Right))
            {
                Direction.X += 1; 
            }
            if (controler.KeyboardUtils.IsKeyDown(input.Left))
            {
                Direction.X -= 1;
            }
            float direction = (float)Math.Atan((double)(Direction.Y / Direction.X)); 
            if(Direction != Vector2.Zero)
            {
                if(Direction.X < 0)
                {
                    direction += (float)Math.PI;
                }
                playerFeet.SetDirection(direction);
            }

            Position += Direction * Velocity;
            playerFeet.Position = Position; 
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            playerFeet.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }
    }
}
