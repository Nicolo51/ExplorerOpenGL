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
    public class MainMenu : MessageBox
    {
        Button btnSinglePlayer;
        Button btnMultiPlayer;
        Button btnOption;
        public MainMenu()
            : base()
        {
            isDraggable = false;

            btnSinglePlayer = new Button(textureManager.OutlineText("Singleplayer", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Singleplayer", "Menu", Color.Black, Color.White, 4));
            btnMultiPlayer = new Button(textureManager.OutlineText("Multiplayer", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Multiplayer", "Menu", Color.Black, Color.White, 4));
            btnOption = new Button(textureManager.OutlineText("Options", "Menu", Color.Black, Color.White, 2), textureManager.OutlineText("Options", "Menu", Color.Black, Color.White, 4));

            btnSinglePlayer.SetAlignOption(AlignOption.Center);
            btnMultiPlayer.SetAlignOption(AlignOption.Center);
            btnOption.SetAlignOption(AlignOption.Center);

            btnSinglePlayer.MouseClicked += BtnSinglePlayer_MouseClicked;
            btnMultiPlayer.MouseClicked += BtnMultiPlayer_MouseClicked;
            btnOption.MouseClicked += BtnOption_MouseClicked;
        }

        private void BtnSinglePlayer_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new PickNameScreen().Show();
            this.Close(); 
        }

        private void BtnMultiPlayer_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            new LoginScreen().Show();
            this.Close(); 
        }

        private void BtnOption_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            
        }

        public override void Show()
        {
            AddChildSprite(btnSinglePlayer, new Vector2(0, -150)); 
            AddChildSprite(btnMultiPlayer, new Vector2(0, 0)); 
            AddChildSprite(btnOption, new Vector2(0, 150));
            base.Show();
        }
    }
}
