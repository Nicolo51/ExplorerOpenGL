using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.View
{
    public class SinglePlayerMenu : MessageBoxIG
    {
        public Button btnPlaymap;
        public Button btnCreateMap;
        public Button btnEditMap;
        public Button btnBack;
        NetworkManager NetworkManager; 
        public SinglePlayerMenu()
            : base()
        {
            isDraggable = false;
            btnPlaymap = new Button(TextureManager.OutlineText("Play map", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Play map", "Menu", Color.Black, Color.White, 4));
            btnCreateMap = new Button(TextureManager.OutlineText("Create new map", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Create new map", "Menu", Color.Black, Color.White, 4));
            btnEditMap = new Button(TextureManager.OutlineText("Edit existing map", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Edit existing map", "Menu", Color.Black, Color.White, 4));
            btnBack = new Button(TextureManager.OutlineText("Back", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Back", "Menu", Color.Black, Color.White, 4));

            btnCreateMap.SetAlignOption(AlignOptions.Center);
            btnEditMap.SetAlignOption(AlignOptions.Center);
            btnBack.SetAlignOption(AlignOptions.Center);
            btnPlaymap.SetAlignOption(AlignOptions.Center);

            btnPlaymap.MouseClicked += BtnPlaymap_MouseClicked;
            btnCreateMap.MouseClicked += BtnCreateMap_MouseClicked;
            btnEditMap.MouseClicked += BtnEditMap_MouseClicked;
            btnBack.MouseClicked += BtnBack_MouseClicked;
        }

        private void BtnPlaymap_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            var sme = new SelectMapToEditMenu(); 
            sme.Show();
            sme.MapSelected += OnMapSelectedToPlay; 
            this.Close();

        }

        private void OnMapSelectedToPlay(object sender, string mapName)
        {
            GameManager.StartGame("Nicolas", "127.0.0.1", mapName, true);
            this.Close(); 
            //var us = new UploadScreen(mapName);
            //us.UploadEnded += Us_UploadEnded;
            //us.BtnUpload_MouseClicked(this, null, Vector2.Zero); 
            //us.Show();
            //this.Hide(); 
        }

        private void Us_UploadEnded(object sender, bool success, string mapName)
        {
            (sender as UploadScreen).Close();
            this.UnHide(); 
            //NetworkManager.ChangeMap(mapName, null);
            GameManager.StartGame("Nicolas", "127.0.0.1","", true); 
            this.Close(); 
        }

        private void OnMapSelectedToEdit(object sender, string mapName)
        {
            try
            {
                new MapEditor(mapName).Show();
                this.Close();
            }
            catch (Exception e)
            {
                GameManager.Terminal.AddMessageToTerminal($"Failed to load ./maps/{mapName}.xml :{e.Message}", "Error", Color.Red);
            }
        }

        private void BtnBack_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new MainMenu().Show();
            this.Close(); 
        }

        private void BtnCreateMap_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new MapCreationScreen().Show(); 
            this.Close();
        }

        private void BtnEditMap_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            var sme = new SelectMapToEditMenu();
            sme.Show();
            sme.MapSelected += OnMapSelectedToEdit;
            this.Close();
        }

        public override void Close()
        {
            base.Close();
            btnCreateMap.MouseClicked -= BtnCreateMap_MouseClicked;
            btnEditMap.MouseClicked -= BtnEditMap_MouseClicked;
            btnBack.MouseClicked -= BtnBack_MouseClicked;
        }

        public override void Show()
        {
            GameManager.ChangeGameState(GameState.MainMenu);
            AddChildSprite(btnPlaymap, new Vector2(GameManager.Width / 2, GameManager.Height / 2 - 200));
            AddChildSprite(btnCreateMap, new Vector2(GameManager.Width / 2, GameManager.Height / 2 - 100));
            AddChildSprite(btnEditMap, new Vector2(GameManager.Width / 2, GameManager.Height / 2));
            AddChildSprite(btnBack, new Vector2(GameManager.Width / 2, GameManager.Height / 2 + 100));
            base.Show();
        }
    }
}
