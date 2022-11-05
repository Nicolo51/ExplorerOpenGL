using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Managers.Networking;
using ExplorerOpenGL.Managers.Networking.NetworkObject;
using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Sprite 
    {
        
        public bool IsRemove { get; set; }
        public int ID { get; set; }
        public Vector2 Position { get; set; }
        [MapEditable]
        public float PositionX { get { return Position.X; } set { Position = new Vector2(value, PositionY); } }
        [MapEditable]
        public float PositionY{ get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
        public Vector2 LastPosition { get; set; }

        private Texture2D _texture;
        public Texture2D Texture { get { return _texture; } }
        public Effect Shader { get; set; }


        public AnimationManager Animation { get { return _animation; } }
        protected AnimationManager _animation;
        [MapEditable]
        public float Radian { get; set; }
        protected Vector2 direction;
        public Vector2 Direction { get { return direction; } }
        public float Velocity { get; set; }

        public Rectangle SourceRectangle { get; set; }
        public virtual Rectangle HitBox { get {
                if (_texture != null && SourceRectangle != null) return new Rectangle((int)Position.X - (int)(Origin.X * Scale), (int)Position.Y - (int)(Origin.Y * Scale), (int)(SourceRectangle.Width * Scale), (int)(SourceRectangle.Height * Scale));
                else if (_animation != null && _animation.currentAnimation != null) return new Rectangle((int)Position.X - (int)(Origin.X * Scale), (int)Position.Y - (int)(Origin.Y * Scale), (int)(_animation.currentAnimation.Bounds.X * Scale), (int)(_animation.currentAnimation.Bounds.Y * Scale));
                else return new Rectangle((int)Position.X, (int)Position.Y, 1, 1);
            } }
        public Vector2 Origin { get { return origin * Scale;  } }
        protected Vector2 origin;
        protected AlignOptions alignOption; 
        public AlignOptions AlignOption { get { if (_animation != null && _animation.Count > 0) return _animation.currentAnimation.AlignOption; else return alignOption; } set { alignOption = value; } }
        public Vector2 Bounds { get; set; }
        [MapEditable]
        public float BoundsX { get { return Bounds.X; } set { Bounds = new Vector2(value, Bounds.Y); } }
        [MapEditable]
        public float BoundsY { get { return Bounds.Y; } set { Bounds = new Vector2(Bounds.X, value); } }
        public Rectangle DestinationRectangle { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)Bounds.X, (int)Bounds.Y); } }
        public SpriteEffects Effects { get; set; }
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
        public bool IsClickable { get; set; }
        public bool isDraggable { get; set; }
        public TextZone TextOnTop { get; set; }

        protected bool isGrounded;
        protected bool IsGravityAffected;
        protected bool isCollidable;
        protected bool isMovingTowardPosition; 

        public delegate void MouseOverEventHandler(object sender, MousePointer mousePointer);
        public event MouseOverEventHandler MouseOvered;

        public delegate void MouseLeaveEventHandler(object sender, MousePointer mousePointer);
        public event MouseLeaveEventHandler MouseLeft;

        public delegate void MouseClickEventHandler(object sender, MousePointer mousePointer, Vector2 clickPosition);
        public event MouseClickEventHandler MouseClicked;

        protected GameManager gameManager;
        protected TimeManager timeManager;
        protected DebugManager debugManager;
        protected ShaderManager shaderManager; 

        Vector2 ClickPosition;

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
            Bounds = new Vector2(_animation.currentAnimation.Bounds.X, _animation.currentAnimation.Bounds.Y);
        }

        public Sprite(Texture2D texture)
        {
            Init();
            SetTexture(texture);
            Bounds = new Vector2(texture.Bounds.Width, texture.Bounds.Height); 
        }

        public virtual void NetworkUpdate(NetworkGameObject ngo)
        {
            if (ID != ngo.ID)
                return;
            SetPosition(ngo.Position, false);
            if (ngo.IsRemove)
                Remove();
            Radian = ngo.Direction;
        }

        public virtual void Update(Sprite[] sprites)
        {
            if (!IsDisplayed)
                return;
            LastPosition = Position;
            if (IsGravityAffected)
                ComputeGravity(sprites);
            if (isCollidable)
                CheckCollision(sprites);
        }

        public virtual void ComputeGravity(Sprite[] sprites)
        {
            direction.Y += Gravity;
        }

        public virtual void CheckCollision(Sprite[] sprites)
        {
            isGrounded = false;
            for (int i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                if (sprite == this)
                    continue;
                if (sprites[i].isCollidable)
                {
                    if ((direction.X > 0 && this.IsTouchingLeft(sprite)) ||
                        (direction.X < 0 & this.IsTouchingRight(sprite)))
                        direction.X = 0;

                    if ((this.IsTouchingTop(sprite) && direction.Y > 0) ||
                        (direction.Y < 0 & this.IsTouchingBottom(sprite)))
                        direction.Y = 0;
                }
            }
        }

        public virtual void SetTexture(Texture2D texture)
        {
            _texture = texture;
            Bounds = new Vector2(texture.Bounds.Width, texture.Bounds.Height); 
            SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
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
                LastPosition = newPos;
            Position = newPos;
        }

        public virtual void SetPosition(float x, float y, bool instant = true)
        {
            SetPosition(new Vector2(x, y), instant); 
        }

        public virtual void MoveTo(Vector2 newPos, float time) // time in ms
        {

        }

        public virtual void SetAlignOption(AlignOptions ao)
        {
            if (ao == alignOption)
                return;
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

        public virtual void OnMouseOver(Sprite[] sprites, MousePointer mousePointer)
        {
            MouseOvered?.Invoke(this, mousePointer);
            debugManager.AddEvent("Enter");
        }

        public virtual void OnMouseLeave(Sprite[] sprites, MousePointer mousePointer)
        {
            MouseLeft?.Invoke(this, mousePointer);
            debugManager.AddEvent("Left");
        }

        public virtual void OnMouseClick(Sprite[] sprites, Vector2 clickPosition, MousePointer mousePointer)
        {
            MouseClicked?.Invoke(this, mousePointer, clickPosition);
        }

        public virtual void Draw(SpriteBatch spriteBatch,  GameTime gameTime, float lerpAmount, Vector2? position = null)
        {
            Vector2 OSPos = Vector2.Lerp(LastPosition, position ?? Position, lerpAmount); 
            if ((_texture != null || _animation.currentAnimation != null) && IsDisplayed)
            {
                if (_animation.currentAnimation != null)
                    SourceRectangle = _animation.GetRectangle(gameTime);
                if (this is MessageBox)
                    spriteBatch.Draw(_texture, new Rectangle((int)OSPos.X, (int)OSPos.Y, (int)(Bounds.X * Scale), (int)(Bounds.Y * Scale)), SourceRectangle, Color.White * Opacity, Radian, Origin, Effects, LayerDepth);
                else
                    spriteBatch.Draw(_texture ?? _animation.Texture, new Rectangle((int)OSPos.X, (int)OSPos.Y, (int)(Bounds.X * Scale), (int)(Bounds.Y * Scale)), SourceRectangle, Color.White * Opacity, Radian, Origin, Effects, LayerDepth);
            }
            if (TextOnTop != null)
                spriteBatch.DrawString(TextOnTop.Font, TextOnTop.Text, OSPos, TextOnTop.Color, Radian, Origin - new Vector2(HitBox.Width / 2, HitBox.Height / 2) + TextOnTop.Origin, TextOnTop.Scale, TextOnTop.Effects, LayerDepth-.1f);
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
            IsHUD = false;
            Scale = 1;
            Opacity = 1f;
            LayerDepth = 1f;
            debugManager = DebugManager.Instance;
            gameManager = GameManager.Instance;
            timeManager = TimeManager.Instance;
            shaderManager = ShaderManager.Instance; 
            Gravity = 0.2f;
            Shader = shaderManager.LoadShader("OutlineShader");
        }

        public Sprite Clone()
        {
            return (Sprite)this.MemberwiseClone(); 
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
