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
            var mb = MessageBox.Show("Error", "Je suis un message un peu long qui sera donc wrappé par mon prog", MessageBoxType.YesNo);
            mb.Result += GetResult;
        }

        private void GetResult(MessageBox sender, MessageBoxResultEventArgs e)
        {
            sender.Close(); 
            MessageBox.Show("Vous avez cliqué sur " + e.MessageBoxResult.ToString());
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
