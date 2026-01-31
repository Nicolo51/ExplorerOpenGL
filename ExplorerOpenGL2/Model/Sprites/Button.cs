using ExplorerOpenGL2.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Model.Sprites
{
    public class Button : Sprite
    {
        public string Text { get; set; }
        public bool IsEnable { get; private set; }

        public Color color { get; private set; }

        private Texture2D mouseOverTexture;
        public Vector2 originMouseOver;
        public Vector2 OriginMouseOver { get { return originMouseOver * Scale; } }
        public bool isMouseOver;

        public Button(Texture2D texture, Texture2D MouseOverTexture = null)
            : base(texture)
        {
            IsClickable = true;
            MouseClicked += OnMouseClick;
            MouseOvered += OnMouseOver;
            MouseLeft += OnMouseLeft;

            this.mouseOverTexture = MouseOverTexture ?? texture;
            SetAlignOption(AlignOptions.Center);
            Text = String.Empty;
            LayerDepth = 0.1f;
            Enable();
        }

        public void Disable()
        {
            Opacity = .5f;
            IsEnable = false;
        }

        public void Enable()
        {
            Opacity = 1f;
            IsEnable = true;
        }
        public override void SetAlignOption(AlignOptions ao)
        {
            if (ao == AlignOption)
                return;
            Vector2 bounds = new Vector2(mouseOverTexture.Width, mouseOverTexture.Height);
            switch (ao)
            {
                case AlignOptions.Left:
                    originMouseOver = new Vector2(0, bounds.Y / 2);
                    break;
                case AlignOptions.Right:
                    originMouseOver = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOptions.Top:
                    originMouseOver = new Vector2(bounds.X / 2, 0);
                    break;
                case AlignOptions.Bottom:
                    originMouseOver = new Vector2(bounds.X / 2, bounds.Y);
                    break;
                case AlignOptions.BottomLeft:
                    originMouseOver = new Vector2(0, bounds.Y);
                    break;
                case AlignOptions.BottomRight:
                    originMouseOver = new Vector2(bounds.X, bounds.Y / 2);
                    break;
                case AlignOptions.TopLeft:
                    originMouseOver = new Vector2(0, 0);
                    break;
                case AlignOptions.TopRight:
                    originMouseOver = new Vector2(bounds.X, 0);
                    break;
                case AlignOptions.Center:
                    originMouseOver = new Vector2(bounds.X / 2, bounds.Y / 2);
                    break;
            }
            base.SetAlignOption(ao);
        }

        public override void Update(List<Sprite> sprites, GameTime gametime, NetGameState netGameState)
        {
            if (IsEnable)
                base.Update(sprites, gametime, netGameState);
        }

        private void OnMouseOver(object sender, MousePointer mousePointer)
        {
            if (!IsEnable)
                return; 
            isMouseOver = true;
            gameManager.MousePointer.SetCursorIcon(MousePointerType.Pointer);
            //gameManager.MousePointer.SourceRectangle = new Rectangle(300, 0, 75, 75);
        }
        private void OnMouseLeft(object sender, MousePointer mousePointer)
        {
            isMouseOver = false;
            gameManager.MousePointer.SetCursorIcon(MousePointerType.Default);

        }
        private void OnMouseClick(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {

        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float lerpAmount = 1, params ShaderArgument[] shaderArgs)
        {
            if (!IsDisplayed)
                return;
            //if (isMouseOver)
            //{
            //    spriteBatch.Draw(mouseOverTexture, Position, null, Color.White * Opacity, Radian, OriginMouseOver * Scale, Scale, Effects, LayerDepth);
            //    if (TextOnTop != null)
            //        renderManager.DrawString(TextOnTop.Font, TextOnTop.Text, position ?? Position, TextOnTop.Color, Radian, Origin - new Vector2(HitBox.Width / 2, HitBox.Height / 2) + TextOnTop.Origin, TextOnTop.Scale, TextOnTop.Effects, LayerDepth - .1f);

            //}
            //else
                base.Draw(spriteBatch, gameTime, lerpAmount);
        }
    }
}
