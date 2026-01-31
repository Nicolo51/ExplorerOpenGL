using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace ExplorerOpenGL2.View
{
    public class MapCreationScreen : MessageBoxIG
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
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                var mb = MessageBoxIG.Show("Can't create a map with an empty name.", "Error", MessageBoxIGType.Ok);
                mb.Result += Mb_Result;
                Hide();
                return; 
            }
            if (!Directory.Exists("./maps"))
                Directory.CreateDirectory("./maps");
            if (File.Exists($@".\maps\{tbName.Text}.xml"))
            {
                var mb = MessageBoxIG.Show("This name is already taken");
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
                var mb = MessageBoxIG.Show($"Could not create the map file : {e.Message}");
                mb.Result += Mb_Result; 
                return; 
            }
            MessageBoxIG.Show(tbName.Text + " has been created !");
            new MapEditor(tbName.Text).Show(); 
            this.Close();
        }
        private void Mb_Result(MessageBoxIG sender, MessageBoxIGResultEventArgs e)
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
