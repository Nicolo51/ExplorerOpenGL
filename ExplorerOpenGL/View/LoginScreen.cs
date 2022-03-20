using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    public class LoginScreen : MessageBox
    {
        public const int Height = 250;
        public const int Width = 350;

        public TextinputBox tbName;
        public TextinputBox tbIP;
        public TextZone txtName;
        public TextZone txtIP;
        public Button btnConnect;
        public Button btnBack;

        public LoginScreen()
            :base()
        {
            Position = new Vector2(gameManager.Width / 2, gameManager.Height / 2);
            SpriteFont font = fontManager.GetFont("Default");
            _texture = textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213)));
            SourceRectangle = new Rectangle(0, 0, _texture.Width, _texture.Height);
            Title = "Login in :";

            tbName = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black), font);
            tbIP = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black), font);
            txtName = new TextZone("Your name :", font, Color.Black);
            txtIP = new TextZone("Host address :", font, Color.Black);
            btnConnect = new Button(textureManager.OutlineText("Connect", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Connect", "Default", Color.CornflowerBlue, Color.Black, 2));
            btnBack = new Button(textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 2));
            
            tbName.SetAlignOption(AlignOption.TopLeft);
            tbIP.SetAlignOption(AlignOption.TopLeft);
            txtName.SetAlignOption(AlignOption.TopLeft);
            txtIP.SetAlignOption(AlignOption.TopLeft);
            btnConnect.SetAlignOption(AlignOption.Left);
            btnBack.SetAlignOption(AlignOption.Right);
            SetAlignOption(AlignOption.Center);

            btnBack.MouseClicked += BtnBack_MouseClicked;
            btnConnect.MouseClicked += BtnConnect_MouseClicked;
        }

        private void BtnConnect_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            gameManager.StartGame("Test", "192.168.1.29");
            //gameManager.StartGame("Test", "127.0.0.1");
            this.Close(); 
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
            AddChildSprite(txtIP, new Vector2(50, 125));
            AddChildSprite(tbIP, new Vector2(50, 150));
            AddChildSprite(btnConnect, new Vector2(50, 205));
            AddChildSprite(btnBack, new Vector2(300, 205));
            base.Show();
        }
    }
}
