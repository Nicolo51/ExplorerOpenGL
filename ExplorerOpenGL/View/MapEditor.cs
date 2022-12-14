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
        Button Save;
        Button Exit;
        Button Delete;
        Button Upload;
        List<Sprite> sprites; 
        Sprite selectedSprite; 

        XmlManager xmlManager;
        KeyboardManager keyboardManager;
        NetworkManager networkManager;
        public string MapName { get; private set; }
        
        string mapName; 
        public MapEditor(string mapName)
        {
            sprites = new List<Sprite>(); 
            networkManager = NetworkManager.Instance; 
            this.mapName = mapName; 
            xmlManager = XmlManager.Instance;
            keyboardManager = KeyboardManager.Instance; 

            Sprite[] map = xmlManager.LoadMap($"./maps/{mapName}.xml");
            gameManager.AddSprite(map, this);

            foreach(var s in map)
            {
                sprites.Add(s); 
                s.MouseClicked += MapElementMouseClicked;
            }

            isDraggable = false;
            IsHUD = true;
            //SetTexture(textureManager.CreateTexture(gameManager.Width, gameManager.Height, paint => (paint < gameManager.Width * 100) ? Color.Transparent : Color.Transparent));
            AddableElements = new Button[1];
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
            Save = new Button(texture)
            {
                TextOnTop = new TextZone("Save", AlignOptions.Center),
                Position = new Vector2(Bounds.X - 80, 0),
            };
            Save.MouseClicked += Save_MouseClicked;
            Save.SetAlignOption(AlignOptions.TopRight);

            Exit = new Button(texture)
            {
                TextOnTop = new TextZone("Exit", AlignOptions.Center),
                Position = new Vector2(Bounds.X, 0), 
            };
            Exit.MouseClicked += Exit_MouseClicked;
            Exit.SetAlignOption(AlignOptions.TopRight);


            Delete = new Button(texture)
            {
                TextOnTop = new TextZone("Delete", AlignOptions.Center),
                Position = new Vector2(Bounds.X - 160, 0),
            };
            Delete.MouseClicked += Delete_MouseClicked;
            Delete.SetAlignOption(AlignOptions.TopRight);
            Delete.Disable(); 

            Upload = new Button(texture)
            {
                TextOnTop = new TextZone("Upload", AlignOptions.Center),
                Position = new Vector2(Bounds.X - 240, 0),
            };
            Upload.MouseClicked += Upload_MouseClicked; ;
            Upload.SetAlignOption(AlignOptions.TopRight);

            AddableElements[0] = wall; 
        }

        private void Upload_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            Hide();
            SaveMap();
            new UploadScreen(mapName).Show(); 
            //networkManager.UploadMap(mapName);
            //MessageBox.Show("The map has been uploaded"); 
        }

        private void Delete_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            //Delete.Disable();
            selectedSprite.Remove(); 
        }

        public override void Hide()
        {
            foreach (Sprite s in sprites)
                s.IsDisplayed = false; 
            base.Hide();
        }

        public override void UnHide()
        {
            foreach (Sprite s in sprites)
                s.IsDisplayed = true;
            base.UnHide();
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
            Delete.Enable();
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
            SaveMap(); 
            this.Close();
            gameManager.ClearScene(); 
            new MainMenu().Show(); 
        }

        public void SaveMap()
        {
            var sprites = gameManager.GetSprites().Where(s => s is Wall).ToArray(); // Interface to implemente later car on peut pas garder s is Wall, ça sera pas toujours des wall lol
            xmlManager.SaveMap(sprites, mapName);
        }

        private void Wall_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            Wall wall = new Wall(textureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige));
            wall.MouseClicked += MapElementMouseClicked;
            //Wall wall = new Wall(textureManager.LoadNoneContentLoadedTexture(texturePath));
            wall.isDraggable = true;
            sprites.Add(wall);
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

            AddChildSprite(Save); 
            AddChildSprite(Delete); 
            AddChildSprite(Exit); 
            AddChildSprite(Upload);
            base.Show();
        }
    }
}
