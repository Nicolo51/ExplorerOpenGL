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
    public class TextZone : Sprite
    {
        public string Text { get; private set; }
        public SpriteFont Font { get; private set; }
        public Color Color { get; set; }
        private Vector2 bounds; 
        public override Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)bounds.X, (int)bounds.Y); } }
        public TextZone(string text, SpriteFont font, Color color , AlignOption ao = AlignOption.None)
            :base()
        {
            Color = color; 
            Text = text;
            Font = font;
            bounds = Font.MeasureString(text); 
            isDraggable = false;
            IsClickable = false;
            SetAlignOption(ao);
        }

        public override void Update(Sprite[] sprites)
        {
            base.Update(sprites);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float lerpAmount = 1)
        {
            spriteBatch.DrawString(Font, Text, Position, Color, Radian, origin, scale, Effects, layerDepth); 
        }
    }
    
}
