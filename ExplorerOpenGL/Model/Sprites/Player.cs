using ExplorerOpenGL.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private Texture2D TextureName;
        private Vector2 PositionName { get { return new Vector2(Position.X, Position.Y - 10); } }
        private Vector2 OriginName; 
        public int Health { get; set;  }
        public string Name{ get; private set; }
        public int ID { get; private set; }
        public float PlayerFeetRadian { get { return playerFeet.Radian; } }
        public string CurrentAnimationName { get { if (_animation != null) return _animation.currentAnimation.Name; else return string.Empty; } }

        private TextureManager textureManager;
        private KeyboardManager keyboardManager;
        private NetworkManager networkManager;
        private MouseManager mouseManager;
        private SpriteFont font;

        public Player(string name, params Animation[] animations)
            : base(animations)
        {
            textureManager = TextureManager.Instance;
            keyboardManager = KeyboardManager.Instance;
            networkManager = NetworkManager.Instance;
            mouseManager = MouseManager.Instance; 

            mousePointer = gameManager.MousePointer; 
            ChangeName(name);
            direction = new Vector2(0, 0);
            playerFeet = new PlayerFeet(textureManager.LoadTexture("playerfeet"));
            playerFeet.LayerDepth = .9f;
            Velocity = 3;
            LayerDepth = .9f;
            Scale = 1f;
            Health = 100;
            font = FontManager.Instance.GetFont();
            mouseManager.LeftClicked += FireBullet;
            Radian = 0; 
            //isDraggable = true;
            IsGravityAffected = true;
            isCollidable = true;
            Play("stand"); 
        }

        private void FireBullet(ButtonState buttonState)
        {
            //if (!networkManager.IsConnectedToAServer || buttonState == ButtonState.Released && (gameManager.GameState == GameState.OnlinePlaying || gameManager.GameState == GameState.Playing))
            //    return;
            //networkManager.CreateBullet(this); 
        }

        public override void Remove()
        {
            base.Remove();
            mouseManager.LeftClicked -= FireBullet; 
        }

        public override void Update(Sprite[] sprites)
        {
            //Position = mousePointer.Position;
            playerFeet.Update(sprites);
            //Radian = CalculateAngle(Position, mousePointer.InGamePosition);
            Move(sprites);
            base.Update(sprites);
            SetPosition(Position + direction, false);
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

        protected virtual void Move(Sprite[] sprites)
        {
            direction.X = 0;
            //Direction = Vector2.Zero; 

            //if (keyboardManager.IsKeyDown(input.Down))
            //    Direction.Y = Velocity;
            if (keyboardManager.IsKeyDown(input.Up))
                Jump(); 
            if (keyboardManager.IsKeyDown(input.Right))
                direction.X = Velocity;
            if (keyboardManager.IsKeyDown(input.Left))
                direction.X = -Velocity;

            if (direction != Vector2.Zero)
            {
                //Direction = Vector2.Normalize(Direction); 
                float direction = (float)Math.Atan((double)(base.direction.Y / base.direction.X));
                if (base.direction.X < 0)
                {
                    direction += (float)Math.PI;
                }
                playerFeet.SetDirection(direction);
                if (base.direction.X < 0)
                    Effects = SpriteEffects.FlipHorizontally;
                if (base.direction.X > 0)
                    Effects = SpriteEffects.None;
                
            }
            if (direction.X != 0)
                Play("walk");
            else
                Play("stand");
        }
        private void Jump()
        {
            if (isGrounded)
                direction.Y = -10; 
        }

        public override void SetPosition(Vector2 newPos, bool instant = true)
        {
            playerFeet.SetPosition(newPos, instant);
            base.SetPosition(newPos, instant);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float lerpAmount, Vector2? position = null)
        {
            //playerFeet.Draw(spriteBatch, gameTime, lerpAmount);
            spriteBatch.Draw(TextureName, PositionName , null, Color.White, 0f, OriginName, .75f, SpriteEffects.None, LayerDepth);
            //spriteBatch.DrawString(font, Health.ToString("#"), Position - new Vector2(0,50), Color.White, 0f, font.MeasureString(Health.ToString("#")) / 2, 1f, SpriteEffects.None, layerDepth);
            base.Draw(spriteBatch, gameTime, lerpAmount);
        }



        public string GetJSON()
        {
            string output = Position.ToString(); 
            return output;
        }
    }
}
