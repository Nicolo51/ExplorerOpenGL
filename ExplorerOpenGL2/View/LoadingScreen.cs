using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.View
{
    public class LoadingScreen : MessageBoxIG
    {
        public const int Height = 150;
        public const int Width = 500;
        Sprite bar; 
        Sprite progressBar; 
        TextZone txtPercent;
        double percent; 
        public LoadingScreen(string message = "")
        {
            SetPosition(new Vector2(gameManager.Width / 2, gameManager.Height / 2));
            SpriteFont font = fontManager.GetFont("Default");
            SetTexture(textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213))));
            SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Title = $"Loading {message}:";
            percent = 0d;

            bar = new Sprite(textureManager.CreateTexture(1, 1, paint => Color.Red));
            bar.Bounds = new Vector2(0,30);
            progressBar = new Sprite(textureManager.CreateBorderedTexture(460, 80, 2, 20, paint => Color.Black, paint => Color.Transparent));
            txtPercent = new TextZone("0 %", AlignOptions.Center);

            bar.SetAlignOption(AlignOptions.Center);
            progressBar.SetAlignOption(AlignOptions.Center);
            txtPercent.SetAlignOption(AlignOptions.Center);
            SetAlignOption(AlignOptions.Center);
        }

        public void ChangePercent(double newPercent)
        {
            //max 440 px
            percent = newPercent;
            txtPercent.Text = percent.ToString("N1") + " %";
            bar.Bounds = new Vector2((float)(410 * newPercent / 100), 30);
        }

        public override void Show()
        {
            AddChildSprite(bar, new Vector2(Width / 2, 65));
            AddChildSprite(progressBar, new Vector2(Width / 2, 65));
            AddChildSprite(txtPercent, new Vector2(Width/2, 120)); 

            base.Show();
        }
    }
}
