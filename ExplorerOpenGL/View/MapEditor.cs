using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    public class MapEditor : MessageBox
    {
        Button[] AddableElements;
        Button ToMainMenu;
        Button Save;
        
        Sprite selectedSprite; 

        XmlManager xmlManager;
        KeyboardManager keyboardManager; 
        
        string mapName; 
        public MapEditor(string mapName)
        {
            this.mapName = mapName; 
            xmlManager = XmlManager.Instance;
            keyboardManager = KeyboardManager.Instance; 

            Sprite[] map = xmlManager.LoadMap($"./maps/{mapName}.xml");
            gameManager.AddSprite(map, this);

            foreach(var s in map)
            {
                s.MouseClicked += MapElementMouseClicked;
                //s.MouseLeft += (object sender, MousePointer mousePointer) => { s.Shader = shaderManager.LoadShader("Normal"); };
                //s.MouseOvered += (object sender, MousePointer mousePointer) => { s.Shader = shaderManager.LoadShader("Outline"); };
                //s.MouseClicked += (object sender, MousePointer mousePointer, Vector2 clickPosition) => { s.Shader = shaderManager.LoadShader("Outline"); s.SetShaderArgs(new ShaderArgument[] { new ShaderArgument("thickness", new Vector2(3, 3)), new ShaderArgument("outlineColor", Color.White) }); };
            }

            isDraggable = false;
            IsHUD = true;
            //SetTexture(textureManager.CreateTexture(gameManager.Width, gameManager.Height, paint => (paint < gameManager.Width * 100) ? Color.Transparent : Color.Transparent));
            AddableElements = new Button[4];
            Bounds = new Vector2(gameManager.Width, gameManager.Height);

            Button wall = new Button(textureManager.LoadTexture("EditorTile"))
            {
                TextOnTop = new TextZone("Wall", AlignOptions.Top),
                Scale = 0.5f,
            };
            wall.SetAlignOption(AlignOptions.Left); 
            wall.MouseOvered += Wall_MouseOvered;
            wall.MouseLeft += Wall_MouseLeft;
            wall.MouseClicked += Wall_MouseClicked;
            

            Texture2D texture = textureManager.CreateBorderedTexture(80, 40, 2, 0, paint => Color.Black, paint => Color.Green);
            Button save = new Button(texture)
            {
                TextOnTop = new TextZone("Save", AlignOptions.Center),
                Position = new Vector2(Bounds.X - 80, 0),
            };
            save.MouseClicked += Save_MouseClicked;
            save.SetAlignOption(AlignOptions.TopRight);

            Button exit = new Button(texture)
            {
                TextOnTop = new TextZone("Exit", AlignOptions.Center),
                Position = new Vector2(Bounds.X, 0), 
            };
            exit.MouseClicked += Exit_MouseClicked;
            exit.SetAlignOption(AlignOptions.TopRight);

            Button delete = new Button(texture)
            {
                TextOnTop = new TextZone("Delete", AlignOptions.Center),
                Position = new Vector2(Bounds.X - 160, 0),
            };
            delete.MouseClicked += Delete_MouseClicked;
            delete.SetAlignOption(AlignOptions.TopRight);
            delete.Disable(); 

            AddableElements[0] = wall; 
            AddableElements[1] = save; 
            AddableElements[2] = exit;
            AddableElements[3] = delete;
        }

        private void Delete_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            AddableElements[3].Disable();
            selectedSprite.Remove(); 
        }

        private void MapElementMouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            if (selectedSprite != null)
            {
                selectedSprite.Opacity = 1f;
                selectedSprite.Shader = shaderManager.GetDefaultShader(); 
            }
            selectedSprite = (Sprite)sender;
            //selectedSprite.Opacity = .5f;
            selectedSprite.Shader = shaderManager.LoadShader("Outline");
            selectedSprite.SetShaderArgs(new ShaderArgument[] 
            { 
                new ShaderArgument("thickness", new Vector2(5, 5)), 
                new ShaderArgument("outlineColor", Color.Red) 
            });
            AddableElements[3].Enable();
            if (keyboardManager.IsKeyDown(Keys.LeftControl))
            {
                new ChoosePropertiesMenu(selectedSprite).Show();  ; 
            }
        }

        private void Exit_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            this.Close();
            gameManager.ClearScene(); 
            new MainMenu().Show(); 
        }

        private void Save_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            var sprites = gameManager.GetSprites().Where(s => s is Wall).ToArray(); // Interface to impemente later
            xmlManager.SaveMap(sprites, mapName);
            this.Close();
            gameManager.ClearScene(); 
            new MainMenu().Show(); 
        }

        private void Wall_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            Wall wall = new Wall(textureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige));
            wall.MouseClicked += MapElementMouseClicked;
            //Wall wall = new Wall(textureManager.LoadNoneContentLoadedTexture(texturePath));
            wall.isDraggable = true;
            gameManager.AddSprite(wall, this);
        }

        private void Wall_MouseLeft(object sender, MousePointer mousePointer)
        {
            SetChildPosition(sender as Sprite, new Vector2((sender as Sprite).Position.X, (sender as Sprite).Position.Y-10));
        }

        private void Wall_MouseOvered(object sender, MousePointer mousePointer)
        {
            SetChildPosition(sender as Sprite, new Vector2((sender as Sprite).Position.X, (sender as Sprite).Position.Y + 10)); 
        }

        public override void Show()
        {
            foreach (Button btn in AddableElements)
                AddChildSprite(btn, btn.Position); 

            base.Show();
        }
    }
}
