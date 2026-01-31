using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.View
{
    public class MapEditor : MessageBoxIG
    {
        Button[] AddableElements;
        Button Save;
        Button Exit;
        Button Delete;
        Button DeleteMap;
        Button Upload;
        List<Sprite> sprites; 
        Sprite selectedSprite; 

        XmlManager xmlManager;
        KeyboardManager keyboardManager;
        NetworkManager networkManager;

        LoadingScreen loadingScreen; 

        public string MapName { get; private set; }
        
        string mapName; 
        public MapEditor(string mapName)
        {
            sprites = new List<Sprite>(); 
            networkManager = NetworkManager.Instance; 
            this.mapName = mapName; 
            xmlManager = XmlManager.Instance;
            keyboardManager = KeyboardManager.Instance; 

            

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
            Texture2D textureDeletemap = textureManager.CreateBorderedTexture(135, 40, 2, 0, paint => Color.Black, paint => Color.DarkRed);
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

            DeleteMap = new Button(textureDeletemap)
            {
                TextOnTop = new TextZone("Delete Map", AlignOptions.Center),
                Position = new Vector2(Bounds.X - 320, 0),
            };
            DeleteMap.MouseClicked += DeleteMap_MouseClicked;
            DeleteMap.SetAlignOption(AlignOptions.TopRight);
            DeleteMap.Enable();



            AddableElements[0] = wall;
            loadingScreen = new LoadingScreen();
            loadingScreen.Show();
            ThreadManager.Instance.StartThread(LoadMap, delegate(object arg) { UnHide(); }); ;
            this.Hide(); 
        }

        private void DeleteMap_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            MessageBoxIG.Show("Are you sure you want to delete this map ?", "Confirmation", MessageBoxIGType.YesNo).Result += MapEditor_Result; ; 
        }

        private void MapEditor_Result(MessageBoxIG sender, MessageBoxIGResultEventArgs e)
        {
            if(e.MessageBoxIGResult == MessageBoxIGResult.Yes)
            {
                if (Directory.Exists($"./maps/{mapName}"))
                    Directory.Delete($"./maps/{mapName}", true);
                if (File.Exists($"./maps/{mapName}.xml"))
                    File.Delete($"./maps/{mapName}.xml");
                sender.Close(); 
                Exit_MouseClicked(this, null, Vector2.Zero);
            }
            else
            {
                sender.Close(); 
            }
        }

        public object LoadMap()
        {
            MapXml[] mapXml = xmlManager.LoadMap(mapName);
            Sprite[] map = new Sprite[mapXml.Length]; 
            for(int i = 0; i < mapXml.Length; i++)
            {
                map[i] = xmlManager.GenerateSpriteFromXml(mapXml[i].node, mapXml[i].mapName);
                map[i].isDraggable = true; 
                loadingScreen.ChangePercent((i + 1) / (float)mapXml.Length * 100); 
            }
            foreach (var s in map)
            {
                gameManager.AddSprite(s, this); 
                sprites.Add(s);
                s.MouseClicked += MapElementMouseClicked;
            }
            loadingScreen.Close(); 
            return null; 
        }

        private void Upload_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            Hide();
            SaveMap();
            var uploadScreen = new UploadScreen(mapName);
            uploadScreen.UploadEnded += UploadScreen_UploadEnded;
            uploadScreen.Show(); 
            //networkManager.UploadMap(mapName);
            //MessageBoxIG.Show("The map has been uploaded"); 
        }

        private void UploadScreen_UploadEnded(object sender, bool success, string mapName)
        {
            var mb = MessageBoxIG.Show("The upload was successful.", "Info", MessageBoxIGType.Ok);
            mb.Result += OnBackToEditPressed;
            (sender as UploadScreen).Close();
        }

        private void OnBackToEditPressed(MessageBoxIG sender, MessageBoxIGResultEventArgs e)
        {
            (sender as MessageBoxIG).Close(); 
            UnHide(); 
        }

        private void Delete_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            if (selectedSprite == null)
                return; 
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
            AddChildSprite(DeleteMap); 
            base.Show();
        }
    }
}
