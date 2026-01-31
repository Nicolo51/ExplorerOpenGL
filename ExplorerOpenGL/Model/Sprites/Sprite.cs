using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Managers.Networking;
using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Attributes;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExplorerOpenGL2.Model.Sprites
{
    public class Sprite
    {
        public int MyProperty { get; set; }
        private Vector2 LastDrawnPos; 
        public bool IsRemove { get; set; }
        public int ID { get; set; } 
        public Vector2 Position { get; set; }
        [MapEditable]
        public float PositionX { get { return Position.X; } set { Position = new Vector2(value, Position.X); } }
        [MapEditable]
        public float PositionY { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
        public Vector2 LastPosition { get; set; }

        private Texture2D _texture;
        public Texture2D Texture { get { return _texture ?? _animation.Texture; } }
        public Effect Shader { get; set; }
        public ShaderArgument[] ShaderArgs { get; private set; }

        public AnimationManager Animation { get { return _animation; } }
        protected AnimationManager _animation;
        [MapEditable]
        public float Radian { get; set; }
        protected Vector2 direction;
        public Vector2 Direction { get { return direction; } }
        public float Velocity { get; set; }

        public Rectangle SourceRectangle { get; set; }
        public virtual Rectangle HitBox { get {
                if (_texture != null && SourceRectangle != null) 
                    return new Rectangle(
                        (int)Position.X - (int)(Origin.X * Scale), 
                        (int)Position.Y - (int)(Origin.Y * Scale), 
                        (int)(Bounds.X * Scale), 
                        (int)(Bounds.Y * Scale));

                else if (_animation != null && _animation.currentAnimation != null) 
                    return new Rectangle(
                        (int)Position.X - (int)(Origin.X * Scale), 
                        (int)Position.Y - (int)(Origin.Y * Scale), 
                        (int)(_animation.currentAnimation.Bounds.X * Scale), 
                        (int)(_animation.currentAnimation.Bounds.Y * Scale)
                        );

                else 
                    return new Rectangle((int)Position.X, (int)Position.Y, 1, 1);
            }
        }
        public virtual Vector2 Origin { get { return origin * Scale; } }
        protected Vector2 origin;
        protected AlignOptions alignOption;
        public AlignOptions AlignOption { get { if (_animation != null && _animation.Count > 0) return _animation.currentAnimation.AlignOption; else return alignOption; } set { alignOption = value; } }
        public Vector2 Bounds { get; set; }
        [MapEditable]
        public float BoundsX { get { return Bounds.X; } set { Bounds = new Vector2(value, Bounds.Y); } }
        [MapEditable]
        public float BoundsY { get { return Bounds.Y; } set { Bounds = new Vector2(Bounds.X, value); } }
        public Rectangle DestinationRectangle { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)Bounds.X, (int)Bounds.Y); } }
        public SpriteEffects Effect { get; set; }
        [MapEditable]
        public float LayerDepth { get; set; }
        [MapEditable]
        public float Scale { get; set; }
        [MapEditable]
        public float Opacity { get; set; }
        [MapEditable]
        public float Gravity { get; set; }
        public bool IsHUD { get; set; }
        public bool IsDisplayed { get; set; }
        public bool IsEnable { get; set; }
        public bool IsClickable { get; set; }
        public bool IsPartOfGameState { get; set; }
        public bool isDraggable { get; set; }
        public TextZone TextOnTop { get; set; }

        protected bool isGrounded;
        protected bool IsGravityAffected;
        protected bool isCollidable;
        protected bool isMovingTowardPosition;
        protected bool gameStateForced; 

        public delegate void MouseOverEventHandler(object sender, MousePointer mousePointer);
        public event MouseOverEventHandler MouseOvered;

        public delegate void MouseLeaveEventHandler(object sender, MousePointer mousePointer);
        public event MouseLeaveEventHandler MouseLeft;

        public delegate void MouseClickEventHandler(object sender, MousePointer mousePointer, Vector2 clickPosition);
        public event MouseClickEventHandler MouseClicked;

        protected GameManager gameManager;
        protected DebugManager debugManager;
        protected ShaderManager shaderManager;
        protected RenderManager renderManager;
        public Sprite()
        {
            Init();
        }

        public Sprite(params Animation[] animations)
        {
            
            Init();
            if (animations.Length < 1)
                return;
            foreach (var a in animations)
                _animation.Add(a.Name, a);
            Play(animations[0]);
            Bounds = _animation.GetBounds(); 
        }

        public Sprite(Texture2D texture)
        {
            Init();
            SetTexture(texture);
            Bounds = new Vector2(texture.Bounds.Width, texture.Bounds.Height);
        }

        public Sprite(params Texture2D[] texture)
        {
            Init();
        }
        
        public virtual void Update(List<Sprite> sprites, GameTime gametime, NetGameState netGameState)
        {
            double lerp = gametime.ElapsedGameTime.TotalMilliseconds / 16; 
            if (!IsDisplayed)
                return;
            LastPosition = Position;
            
            if (IsGravityAffected)
                ComputeGravity(sprites, lerp);

            if (isCollidable && Direction != Vector2.Zero)
                CheckCollision(sprites);

            if(IsPartOfGameState)
                WriteToNet(netGameState, this.GetType()); 
        }

        public virtual void ComputeGravity(List<Sprite> sprites, double lerp)
        {
            direction.Y += (float)(Gravity * lerp);
        }

        public virtual void CheckCollision(List<Sprite> sprites)
        {
            isGrounded = false;
            
            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                if (sprite == this)
                    continue;
                if (sprites[i].isCollidable)
                {
                    
                    if (direction.Y != 0)
                    {
                        if (this.IsTouchingTop(sprite) && direction.Y > 0)
                        {
                            direction.Y = sprite.HitBox.Top - this.HitBox.Bottom; 
                            if(direction.Y < -5)
                                debugManager.AddEventToTerminal("chelou");
                        }
                        if (direction.Y < 0 & this.IsTouchingBottom(sprite))
                        {
                            direction.Y = sprite.HitBox.Bottom - this.HitBox.Top;
                        }
                    }
                    if (direction.X != 0)
                    {
                        if ((direction.X > 0 && this.IsTouchingLeft(sprite)) ||
                        (direction.X < 0 & this.IsTouchingRight(sprite)))
                        {
                            int heightDiff = this.HitBox.Bottom - sprite.HitBox.Top;
                            if (heightDiff < 10)
                                PositionY += -heightDiff;
                            else
                                direction.X = 0;
                        }
                    }
                }
            }
        }

        public virtual void SetTexture(Texture2D texture)
        {
            _texture = texture;
            Bounds = new Vector2(texture.Bounds.Width, texture.Bounds.Height);
            SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public virtual void SetShaderArgs(ShaderArgument[] args)
        {
            ShaderArgs = args; 
        }

        public void AddAfterAnimation(Animation animation, Animation afterAnimation)
        {
            _animation.PlayAfterAnimation(animation, afterAnimation);
        }
        public void AddAfterAnimation(string animationName, string afterAnimationName)
        {
            _animation.PlayAfterAnimation(animationName, afterAnimationName);
        }

        public virtual void Remove()
        {

            if (MouseClicked != null)
            {
                foreach (Delegate d in MouseClicked?.GetInvocationList())
                    MouseClicked -= (MouseClickEventHandler)d;
            }
            if (MouseLeft != null)
            {
                foreach (Delegate d in MouseLeft?.GetInvocationList())
                    MouseLeft -= (MouseLeaveEventHandler)d;
            }
            if (MouseOvered != null) 
            {
                foreach (Delegate d in MouseOvered?.GetInvocationList())
                    MouseOvered -= (MouseOverEventHandler)d;
            } 
            IsRemove = true; 
        }

        public virtual void SetPosition(Vector2 newPos, bool instant = true)
        {
            if (instant)
                LastPosition = LastDrawnPos;
            Position = newPos;
        }

        public virtual void SetPosition(float x, float y, bool instant = true)
        {
            SetPosition(new Vector2(x, y), instant); 
        }

        public virtual void MoveTo(Vector2 newPos, float time) // time in ms
        {

        }

        public void ForcedGameState()
        {
            gameStateForced = true;
        }

        public void ReleaseForcedGameState()
        {
            gameStateForced = false;
        }

        public virtual void SetAlignOption(AlignOptions ao)
        {
            //if (ao == alignOption)
            //    return;
            Vector2 bounds = new Vector2(HitBox.Width , HitBox.Height);
            switch (ao)
            {
                case AlignOptions.Left:
                    origin = new Vector2(0, bounds.Y / 2);
                    break;
                case AlignOptions.Right:
                    origin = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOptions.Top:
                    origin = new Vector2(bounds.X / 2, 0);
                    break;
                case AlignOptions.Bottom:
                    origin = new Vector2(bounds.X / 2, bounds.Y);
                    break;
                case AlignOptions.BottomLeft:
                    origin = new Vector2(0, bounds.Y);
                    break;
                case AlignOptions.BottomRight:
                    origin = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOptions.TopLeft:
                    origin = new Vector2(0, 0);
                    break;
                case AlignOptions.TopRight:
                    origin = new Vector2(bounds.X, 0);
                    break;
                case AlignOptions.Center:
                    origin = new Vector2(bounds.X / 2, bounds.Y / 2);
                    break;
            }
            alignOption = ao;
        }
        public void Play(string animationName)
        {
            if (_animation == null || string.IsNullOrWhiteSpace(animationName))
                return;
            _animation.Play(animationName);
            Bounds = _animation.currentAnimation.Bounds; 
            if(animationName != _animation.currentAnimation.Name)
                SetAlignOption(_animation.currentAnimation.AlignOption); 
        }

        public void Play(Animation animation)
        {
            Play(animation.Name);
        }

        public void SetPadding(Vector2 pad)
        {
            SetAlignOption(alignOption);
            origin += pad;
        }

        public virtual void OnMouseOver(List<Sprite> sprites, MousePointer mousePointer)
        {
            MouseOvered?.Invoke(this, mousePointer);
            debugManager.AddEvent("Enter");
        }

        public virtual void OnMouseLeave(List<Sprite> sprites, MousePointer mousePointer)
        {
            MouseLeft?.Invoke(this, mousePointer);
            debugManager.AddEvent("Left");
        }

        public virtual void OnMouseClick(List<Sprite> sprites, Vector2 clickPosition, MousePointer mousePointer)
        {
            MouseClicked?.Invoke(this, mousePointer, clickPosition);
        }

        public virtual void Draw(SpriteBatch spriteBatch,  GameTime gameTime, float lerpAmount, params ShaderArgument[] shaderArgs)
        {
            //Vector2 OSPos = Vector2.Lerp(LastPosition, Position, lerpAmount > 1 ? 1: lerpAmount);
            LastDrawnPos = Position; 
            if ((_texture != null || _animation.currentAnimation != null) && IsDisplayed)
            {
                if (_animation.currentAnimation != null)
                    SourceRectangle = _animation.GetRectangle(gameTime);
                
                //shaderManager.Apply(this, Shader, ShaderArgs);
                //shaderManager.GetDefaultShader().CurrentTechnique.Passes[0].Apply();
                if(_texture != null)
                    spriteBatch.Draw(_texture, new Rectangle((int)Position.X, (int)Position.Y, (int)(Bounds.X * Scale), (int)(Bounds.Y * Scale)), SourceRectangle, Color.White * Opacity, Radian, Origin, Effect, LayerDepth);
                if(_animation.currentAnimation != null)
                    spriteBatch.Draw(_animation.Texture, new Rectangle((int)Position.X, (int)Position.Y, SourceRectangle.Width, SourceRectangle.Height), SourceRectangle, Color.White * Opacity, Radian, Origin, Effect, LayerDepth);
            }

            if (TextOnTop != null)
                renderManager.DrawString(TextOnTop.Font, TextOnTop.Text, Position, TextOnTop.Color, Radian, Origin - new Vector2(HitBox.Width / 2, HitBox.Height / 2) + TextOnTop.Origin, TextOnTop.Scale, TextOnTop.Effect, LayerDepth - .1f);
            if (debugManager.IsDebuging)
                spriteBatch.Draw(debugManager.debugTexture, HitBox, Color.White * .5f);
        }

        public bool IsTouchingLeft(Sprite sprite)
        {
            return this.HitBox.Right + this.Velocity * direction.X > sprite.HitBox.Left &&
              this.HitBox.Left < sprite.HitBox.Left &&
              this.HitBox.Bottom > sprite.HitBox.Top &&
              this.HitBox.Top < sprite.HitBox.Bottom;
        }

        public bool IsTouchingRight(Sprite sprite)
        {
            return this.HitBox.Left + this.Velocity * direction.X < sprite.HitBox.Right &&
              this.HitBox.Right > sprite.HitBox.Right &&
              this.HitBox.Bottom > sprite.HitBox.Top &&
              this.HitBox.Top < sprite.HitBox.Bottom;
        }

        public bool IsTouchingTop(Sprite sprite)
        {
            bool output =  this.HitBox.Bottom + direction.Y > sprite.HitBox.Top &&
              this.HitBox.Top < sprite.HitBox.Top &&
              this.HitBox.Right > sprite.HitBox.Left &&
              this.HitBox.Left < sprite.HitBox.Right;
            if(!isGrounded)
                isGrounded = output;
            
            return output;
        }

        public bool IsTouchingBottom(Sprite sprite)
        {
            return this.HitBox.Top + direction.Y < sprite.HitBox.Bottom &&
              this.HitBox.Bottom > sprite.HitBox.Bottom &&
              this.HitBox.Right > sprite.HitBox.Left &&
              this.HitBox.Left < sprite.HitBox.Right;
        }

        public void Init()
        {
            _animation = new AnimationManager();
            alignOption = AlignOptions.None;
            isDraggable = false;
            IsDisplayed = true;
            IsEnable = true; 
            IsHUD = false;
            gameStateForced = false;
            Scale = 1;
            Opacity = 1f;
            LayerDepth = 1f;
            debugManager = DebugManager.Instance;
            gameManager = GameManager.Instance;
            shaderManager = ShaderManager.Instance;
            renderManager = RenderManager.Instance; 
            Gravity = 0.2f;
            Shader = shaderManager.LoadShader("Normal");
            IsPartOfGameState = false;
        }

        public Sprite Clone()
        {
            return (Sprite)this.MemberwiseClone(); 
        }

        public virtual NetDataWriter WriteToNet(NetGameState gameState, Type type)
        {
            NetDataWriter data = gameState.GetDataWriter();

            /*read on event handler*/
            data.Put(gameManager.SpriteTypeToId[type]); 
            data.Put(ID);
            data.Put(gameStateForced);
            /*end of read on event handler*/

            data.Put(PositionX);
            data.Put(PositionY);
            data.Put(direction.X);
            data.Put(direction.Y);
            data.Put(Velocity);
            data.Put((int)Effect);
            return data; 
        }

        public virtual void ReadGameState(NetDataReader r)
        {
            float positionX = r.GetFloat(); 
            float positionY = r.GetFloat(); 
            float directionX = r.GetFloat();
            float directionY = r.GetFloat();
            float velocity = r.GetFloat();
            int effect = r.GetInt();

            Effect = (SpriteEffects)effect; 
            Position = new Vector2(positionX, positionY);
            direction = new Vector2(directionX, directionY);

        }
    }

    

    public enum AlignOptions 
    {
        None,
        Left,
        Right,
        Center,
        Top,
        Bottom,
        BottomRight,
        BottomLeft,
        TopRight,
        TopLeft, 
    }

}
