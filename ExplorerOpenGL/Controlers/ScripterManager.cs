using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers
{
    public class ScripterManager
    {
        public static EventHandler InstanceCreated; 
        private static ScripterManager instance;
        private Controler controler;
        private List<Sprite> sprites;
        public void SetControlerAndComponents(Controler controler, List<Sprite> sprites)
        {
            this.controler = controler;
            this.sprites = sprites;
        }
        public static ScripterManager Instance { get { 
                if (instance == null) 
                { 
                    instance = new ScripterManager();
                    InstanceCreated?.Invoke(instance, null);
                    return instance; 
                } 
                else 
                    return instance; } }

        private ScripterManager() { } 
        private void ClearScene()
        {
            for (int i = 0; i < instance.sprites.Count; i++)
            {
                var sprite = instance.sprites[i];
                if (!(sprite is Terminal || sprite is MousePointer))
                {
                    instance.sprites.Remove(sprite);
                    i--;
                }
            }
        }

        public void CreateMenu()
        {
            ClearScene();
            var sp = new Button(instance.controler.TextureManager.OutlineText("Singleplayer", "Menu", Color.Black, new Color(4, 136, 201), 1), instance.controler.TextureManager.OutlineText("Singleplayer", "Menu", Color.Black, new Color(4, 136, 201), 2)) 
            { Position = new Vector2(0, -200)};
            var mp = new Button(instance.controler.TextureManager.OutlineText("Multiplayer", "Menu", Color.Black, new Color(4, 136, 201), 1), instance.controler.TextureManager.OutlineText("Multiplayer", "Menu", Color.Black, new Color(4, 136, 201), 2)) 
            { Position = new Vector2(0, 0) };
            var option = new Button(instance.controler.TextureManager.OutlineText("Options", "Menu", Color.Black, new Color(4, 136, 201), 1), instance.controler.TextureManager.OutlineText("Options", "Menu", Color.Black, new Color(4, 136, 201), 2)) 
            { Position = new Vector2(0, 200) };
            
            sp.MouseClicked += LaunchSinglePlayer;
            mp.MouseClicked += AskNameForMultiplayer;
            option.MouseClicked += DisplayOptionMenu;

            instance.controler.AddSprite(sp);
            instance.controler.AddSprite(mp);
            instance.controler.AddSprite(option);
        }


        public void AskNameForMultiplayer(object sender, MousePointer mousePointer, Controler controler, Vector2 clickPosition)
        {
            controler.Camera.LookAt(new Vector2(0, 0));
            ClearScene();
            var ti = new TextinputBox(controler.TextureManager.CreateTexture(200, 50, e => Color.Black), controler.Fonts["Default"], controler.KeyboardUtils, false);
            ti.Validated += ConnectToServer;
            instance.sprites.Add(ti);
        }

        public void ConnectToServer(string message, TextinputBox textinput)
        {
            instance.controler.NetworkManager.Connect(message);
        }

        public void LaunchSinglePlayer(object sender, MousePointer mousePointer, Controler controler, Vector2 clickPosition)
        {

        }
        public void DisplayOptionMenu(object sender, MousePointer mousePointer, Controler controler, Vector2 clickPosition)
        {

        }

    }
}
