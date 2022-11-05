using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace ExplorerOpenGL.View
{
    public class MapCreationScreen : MessageBox
    {
        public const int Height = 150;
        public const int Width = 350;

        public TextinputBox tbName;
        public TextZone txtName;
        public Button btnCreate;
        public Button btnBack;

        public MapCreationScreen()
            : base()
        {
            SetPosition(new Vector2(gameManager.Width / 2, gameManager.Height / 2));
            SpriteFont font = fontManager.GetFont("Default");
            SetTexture(textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213))));
            SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Title = "Create map :";

            tbName = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black), font);
            txtName = new TextZone("Map name :", font, Color.Black);
            btnCreate = new Button(textureManager.OutlineText("Create", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Create", "Default", Color.CornflowerBlue, Color.Black, 2));
            btnBack = new Button(textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 2));

            tbName.SetAlignOption(AlignOptions.TopLeft);
            txtName.SetAlignOption(AlignOptions.TopLeft);
            btnCreate.SetAlignOption(AlignOptions.Left);
            btnBack.SetAlignOption(AlignOptions.Right);
            SetAlignOption(AlignOptions.Center);

            btnBack.MouseClicked += BtnBack_MouseClicked;
            btnCreate.MouseClicked += BtnCreate_MouseClicked;
        }

        private void BtnCreate_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            if (File.Exists($@".\maps\{tbName.Text}.xml"))
            {
                var mb = MessageBox.Show("This name is already taken");
                mb.Result += Mb_Result;
                Hide();
                tbName.UnFocus(); 
                return;
            }
            try
            {
                var stream = File.CreateText($@".\maps\{tbName.Text}.xml");
                Directory.CreateDirectory($@".\maps\{tbName.Text}");
                stream.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<map><name>" + tbName.Text + "</name>\n<sprites></sprites></map>");
                stream.Close(); 
            }
            catch (Exception e)
            {
                Hide();
                var mb = MessageBox.Show($"Could not create the map file : {e.Message}");
                mb.Result += Mb_Result; 
                return; 
            }
            MessageBox.Show(tbName.Text + " has been created !");
            new MapEditor(tbName.Text).Show(); 
            this.Close();
        }
        private void Mb_Result(MessageBox sender, MessageBoxResultEventArgs e)
        {
            UnHide();         
        }

        private void BtnBack_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new SinglePlayerMenu().Show();
            this.Close();
        }

        public override void Show()
        {
            AddChildSprite(txtName, new Vector2(50, 50));
            AddChildSprite(tbName, new Vector2(50, 75));
            AddChildSprite(btnCreate, new Vector2(50, 130));
            AddChildSprite(btnBack, new Vector2(300, 130));
            base.Show();
        }
    }
}
