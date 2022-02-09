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

        public MessageBox(Texture2D texture)
            :base(texture)
        {
            childSprites = new List<Sprite>();
            childSpritesPosition = new List<Vector2>();
            //origin = new Vector2(texture.Width / 2, texture.Height / 2);
            isDraggable = true;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            for(int i = 0; i < childSprites.Count; i++)
            {
                Sprite child = childSprites[i];
                child.Position = Position + childSpritesPosition[i]; 
            }
            base.Update(gameTime, sprites);
        }

        public void AddChildSprite(Sprite sprite, Vector2 childPosition)
        {
            sprite.layerDepth -= .01f;
            sprite.SetOriginToMiddle(); 
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
            TextureManager tm = TextureManager.Instance;
            FontManager fm = FontManager.Instance;
            fm.GetFont("Default").MeasureString(message);
            MessageBox mb = new MessageBox(tm.CreateBorderedTexture()
        }

        public enum MessageBoxType
        {
            YesNo, 
            OkCancel, 
            Ok, 
            Custom
        }
    }
}
