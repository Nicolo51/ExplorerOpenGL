using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    public class SinglePlayerMenu : MessageBox
    {
        public Button btnCreateMap;
        public Button btnEditMap;
        public Button btnBack; 
        public SinglePlayerMenu()
            : base()
        {
            isDraggable = false;

            btnCreateMap = new Button(textureManager.OutlineText("Create new map", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Create new map", "Menu", Color.Black, Color.White, 4));
            btnEditMap = new Button(textureManager.OutlineText("Edit existing map", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Edit existing map", "Menu", Color.Black, Color.White, 4));
            btnBack = new Button(textureManager.OutlineText("Back", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Back", "Menu", Color.Black, Color.White, 4));

            btnCreateMap.SetAlignOption(AlignOptions.Center);
            btnEditMap.SetAlignOption(AlignOptions.Center);
            btnBack.SetAlignOption(AlignOptions.Center); 

            btnCreateMap.MouseClicked += BtnCreateMap_MouseClicked;
            btnEditMap.MouseClicked += BtnEditMap_MouseClicked;
            btnBack.MouseClicked += BtnBack_MouseClicked;
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
            new SelectMapToEditMenu().Show(); 
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
            AddChildSprite(btnCreateMap, new Vector2(gameManager.Width / 2, gameManager.Height / 2 - 150));
            AddChildSprite(btnEditMap, new Vector2(gameManager.Width / 2, gameManager.Height / 2));
            AddChildSprite(btnBack, new Vector2(gameManager.Width / 2, gameManager.Height / 2 + 150));
            base.Show();
        }
    }
}
