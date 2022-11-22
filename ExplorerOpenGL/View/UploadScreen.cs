using ExplorerOpenGL.Managers;
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
    public class UploadScreen : MessageBox
    {
       
        public const int Height = 280;
        public const int Width = 350;

        public TextinputBox tbName;
        public TextinputBox tbIP;
        public TextZone txtName;
        public TextZone txtIP;
        public TextZone txtCompletion; 
        public Button btnConnect;
        public Button btnBack;
        public int UploadCompletion { get; set; }

        private NetworkManager networkManager; 

        public UploadScreen(string mapName)
            : base()
        {
            UploadCompletion = 0; 
            SetPosition(new Vector2(gameManager.Width / 2, gameManager.Height / 2));
            SetTexture(textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213))));
            SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Title = "Upload :";

            tbName = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black));
            tbName.Text = mapName;
            tbIP = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black));
            txtName = new TextZone("Map name :");
            txtIP = new TextZone("Host address :");
            txtCompletion = new TextZone($"{UploadCompletion} % complete..."); 
            btnConnect = new Button(textureManager.OutlineText("Upload", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Upload", "Default", Color.CornflowerBlue, Color.Black, 2));
            btnBack = new Button(textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 2));
            tbName.SetAlignOption(AlignOptions.TopLeft);
            tbIP.SetAlignOption(AlignOptions.TopLeft);
            txtName.SetAlignOption(AlignOptions.TopLeft);
            txtIP.SetAlignOption(AlignOptions.TopLeft);
            btnConnect.SetAlignOption(AlignOptions.Left);
            btnBack.SetAlignOption(AlignOptions.Right);
            SetAlignOption(AlignOptions.Center);

            btnBack.MouseClicked += BtnBack_MouseClicked;
            btnConnect.MouseClicked += BtnUpload_MouseClicked;

            networkManager = NetworkManager.Instance; 
        }

        public override void Update(Sprite[] sprites)
        {
            txtCompletion.Text = $"{UploadCompletion} % complete...";
            base.Update(sprites);
        }

        private void BtnUpload_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            //gameManager.StartGame(tbName.Text, tbIP.Text);
            //gameManager.StartGame("Test", "192.168.1.29");
            networkManager.UploadMap(tbName.Text, tbIP.Text);
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
            AddChildSprite(txtCompletion, new Vector2(50, 210)); 
            AddChildSprite(btnConnect, new Vector2(50, 250));
            AddChildSprite(btnBack, new Vector2(300, 250));
            base.Show();
        }
    }
}
