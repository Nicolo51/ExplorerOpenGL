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
    public class PauseMenu : MessageBoxIG
    {
        public Button MainMenuButton;
        public Button ResumeButton; 

        public PauseMenu()
            : base()
        {
            SetTexture(TextureManager.CreateTexture(GameManager.Width, GameManager.Height, paint => Color.Black));
            isDraggable = false;
            SetPosition(Vector2.Zero);

            MainMenuButton = new Button(TextureManager.OutlineText("Main Menu", "Default", Color.CornflowerBlue, Color.Black, 1), TextureManager.OutlineText("Main Menu", "Default", Color.CornflowerBlue, Color.Black, 2));
            ResumeButton = new Button(TextureManager.OutlineText("Resume", "Default", Color.CornflowerBlue, Color.Black, 1), TextureManager.OutlineText("Resume", "Default", Color.CornflowerBlue, Color.Black, 2));
            SetTexture(TextureManager.CreateTexture(GameManager.Width, GameManager.Height, paint => new Color(Color.Black, .5f))); 
            ResumeButton.SetAlignOption(AlignOptions.Center); 
            MainMenuButton.SetAlignOption(AlignOptions.Center); 

            MainMenuButton.MouseClicked += MainMenuButton_MouseClicked;
            ResumeButton.MouseClicked += ResumeButton_MouseClicked;
        }

        private void ResumeButton_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            this.Close(); 
        }

        private void MainMenuButton_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            GameManager.ToMainMenu();
            GameManager.Terminal.EnableMouseOver();
            this.Close();
        }

        public override void Show()
        {
            GameManager.ChangeGameState(GameState.Pause);
            AddChildSprite(MainMenuButton, new Vector2(GameManager.Width / 2, GameManager.Height / 2 + 50));
            AddChildSprite(ResumeButton, new Vector2(GameManager.Width / 2, GameManager.Height / 2 - 50));
            foreach (Sprite s in childSprites)
            {
                s.IsRemove = false;
            }
            GameManager.ChangeGameState(GameState.Pause);
            GameManager.Terminal.DisableMouseOver(); 
            base.Show();
        }
        public override void Close()
        {
            if (GameManager.GameState == GameState.Pause)
            {
                GameManager.ChangeToLastGameState();
                GameManager.Terminal.EnableMouseOver();
            }
            base.Close();
        }
    }
}
