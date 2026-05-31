using ExplorerOpenGL.View;
using ExplorerOpenGL2.Managers;
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
    public class MainMenu : MessageBoxIG
    {
        public Button btnSinglePlayer;
        public Button btnMultiPlayer;
        public Button btnOption;
        private Button btnQuit;

        public MainMenu()
            : base()
        {
            isDraggable = false;

            btnSinglePlayer = new Button(TextureManager.OutlineText("Singleplayer", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Singleplayer", "Menu", Color.Black, Color.White, 4));
            btnMultiPlayer = new Button(TextureManager.OutlineText("Multiplayer", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Multiplayer", "Menu", Color.Black, Color.White, 4));
            btnOption = new Button(TextureManager.OutlineText("Options", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Options", "Menu", Color.Black, Color.White, 4));
            btnQuit = new Button(TextureManager.OutlineText("Quit", "Menu", Color.Black, Color.White, 2), TextureManager.OutlineText("Quit", "Menu", Color.Black, Color.White, 4));
            
            btnSinglePlayer.SetAlignOption(AlignOptions.Center);
            btnMultiPlayer.SetAlignOption(AlignOptions.Center);
            btnOption.SetAlignOption(AlignOptions.Center);
            btnQuit.SetAlignOption(AlignOptions.Center);

            btnSinglePlayer.MouseClicked += BtnSinglePlayer_MouseClicked;
            btnMultiPlayer.MouseClicked += BtnMultiPlayer_MouseClicked;
            btnOption.MouseClicked += BtnOption_MouseClicked;
            btnQuit.MouseClicked += BtnQuit_MouseClicked;
        }

        private void BtnQuit_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            GameManager.Exit(); 
        }

        private void BtnSinglePlayer_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new SinglePlayerMenu().Show();
            this.Close(); 
        }

        private void BtnMultiPlayer_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new LoginScreen().Show();
            this.Close(); 
        }

        private void BtnOption_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new OptionScreen().Show();
            this.Close(); 
        }

        public override void Close()
        {
            btnSinglePlayer.MouseClicked -= BtnSinglePlayer_MouseClicked;
            btnMultiPlayer.MouseClicked -= BtnMultiPlayer_MouseClicked;
            btnOption.MouseClicked -= BtnOption_MouseClicked;
            base.Close();
        }

        public override void Show()
        {
            GameManager.ChangeGameState(GameState.MainMenu);
            AddChildSprite(btnSinglePlayer, new Vector2(GameManager.Width / 2, GameManager.Height / 2 - 200)); 
            AddChildSprite(btnMultiPlayer, new Vector2(GameManager.Width / 2, GameManager.Height/2-50)); 
            AddChildSprite(btnOption, new Vector2(GameManager.Width / 2, GameManager.Height / 2 + 100));
            AddChildSprite(btnQuit, new Vector2(GameManager.Width / 2, GameManager.Height / 2 + 250));
            base.Show();
        }
    }
}
