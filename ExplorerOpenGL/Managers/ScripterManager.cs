using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
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

    }
}
