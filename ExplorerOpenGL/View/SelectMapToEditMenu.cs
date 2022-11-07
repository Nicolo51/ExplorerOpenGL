using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    class SelectMapToEditMenu : MessageBox
    {
        public const int Height = 600;
        public const int Width = 500;

        public Button btnNext;
        public Button btnLast;
        public Button btnBack;
        public TextZone Label;
        public TextZone PageCount;
        private int indexMapsStart;
        public string[] maps;
        public Button[] btnMaps; 
        public int NbrOfPages { get { return (maps.Length / nbrMapsPerPage) + 1;  } }
        public int CurrentPage;
        private const int nbrMapsPerPage = 8;
        public SelectMapToEditMenu()
            : base()
        {
            indexMapsStart = 0;
            IsHUD = true;
            if (!Directory.Exists("./maps"))
                Directory.CreateDirectory("./maps"); 
            maps = Directory.GetFiles("./maps", "*.xml");
            for (int i = 0; i < maps.Length; i++)
                maps[i] =  Path.GetFileNameWithoutExtension(maps[i]); 
            btnMaps = new Button[maps.Length];
            Parallel.For(0, maps.Length, index =>
            {
                btnMaps[index] = new Button(textureManager.TextureText(" - " + maps[index], "default", Color.Black), textureManager.TextureText(" - " + maps[index], "default", Color.White));
                btnMaps[index].SetAlignOption(AlignOptions.Left);
                btnMaps[index].MouseClicked += OnMapClicked;
            });
            SetTexture(textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213))));
            
            SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Title = "Select maps to edit";
            btnNext = new Button(textureManager.OutlineText(">", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText(">", "Default", Color.CornflowerBlue, Color.Black, 2));
            btnLast = new Button(textureManager.OutlineText("<", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("<", "Default", Color.CornflowerBlue, Color.Black, 2));
            btnBack = new Button(textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 2));
            Label = new TextZone("List of maps : ");
            PageCount = new TextZone("1 / " + NbrOfPages, AlignOptions.Center); 

            btnNext.SetAlignOption(AlignOptions.Center);
            btnBack.SetAlignOption(AlignOptions.Center);
            btnLast.SetAlignOption(AlignOptions.Center);
            SetPosition(new Vector2(gameManager.Width / 2, gameManager.Height / 2));
            SetAlignOption(AlignOptions.Center);
            CurrentPage = 1;

            btnBack.MouseClicked += BtnBack_MouseClicked1;
            btnNext.MouseClicked += BtnNext_MouseClicked;
            btnLast.MouseClicked += BtnLast_MouseClicked;
        }

        private void OnMapClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            int index = Array.IndexOf(btnMaps, sender);
            if (index < 0)
                return;
            debugManager.AddEvent($"map {maps[index]} will be loaded !");
            try
            {
                new MapEditor(maps[index]).Show(); 
                this.Close(); 
            }
            catch(Exception e)
            {
                gameManager.Terminal.AddMessageToTerminal($"Failed to load ./maps/{maps[index]}.xml :{e.Message}", "Error", Color.Red);
            }
        }

        private void BtnLast_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            if (indexMapsStart - nbrMapsPerPage < 0)
                indexMapsStart = 0;
            else
                indexMapsStart -= nbrMapsPerPage;
            updateList();
        }

        private void updateList()
        {
            for (int i = 0; i < btnMaps.Length; i++)
            {
                if (i >= indexMapsStart && i < indexMapsStart + nbrMapsPerPage)
                    btnMaps[i].IsDisplayed = true;
                else
                    btnMaps[i].IsDisplayed = false;
            }
            CurrentPage = indexMapsStart / nbrMapsPerPage + 1;
            PageCount.Text = CurrentPage + " / " + NbrOfPages;
        }

        private void BtnNext_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            if (!(indexMapsStart + nbrMapsPerPage > btnMaps.Length))
                indexMapsStart += nbrMapsPerPage;
            updateList();
        }

        private void BtnBack_MouseClicked1(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new SinglePlayerMenu().Show();
            this.Close();
        }

        public override void Show()
        {
            AddChildSprite(Label, new Vector2(10, 50));
            AddChildSprite(PageCount, new Vector2(250, 550));
            AddChildSprite(btnNext, new Vector2(300, 550));
            AddChildSprite(btnLast, new Vector2(200, 550));
            AddChildSprite(btnBack, new Vector2(50, 575));
            for (int i = 0; i < btnMaps.Length; i++)
                AddChildSprite(btnMaps[i], new Vector2(Width / 5, 125 + (i % nbrMapsPerPage) * 45));
            updateList(); 
            base.Show();
        }
    }
}
