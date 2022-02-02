using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class ScripterManager
    {
        public static EventHandler InstanceCreated; 
        private static ScripterManager instance;
        public static event EventHandler Initialized;

        private GameManager gameManager;
        private FontManager fontManager;
        private KeyboardManager keyboardManager; 
        private TextureManager textureManager;
        private NetworkManager networkManager;
        private DebugManager debugManager;

        public static ScripterManager Instance { get { 
                if (instance == null) 
                { 
                    instance = new ScripterManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance; 
                } 
                    return instance;
            }
        }

        private ScripterManager()
        {
           
        }

        public void InitDependencies()
        {
            keyboardManager = KeyboardManager.Instance;
            textureManager = TextureManager.Instance;
            fontManager = FontManager.Instance;
            gameManager = GameManager.Instance;
            networkManager = NetworkManager.Instance;
            debugManager = DebugManager.Instance; 
        }

        public void CreateMenu()
        {
            gameManager.Camera.LookAt(0, 0);
            gameManager.ClearScene(); 
            var sp = new Button(textureManager.OutlineText("Singleplayer", "Menu", Color.Black, new Color(4, 136, 201), 1), textureManager.OutlineText("Singleplayer", "Menu", Color.Black, new Color(4, 136, 201), 2)) 
            { Position = new Vector2(0, -200)};
            var mp = new Button(textureManager.OutlineText("Multiplayer", "Menu", Color.Black, new Color(4, 136, 201), 1), textureManager.OutlineText("Multiplayer", "Menu", Color.Black, new Color(4, 136, 201), 2)) 
            { Position = new Vector2(0, 0) };
            var option = new Button(textureManager.OutlineText("Options", "Menu", Color.Black, new Color(4, 136, 201), 1), textureManager.OutlineText("Options", "Menu", Color.Black, new Color(4, 136, 201), 2)) 
            { Position = new Vector2(0, 200) };
            
            sp.MouseClicked += LaunchSinglePlayer;
            mp.MouseClicked += DisplayMultiplayerForm;
            option.MouseClicked += DisplayOptionMenu;

            gameManager.AddSprite(sp, this);
            gameManager.AddSprite(mp, this);
            gameManager.AddSprite(option, this);
        }


        public void DisplayMultiplayerForm(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            gameManager.Camera.LookAt(new Vector2(0, 0));
            gameManager.ClearScene();
            var ti = new TextinputBox(textureManager.CreateTexture(200, 50, e => Color.Black), fontManager.GetFont("Default"), false);
            ti.Validated += ConnectToServer;
            
            gameManager.AddSprite(ti, this);
        }

        private void ConnectBtnClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            
        }

        public void ConnectToServer(string message, TextinputBox textinput)
        {
            debugManager.AddEvent(message);
            //networkManager.Connect(message.Trim()); 
        }

        public void LaunchSinglePlayer(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {

        }
        public void DisplayOptionMenu(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {

        }

    }
}
