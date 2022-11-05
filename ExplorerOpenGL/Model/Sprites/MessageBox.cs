using ExplorerOpenGL.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class MessageBox : Sprite
    {
        public string Title { get; set; }
        
        protected List<Sprite> childSprites;
        protected List<Vector2> childSpritesPosition; 
        protected TextureManager textureManager;
        protected FontManager fontManager;
        protected Texture2D borderTexture;
        
        public delegate void MessageBoxResultClickEventHandler(MessageBox sender, MessageBoxResultEventArgs e);
        public event MessageBoxResultClickEventHandler Result;

        public Rectangle TitleBarHitBox { get 
            {
                if (Texture != null && SourceRectangle != null) return new Rectangle((int)Position.X - (int)(Origin.X * Scale), (int)Position.Y - (int)(Origin.Y * Scale), (int)(SourceRectangle.Width * Scale), (int)(35 * Scale));
                else if (_animation != null && _animation.currentAnimation != null) return new Rectangle((int)Position.X - (int)(Origin.X * Scale), (int)Position.Y - (int)(Origin.Y * Scale), (int)(_animation.currentAnimation.Bounds.X * Scale), (int)(35 * Scale));
                else return new Rectangle((int)Position.X, (int)Position.Y, 1, 1);
            } 
        }
        public MessageBox(Texture2D texture)
            : base(texture)
        {
            textureManager = TextureManager.Instance;
            fontManager = FontManager.Instance;

            childSprites = new List<Sprite>();
            childSpritesPosition = new List<Vector2>();
            //origin = new Vector2(texture.Width / 2, texture.Height / 2);
            isDraggable = true;
            IsHUD = true; 
        }

        public MessageBox()
        : base()
        {
            textureManager = TextureManager.Instance;
            fontManager = FontManager.Instance;

            childSprites = new List<Sprite>();
            childSpritesPosition = new List<Vector2>();
            isDraggable = true;
            IsHUD = true; 
        }

        public override void Update(Sprite[] sprites)
        {
            base.Update(sprites);
            for (int i = 0; i < childSprites.Count; i++)
            {
                Sprite child = childSprites[i];
                Vector2 pos = Position + childSpritesPosition[i] - Origin;
                child.SetPosition(pos);
            }
        }

        public Sprite GetChild(int index)
        {
            if(childSprites.Count < index)
            {
                return childSprites[index];
            }
            return null; 
        }
        
        public void AddChildSprite(Sprite sprite, Vector2 childPosition)
        {
            sprite.IsHUD = true; 
            sprite.LayerDepth = LayerDepth - .01f;
            childSprites.Add(sprite);
            childSpritesPosition.Add(childPosition);
            gameManager.AddSprite(sprite, this);
        }

        public void AddChildSprite(Sprite sprite)
        {
            AddChildSprite(sprite, sprite.Position); 
        }

        public void SetChildPosition(Sprite child, Vector2 newPos)
        {
            childSpritesPosition[childSprites.IndexOf(child)] = newPos;
        }

        public void Hide()
        {
            IsDisplayed = false; 
            foreach (var s in childSprites)
                s.IsDisplayed = false; 
        }

        public void UnHide()
        {
            IsDisplayed = true;
            foreach (var s in childSprites)
                s.IsDisplayed = true;
        }

        public virtual void Close()
        {
            foreach(Sprite s in childSprites)
            {
                if(s is TextinputBox)
                {
                    (s as TextinputBox).UnFocus(); 
                }
                s.Remove(); 
            }
            gameManager.MousePointer.SetCursorIcon(MousePointerType.Default);
            childSprites.Clear(); 
            childSpritesPosition.Clear();
            this.Remove(); 
        }
        
        public virtual void Show()
        {
            this.IsRemove = false;
            gameManager.AddSprite(this, this);
            if(Title != null)
                this.AddChildSprite(new TextZone(Title, fontManager.GetFont("Default"), Color.White, AlignOptions.TopLeft), new Vector2(2, 2));
        }

        public override void Draw(SpriteBatch spriteBatch,  GameTime gameTime, float lerpAmount, Vector2? position = null)
        {
            base.Draw(spriteBatch, gameTime, lerpAmount);
            if(borderTexture  != null && IsDisplayed)
                spriteBatch.Draw(borderTexture, Position, null, Color.White * Opacity, Radian, Origin, Scale, Effects, LayerDepth+0.1f);
        }

        public static MessageBox Show(string message)
        {
            return Show("Info", message);
        }

        public static MessageBox Show(string title, string message, MessageBoxType messageBoxType = MessageBoxType.None, string custom = null)
        {
            const int maxWidth = 500; 
            if(messageBoxType == MessageBoxType.Custom && custom == null)
            {
                throw new Exception("No custom message set"); 
            }

            TextureManager tm = TextureManager.Instance;
            FontManager fm = FontManager.Instance;
            GameManager gm = GameManager.Instance;
            SpriteFont font = fm.GetFont("Default"); 
            Vector2 bounds = font.MeasureString(message);
            int lineCount = message.Split('\n').Length; 
            if (bounds.X > maxWidth)
            {
                string[] words = message.Split(' ');
                StringBuilder sb = new StringBuilder(); 
                for(int i = 0; i < words.Length; i++)
                {
                    if(font.MeasureString(sb.ToString() + " " + words[i]).X < maxWidth)
                    {
                        sb.Append(" " + words[i]); 
                    }
                    else
                    {
                        sb.Append("\n" + words[i]);
                        lineCount++;
                    }
                }
                bounds.X = maxWidth;
                message = sb.ToString(); 
            }
            MessageBox mb = new MessageBox(tm.CreateBorderedTexture((int)bounds.X + 50, lineCount * 35 + 90, 3, 0, paint => Color.Black, paint => (paint < ((int)bounds.X + 50) * 30)? new Color(22, 59, 224) : new Color(245, 231, 213)));
            mb.Title = title;
            mb.AddChildSprite(new TextZone(title, font, Color.White, AlignOptions.TopLeft), new Vector2(2,2));
            mb.AddChildSprite(new TextZone(message, font, Color.Black, AlignOptions.Center), new Vector2(mb.Bounds.X / 2, mb.Bounds.Y / 2 - 10));
            switch (messageBoxType)
            {
                case MessageBoxType.YesNo:
                    var yesButtonYesNo = new Button(tm.TextureText("Yes", "Default", Color.Red), tm.OutlineText("Yes", "Default", Color.Black, Color.Red, 2));
                    var noButtonYesNo = new Button(tm.TextureText("No", "Default", Color.Red), tm.OutlineText("No", "Default", Color.Black, Color.Red, 2));

                    yesButtonYesNo.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Yes });
                    noButtonYesNo.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.No });

                    mb.AddChildSprite(yesButtonYesNo, new Vector2(mb.Bounds.X / 2 - 50, mb.Bounds.Y - 35));
                    mb.AddChildSprite(noButtonYesNo, new Vector2(mb.Bounds.X / 2 + 50, mb.Bounds.Y - 35));
                    break;
                case MessageBoxType.OkCancel:
                    var okButtonOkCancel = new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2));
                    var CancelButtonOkCancel = new Button(tm.TextureText("Cancel", "Default", Color.Red), tm.OutlineText("Cancel", "Default", Color.Black, Color.Red, 2));

                    okButtonOkCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });
                    CancelButtonOkCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Cancel});

                    mb.AddChildSprite(okButtonOkCancel, new Vector2(mb.Bounds.X / 2 + 50, mb.Bounds.Y - 20));
                    mb.AddChildSprite(CancelButtonOkCancel, new Vector2(mb.Bounds.X / 2 - 50, mb.Bounds.Y - 35));
                    break;
                case MessageBoxType.Ok:
                    var okButtonOk = new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2));

                    okButtonOk.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });

                    mb.AddChildSprite(okButtonOk, new Vector2(mb.Bounds.X / 2, mb.Bounds.Y - 35));
                    break;
                case MessageBoxType.ContinueCancel:
                    var ContinueButtonContinueCancel = new Button(tm.TextureText("Continue", "Default", Color.Red), tm.OutlineText("Continue", "Default", Color.Black, Color.Red, 2));
                    var CancelButtonContinueCancel = new Button(tm.TextureText("Cancel", "Default", Color.Red), tm.OutlineText("Cancel", "Default", Color.Black, Color.Red, 2));

                    ContinueButtonContinueCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Continue });
                    CancelButtonContinueCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Cancel });

                    mb.AddChildSprite(ContinueButtonContinueCancel, new Vector2(mb.Bounds.X / 2 - 50, mb.Bounds.Y - 35));
                    mb.AddChildSprite(CancelButtonContinueCancel, new Vector2(mb.Bounds.X / 2 + 50, mb.Bounds.Y - 35));
                    break;
                case MessageBoxType.Custom:
                    var customButton = new Button(tm.TextureText(custom, "Default", Color.Red), tm.OutlineText(custom, "Default", Color.Black, Color.Red, 2));
                    var customButtonCancel = new Button(tm.TextureText(custom, "Default", Color.Red), tm.OutlineText(custom, "Default", Color.Black, Color.Red, 2));

                    customButton.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });
                    customButtonCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Cancel });

                    mb.AddChildSprite(customButton, new Vector2(mb.Bounds.X / 2 - 50, mb.Bounds.Y - 35));
                    mb.AddChildSprite(customButtonCancel, new Vector2(mb.Bounds.X / 2 + 50, mb.Bounds.Y - 35));
                    break;
                default:
                    var defaultButton = new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2));
                    defaultButton.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => {
                        mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });
                        mb.Close();
                    };
                    mb.AddChildSprite(defaultButton, new Vector2(mb.Bounds.X / 2, mb.Bounds.Y - 35));
                    break; 
            }
            mb.SetAlignOption(AlignOptions.Center);
            mb.Position = new Vector2(gm.Width / 2, gm.Height / 2);
            gm.AddSprite(mb, null);
            return mb;
        }
    }
    public enum MessageBoxType
    {
        YesNo, 
        OkCancel, 
        Ok, 
        ContinueCancel, 
        Custom, 
        None,
    }

    public  enum MessageBoxResult
    {
        Yes, 
        No, 
        Ok, 
        Cancel, 
        Continue, 
    }
    public class MessageBoxResultEventArgs : EventArgs
    {
        public MessageBoxResult MessageBoxResult { get; set; } 
    }
}
