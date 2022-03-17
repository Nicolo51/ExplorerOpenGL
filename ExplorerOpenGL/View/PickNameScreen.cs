using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    class PickNameScreen : MessageBox
    {
        public const int Height = 150;
        public const int Width = 350;

        TextinputBox tbName;
        TextZone txtName;
        Button btnStart;
        Button btnBack;

        public PickNameScreen()
            : base()
        {
            SpriteFont font = fontManager.GetFont("Default");
            _texture = textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213)));
            SourceRectangle = new Rectangle(0, 0, _texture.Width, _texture.Height);
            Title = "Login in :";

            tbName = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black), font);
            txtName = new TextZone("Your name :", font, Color.Black);
            btnStart = new Button(textureManager.OutlineText("Start", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Start", "Default", Color.CornflowerBlue, Color.Black, 2));
            btnBack = new Button(textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 2));

            tbName.SetAlignOption(AlignOption.TopLeft);
            txtName.SetAlignOption(AlignOption.TopLeft);
            btnStart.SetAlignOption(AlignOption.Left);
            btnBack.SetAlignOption(AlignOption.Right);

            btnBack.MouseClicked += BtnBack_MouseClicked;
            btnStart.MouseClicked += BtnConnect_MouseClicked;

        }

        private void BtnConnect_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            this.Close(); 
            gameManager.StartGame(tbName.Text);
        }

        private void BtnBack_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new MainMenu().Show();
            this.Close();
        }

        public override void Show()
        {
            AddChildSprite(txtName, new Vector2(50, 50));
            AddChildSprite(tbName, new Vector2(50, 75));
            AddChildSprite(btnStart, new Vector2(50, 130));
            AddChildSprite(btnBack, new Vector2(300, 130));
            base.Show();
        }
    }
}
