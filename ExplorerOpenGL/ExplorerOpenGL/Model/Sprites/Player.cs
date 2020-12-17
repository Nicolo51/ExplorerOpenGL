using ExplorerOpenGL.Controlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
        public Vector2 Direction;

        public float PlayerFeetRadian { get { return playerFeet.Radian; } }

        public Player(Texture2D texture, Texture2D playerFeetTexture, MousePointer mousepointer)
            : base()
        {
            Direction = new Vector2(0, 0);
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
            Move(controler, sprites);

            base.Update(gameTime, sprites, controler);
        }

        private float CalculateAngle(Vector2 A, Vector2 B)
        {
            Vector2 triangle = B - A;
            float output = ((float)Math.Atan((double)((triangle.Y) / (triangle.X))));
            if (triangle.X < 0)
            {
                output += (float)Math.PI;
            }
            return output;
        }
        private void Move(Controler controler, List<Sprite> sprites)
        {
            Direction = Vector2.Zero;
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

            if (Direction != Vector2.Zero)
            {
                float direction = (float)Math.Atan((double)(Direction.Y / Direction.X));
                if (Direction.X < 0)
                {
                    direction += (float)Math.PI;
                }
                playerFeet.SetDirection(direction);
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];
                    if (sprites[i] is Wall)
                    {
                        Debug.WriteLine(this.Direction);

                        if ((Direction.X > 0 && this.IsTouchingLeft(sprite)) ||
                            (Direction.X < 0 & this.IsTouchingRight(sprite)))
                            Direction.X = 0;

                        if ((Direction.Y > 0 && this.IsTouchingTop(sprite)) ||
                            (Direction.Y < 0 & this.IsTouchingBottom(sprite)))
                            Direction.Y = 0;
                    }
                }
                Position += Direction * Velocity;
                playerFeet.Position = Position;
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            playerFeet.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }

        private bool IsTouchingLeft(Sprite sprite)
        {
            return this.HitBox.Right + this.Velocity * Direction.X > sprite.HitBox.Left &&
              this.HitBox.Left < sprite.HitBox.Left &&
              this.HitBox.Bottom > sprite.HitBox.Top &&
              this.HitBox.Top < sprite.HitBox.Bottom;
        }

        private bool IsTouchingRight(Sprite sprite)
        {
            return this.HitBox.Left + this.Velocity * Direction.X < sprite.HitBox.Right &&
              this.HitBox.Right > sprite.HitBox.Right &&
              this.HitBox.Bottom > sprite.HitBox.Top &&
              this.HitBox.Top < sprite.HitBox.Bottom;
        }

        private bool IsTouchingTop(Sprite sprite)
        {
            return this.HitBox.Bottom + this.Velocity * Direction.Y > sprite.HitBox.Top &&
              this.HitBox.Top < sprite.HitBox.Top &&
              this.HitBox.Right > sprite.HitBox.Left &&
              this.HitBox.Left < sprite.HitBox.Right;
        }

        private bool IsTouchingBottom(Sprite sprite)
        {
            return this.HitBox.Top + this.Velocity * Direction.Y < sprite.HitBox.Bottom &&
              this.HitBox.Bottom > sprite.HitBox.Bottom &&
              this.HitBox.Right > sprite.HitBox.Left &&
              this.HitBox.Left < sprite.HitBox.Right;
        }

        public string GetJSON()
        {
            string output = Position.ToString(); 
            return output;
        }
    }
}
