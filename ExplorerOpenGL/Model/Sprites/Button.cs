using ExplorerOpenGL.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Button : Sprite
    {
        public string Text { get; set; }
        
        public Color color { get; private set; }

        private Texture2D mouseOverTexture;
        public Vector2 originMouseOver; 
        public bool isMouseOver; 
        
        public Button(Texture2D texture, Texture2D MouseOverTexture)
            : base(texture)
        {
            IsClickable = true;
            MouseClicked += OnMouseClick; 
            MouseOvered += OnMouseOver; 
            MouseLeft += OnMouseLeft;

            this.mouseOverTexture = MouseOverTexture;
            SetAlignOption(AlignOption.Center);
            Text = String.Empty;
            layerDepth = 0.1f;
            isDraggable = true;
        }
        public override void SetAlignOption(AlignOption ao)
        {
            if (ao == alignOption)
                return;
            Vector2 bounds = new Vector2(mouseOverTexture.Width, mouseOverTexture.Height);
            switch (ao)
            {
                case AlignOption.Left:
                    originMouseOver = new Vector2(0, bounds.Y / 2);
                    break;
                case AlignOption.Right:
                    originMouseOver = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOption.Top:
                    originMouseOver = new Vector2(bounds.X / 2, 0);
                    break;
                case AlignOption.Bottom:
                    originMouseOver = new Vector2(bounds.X / 2, bounds.Y);
                    break;
                case AlignOption.BottomLeft:
                    originMouseOver = new Vector2(0, bounds.Y);
                    break;
                case AlignOption.BottomRight:
                    originMouseOver = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOption.TopLeft:
                    originMouseOver = new Vector2(0, 0);
                    break;
                case AlignOption.TopRight:
                    originMouseOver = new Vector2(bounds.X, 0);
                    break;
                case AlignOption.Center:
                    originMouseOver = new Vector2(bounds.X / 2, bounds.Y / 2);
                    break;
            }
            base.SetAlignOption(ao);
        }

        public override void Update(List<Sprite> sprites)
        {
           
            base.Update(sprites);
        }

        private void OnMouseOver(object sender, MousePointer mousePointer)
        {
            isMouseOver = true;
            gameManager.MousePointer.SetCursorIcon(MousePointerType.Pointer);
            //gameManager.MousePointer.SourceRectangle = new Rectangle(300, 0, 75, 75);
        }
        private void OnMouseLeft(object sender, MousePointer mousePointer)
        {
            isMouseOver = false;
            gameManager.MousePointer.SetCursorIcon(MousePointerType.Default);

        }
        private void OnMouseClick(object sender, MousePointer mousePointer,  Vector2 clickPosition)
        {

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if(isMouseOver)
                spriteBatch.Draw(mouseOverTexture, Position, null, Color.White * Opacity * (isClicked ? .5f : 1f), Radian, originMouseOver, scale, Effects, layerDepth);
            else
                spriteBatch.Draw(_texture, Position, null, Color.White, Radian, origin, scale, Effects, layerDepth);

        }
    }
}
