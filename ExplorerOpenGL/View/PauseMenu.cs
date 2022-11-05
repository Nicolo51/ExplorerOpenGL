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
    public class PauseMenu : MessageBox
    {
        public Button MainMenuButton;
        public Button ResumeButton; 

        public PauseMenu()
            : base()
        {
            SetTexture(textureManager.CreateTexture(gameManager.Width, gameManager.Height, paint => Color.Black));
            isDraggable = false;
            SetPosition(Vector2.Zero);

            MainMenuButton = new Button(textureManager.OutlineText("Main Menu", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Main Menu", "Default", Color.CornflowerBlue, Color.Black, 2));
            ResumeButton = new Button(textureManager.OutlineText("Resume", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Resume", "Default", Color.CornflowerBlue, Color.Black, 2));
            SetTexture(textureManager.CreateTexture(gameManager.Width, gameManager.Height, paint => new Color(Color.Black, .5f))); 
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
            gameManager.ToMainMenu();
            gameManager.Terminal.EnableMouseOver();
            this.Close();
        }

        public override void Show()
        {
            gameManager.ChangeGameState(GameState.Pause);
            AddChildSprite(MainMenuButton, new Vector2(gameManager.Width / 2, gameManager.Height / 2 + 50));
            AddChildSprite(ResumeButton, new Vector2(gameManager.Width / 2, gameManager.Height / 2 - 50));
            foreach (Sprite s in childSprites)
            {
                s.IsRemove = false;
            }
            gameManager.ChangeGameState(GameState.Pause);
            gameManager.Terminal.DisableMouseOver(); 
            base.Show();
        }
        public override void Close()
        {
            if (gameManager.GameState == GameState.Pause)
            {
                gameManager.ChangeToLastGameState();
                gameManager.Terminal.EnableMouseOver();
            }
            base.Close();
        }
    }
}
