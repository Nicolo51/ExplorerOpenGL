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
                child.Position = Position + childSpritesPosition[i] - origin;
            }
        }

        public void AddChildSprite(Sprite sprite, Vector2 childPosition)
        {
            sprite.IsHUD = true; 
            sprite.layerDepth -= .01f;
            childSprites.Add(sprite);
            childSpritesPosition.Add(childPosition);
            gameManager.AddSprite(sprite, this);
        }

        public void Close()
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

        public void Hide()
        {
            foreach (Sprite s in childSprites)
            {
                s.Remove();
            }
            gameManager.MousePointer.SetCursorIcon(MousePointerType.Default);
            childSprites.Clear();
            childSpritesPosition.Clear();
            this.Remove();
        }
        public virtual void Show()
        {
            gameManager.AddSprite(this, this);
            if(Title != null)
                this.AddChildSprite(new TextZone(Title, fontManager.GetFont("Default"), Color.White, AlignOption.TopLeft), new Vector2(2, 2));
        }

        public override void Draw(SpriteBatch spriteBatch, float lerpAmount )
        {
            base.Draw(spriteBatch, lerpAmount);
            if(borderTexture  != null)
                spriteBatch.Draw(borderTexture, Position, null, Color.White * Opacity * (isClicked && IsClickable ? .5f : 1f), Radian, origin, scale, Effects, layerDepth+0.1f);
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
            mb.AddChildSprite(new TextZone(title, font, Color.White, AlignOption.TopLeft), new Vector2(2,2));
            mb.AddChildSprite(new TextZone(message, font, Color.Black, AlignOption.Center), new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height / 2 - 10));
            switch (messageBoxType)
            {
                case MessageBoxType.YesNo:
                    var yesButtonYesNo = new Button(tm.TextureText("Yes", "Default", Color.Red), tm.OutlineText("Yes", "Default", Color.Black, Color.Red, 2));
                    var noButtonYesNo = new Button(tm.TextureText("No", "Default", Color.Red), tm.OutlineText("No", "Default", Color.Black, Color.Red, 2));

                    yesButtonYesNo.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Yes });
                    noButtonYesNo.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.No });

                    mb.AddChildSprite(yesButtonYesNo, new Vector2(mb.Bounds.Width / 2 - 50, mb.Bounds.Height - 35));
                    mb.AddChildSprite(noButtonYesNo, new Vector2(mb.Bounds.Width / 2 + 50, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.OkCancel:
                    var okButtonOkCancel = new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2));
                    var CancelButtonOkCancel = new Button(tm.TextureText("Cancel", "Default", Color.Red), tm.OutlineText("Cancel", "Default", Color.Black, Color.Red, 2));

                    okButtonOkCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });
                    CancelButtonOkCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Cancel});

                    mb.AddChildSprite(okButtonOkCancel, new Vector2(mb.Bounds.Width / 2 + 50, mb.Bounds.Height - 20));
                    mb.AddChildSprite(CancelButtonOkCancel, new Vector2(mb.Bounds.Width / 2 - 50, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.Ok:
                    var okButtonOk = new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2));

                    okButtonOk.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });

                    mb.AddChildSprite(okButtonOk, new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.ContinueCancel:
                    var ContinueButtonContinueCancel = new Button(tm.TextureText("Continue", "Default", Color.Red), tm.OutlineText("Continue", "Default", Color.Black, Color.Red, 2));
                    var CancelButtonContinueCancel = new Button(tm.TextureText("Cancel", "Default", Color.Red), tm.OutlineText("Cancel", "Default", Color.Black, Color.Red, 2));

                    ContinueButtonContinueCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Continue });
                    CancelButtonContinueCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Cancel });

                    mb.AddChildSprite(ContinueButtonContinueCancel, new Vector2(mb.Bounds.Width / 2 - 50, mb.Bounds.Height - 35));
                    mb.AddChildSprite(CancelButtonContinueCancel, new Vector2(mb.Bounds.Width / 2 + 50, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.Custom:
                    var customButton = new Button(tm.TextureText(custom, "Default", Color.Red), tm.OutlineText(custom, "Default", Color.Black, Color.Red, 2));
                    var customButtonCancel = new Button(tm.TextureText(custom, "Default", Color.Red), tm.OutlineText(custom, "Default", Color.Black, Color.Red, 2));

                    customButton.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });
                    customButtonCancel.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Cancel });

                    mb.AddChildSprite(customButton, new Vector2(mb.Bounds.Width / 2 - 50, mb.Bounds.Height - 35));
                    mb.AddChildSprite(customButtonCancel, new Vector2(mb.Bounds.Width / 2 + 50, mb.Bounds.Height - 35));
                    break;
                default:
                    var defaultButton = new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2));
                    defaultButton.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => {
                        mb.Result?.Invoke(mb, new MessageBoxResultEventArgs() { MessageBoxResult = MessageBoxResult.Ok });
                        mb.Close();
                    };
                    mb.AddChildSprite(defaultButton, new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height - 35));
                    break; 
            }
            mb.SetAlignOption(AlignOption.Center);
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
