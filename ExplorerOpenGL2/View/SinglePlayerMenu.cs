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
        NetworkManager networkManager; 
        public SinglePlayerMenu()
            : base()
        {
            isDraggable = false;
            networkManager = NetworkManager.Instance;
            btnPlaymap = new Button(textureManager.OutlineText("Play map", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Play map", "Menu", Color.Black, Color.White, 4));
            btnCreateMap = new Button(textureManager.OutlineText("Create new map", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Create new map", "Menu", Color.Black, Color.White, 4));
            btnEditMap = new Button(textureManager.OutlineText("Edit existing map", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Edit existing map", "Menu", Color.Black, Color.White, 4));
            btnBack = new Button(textureManager.OutlineText("Back", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Back", "Menu", Color.Black, Color.White, 4));

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
            gameManager.StartGame("Nicolas", "127.0.0.1", mapName, true);
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
            //networkManager.ChangeMap(mapName, null);
            gameManager.StartGame("Nicolas", "127.0.0.1","", true); 
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
                gameManager.Terminal.AddMessageToTerminal($"Failed to load ./maps/{mapName}.xml :{e.Message}", "Error", Color.Red);
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
            gameManager.ChangeGameState(GameState.MainMenu);
            AddChildSprite(btnPlaymap, new Vector2(gameManager.Width / 2, gameManager.Height / 2 - 200));
            AddChildSprite(btnCreateMap, new Vector2(gameManager.Width / 2, gameManager.Height / 2 - 100));
            AddChildSprite(btnEditMap, new Vector2(gameManager.Width / 2, gameManager.Height / 2));
            AddChildSprite(btnBack, new Vector2(gameManager.Width / 2, gameManager.Height / 2 + 100));
            base.Show();
        }
    }
}
