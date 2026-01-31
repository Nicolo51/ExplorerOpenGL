using ExplorerOpenGL2.Managers;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Model.Sprites
{
    public class Player : Sprite
    {
        private int height; 
        private int width;
        private MousePointer mousePointer;
        public Input input;
        private Texture2D TextureName;
        private Vector2 PositionName { get { return new Vector2(Position.X, Position.Y - 10); } }
        private Vector2 OriginName;

        public bool IsNetContolled { get; private set; }

        public int Health { get; set;  }
        public string Name{ get; private set; }
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
            Velocity = 3;
            LayerDepth = .9f;
            Scale = 1f;
            Health = 100;
            font = FontManager.Instance.GetFont();
            mouseManager.LeftClicked += FireBullet;
            Radian = 0; 

            isDraggable = true;
            IsGravityAffected = true; 
            isCollidable = true;
            IsPartOfGameState = true;

            Play("idle");
            //Shader = shaderManager.LoadShader("Outline");
            //height = 100;
            //width = 40;
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

        public override void Update(List<Sprite> sprites, GameTime gametime, NetGameState netGameState)
        {
            //Position = mousePointer.Position;
            float lerp = (float)(gametime.ElapsedGameTime.TotalMilliseconds / 16);
            //Radian = CalculateAngle(Position, mousePointer.InGamePosition);

            Move(sprites, lerp);
            base.Update(sprites, gametime, netGameState);
            SetPosition(Position + direction * lerp, false);
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

        protected virtual void Move(List<Sprite> sprites, float lerp)
        {
            if (input == null)
                return; 
            direction.X = 0;

            if (keyboardManager.IsKeyDown(input.Up))
                Jump();
            if (keyboardManager.IsKeyDown(input.Right))
                direction.X = Velocity;
            if (keyboardManager.IsKeyDown(input.Left))
                direction.X = -Velocity;

            if (keyboardManager.IsKeyDown(input.Run))
                direction.X *= 2;

            if (direction != Vector2.Zero)
            {
                float direction = (float)Math.Atan((double)(base.direction.Y / base.direction.X));
                if (base.direction.X < 0)
                {
                    direction += (float)Math.PI;
                }
                if (base.direction.X < 0)
                    Effect = SpriteEffects.FlipHorizontally;
                if (base.direction.X > 0)
                    Effect = SpriteEffects.None;
                
            }

            if (direction.Y > 0)
                Play("falling"); 

            if (!isGrounded)
                return;
            if (direction.X != 0) {
                if (Math.Abs(direction.X) > 4)
                    Play("run");
                else
                    Play("walk");
            }
            else
                Play("idle");
        }
        private void Jump()
        {
            if (isGrounded && direction.Y == 0f)
            {
                debugManager.AddEvent(direction.Y);
                Play("idle");
                Play("jump");
                isGrounded = false; 
                direction.Y = -5;
            }
        }

        public override void SetPosition(Vector2 newPos, bool instant = true)
        {
            base.SetPosition(newPos, instant);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float lerpAmount, params ShaderArgument[] shaderArgs)
        {
            base.Draw(spriteBatch, gameTime, lerpAmount);
            spriteBatch.Draw(TextureName, PositionName , null, Color.White, 0f, OriginName, .75f, SpriteEffects.None, LayerDepth);
            //playerFeet.Draw(spriteBatch, gameTime, lerpAmount);
            //spriteBatch.DrawString(font, Health.ToString("#"), Position - new Vector2(0,50), Color.White, 0f, font.MeasureString(Health.ToString("#")) / 2, 1f, SpriteEffects.None, layerDepth);
        }

        public override NetDataWriter WriteToNet(NetGameState netGameState, Type type)
        {
            NetDataWriter data = base.WriteToNet(netGameState, this.GetType());
            data.Put(Animation.currentAnimation.Name);
            data.Put(Name);
            return data; 
        }

        public override void ReadGameState(NetDataReader r)
        {
            base.ReadGameState(r);
            string animation = r.GetString();
            string name = r.GetString();
            Play(animation);
            if(Name != name)
                ChangeName(name);
        }

        public string GetJSON()
        {
            string output = Position.ToString(); 
            return output;
        }
    }
}