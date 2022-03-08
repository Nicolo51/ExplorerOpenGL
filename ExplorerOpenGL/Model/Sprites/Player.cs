using ExplorerOpenGL.Managers;
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
        private Texture2D TextureName;
        private Vector2 PositionName;
        private Vector2 OriginName; 
        public Vector2 Direction;
        public long Health { get; private set;  }
        public string Name{ get; private set; }
        public int ID { get; private set; }
        public float PlayerFeetRadian { get { return playerFeet.Radian; } }

        private TextureManager textureManager;
        private KeyboardManager keyboardManager; 

        public Player(Texture2D texture, Texture2D playerFeetTexture, string name)
            : base(texture)
        {
            this.textureManager = TextureManager.Instance;
            mousePointer = gameManager.MousePointer; 
            ChangeName(name);
            Direction = new Vector2(0, 0);
            playerFeet = new PlayerFeet(playerFeetTexture);
            playerFeet.layerDepth = .9f;
            origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Velocity = 5;
            layerDepth = .9f;
            scale = .5f;
            Health = 100;
            keyboardManager = KeyboardManager.Instance; 
        }

        public override void Update(List<Sprite> sprites)
        {
            Radian = CalculateAngle(Position, mousePointer.InWindowPosition);
            Move(sprites);
            PositionName = new Vector2(Position.X, Position.Y + 50);
            base.Update(sprites);
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

        public void ChangeName(object name)
        {
            TextureName = textureManager.OutlineText((name as string), "Default", Color.Black, Color.White, 2); 
            Name = (name as string);
            OriginName = new Vector2(TextureName.Width / 2, TextureName.Height / 2);
        }

        protected virtual void Move(List<Sprite> sprites)
        {
            Direction = Vector2.Zero;
            if (!keyboardManager.IsTextInputBoxFocused)
            {
                if (keyboardManager.IsKeyDown(input.Down))
                {
                    Direction.Y += 1;
                }
                if (keyboardManager.IsKeyDown(input.Up))
                {
                    Direction.Y -= 1;
                }
                if (keyboardManager.IsKeyDown(input.Right))
                {
                    Direction.X += 1;
                }
                if (keyboardManager.IsKeyDown(input.Left))
                {
                    Direction.X -= 1;
                }
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
            spriteBatch.Draw(TextureName, PositionName , null, Color.White, 0f, OriginName, .75f, SpriteEffects.None, layerDepth); 
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
