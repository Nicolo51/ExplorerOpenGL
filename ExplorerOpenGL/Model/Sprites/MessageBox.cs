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

        private List<Sprite> childSprites;
        private List<Vector2> childSpritesPosition;
        public MessageBoxResult Result { get; set; }

        public MessageBox(Texture2D texture)
            : base(texture)
        {
            childSprites = new List<Sprite>();
            childSpritesPosition = new List<Vector2>();
            //origin = new Vector2(texture.Width / 2, texture.Height / 2);
            isDraggable = true;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            for (int i = 0; i < childSprites.Count; i++)
            {
                Sprite child = childSprites[i];
                child.Position = Position + childSpritesPosition[i];
            }
            base.Update(gameTime, sprites);
        }

        public void AddChildSprite(Sprite sprite, Vector2 childPosition)
        {
            sprite.layerDepth -= .01f;
            childSprites.Add(sprite);
            childSpritesPosition.Add(childPosition);
            gameManager.AddSprite(sprite, this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public static MessageBox Show(string title, string message, MessageBoxType messageBoxType, string custom = null)
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
            int lineCount = 1; 
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
            mb.AddChildSprite(new TextZone(title, font, Color.White, AlignOption.TopLeft), new Vector2(2,2));
            mb.AddChildSprite(new TextZone(message, font, Color.Black, AlignOption.Center), new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height / 2 - 10));
            switch (messageBoxType)
            {
                case MessageBoxType.YesNo:
                    mb.AddChildSprite(new Button(tm.TextureText("Yes", "Default", Color.Red), tm.OutlineText("Yes", "Default", Color.Black, Color.Red, 2)), new Vector2(mb.Bounds.Width / 2 + 50, mb.Bounds.Height - 35));
                    mb.AddChildSprite(new Button(tm.TextureText("No", "Default", Color.Red), tm.OutlineText("No", "Default", Color.Black, Color.Red, 2)), new Vector2(mb.Bounds.Width / 2 - 50, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.OkCancel:
                    var okButton = new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2));
                    var CancelButton = new Button(tm.TextureText("Cancel", "Default", Color.Red), tm.OutlineText("Cancel", "Default", Color.Black, Color.Red, 2)); 

                    mb.AddChildSprite(okButton, new Vector2(mb.Bounds.Width / 2 + 50, mb.Bounds.Height - 20));
                    mb.AddChildSprite(CancelButton, new Vector2(mb.Bounds.Width / 2 - 50, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.Ok:
                    mb.AddChildSprite(new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2)), new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.Continue:
                    mb.AddChildSprite(new Button(tm.TextureText("Continue", "Default", Color.Red), tm.OutlineText("Continue", "Default", Color.Black, Color.Red, 2)), new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height - 35));
                    break;
                case MessageBoxType.Custom:
                    mb.AddChildSprite(new Button(tm.TextureText(custom, "Default", Color.Red), tm.OutlineText(custom, "Default", Color.Black, Color.Red, 2)), new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height - 35));
                    break;
                default:
                    mb.AddChildSprite(new Button(tm.TextureText("OK", "Default", Color.Red), tm.OutlineText("OK", "Default", Color.Black, Color.Red, 2)), new Vector2(mb.Bounds.Width / 2, mb.Bounds.Height - 35));
                    break; 
            }
            gm.AddSprite(mb, null);
            return mb;
        }
    }
    public enum MessageBoxType
    {
        YesNo, 
        OkCancel, 
        Ok, 
        Continue, 
        Custom
    }

    public  enum MessageBoxResult
    {
        Yes, 
        No, 
        Ok, 
        Cancel, 
        Continue, 
    }
}
