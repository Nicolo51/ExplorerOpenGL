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
        private AlignOption alignOption;
        private Vector2 padding;
        public TextZone(string text, SpriteFont font, Color color, AlignOption ao)
            :base()
        {
            Color = color; 
            Text = text;
            Font = font; 
            isDraggable = false;
            IsClickable = false;
            bounds = font.MeasureString(text);
            SetAlignOption(ao);
            padding = Vector2.Zero;
        }

        public void SetAlignOption(AlignOption ao)
        {
            if (ao == alignOption)
                return; 
            switch (ao)
            {
                case AlignOption.Left:
                    origin = new Vector2(0, bounds.Y / 2); 
                    break;
                case AlignOption.Right:
                    origin = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOption.Top:
                    origin = new Vector2(bounds.X / 2, 0);
                    break;
                case AlignOption.Bottom:
                    origin = new Vector2(bounds.X / 2, bounds.Y);
                    break;
                case AlignOption.BottomLeft:
                    origin = new Vector2(0, bounds.Y);
                    break;
                case AlignOption.BottomRight:
                    origin = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOption.TopLeft:
                    origin = new Vector2(0, 0);
                    break;
                case AlignOption.TopRight:
                    origin = new Vector2(bounds.X, 0);
                    break;
                case AlignOption.Center:
                    origin = new Vector2(bounds.X / 2, bounds.Y / 2);
                    break;
            }
            alignOption = ao; 
        }

        public void SetPadding(Vector2 pad)
        {
            if (pad == padding)
                return;
            padding = pad; 
            SetAlignOption(alignOption);
            origin += padding; 
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            base.Update(gameTime, sprites);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, Text, Position, Color, Radian, origin, scale, Effects, layerDepth); 
        }
    }
    public enum AlignOption
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
        TopLeft
    }
}
