using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class GameManager 
    {
        private List<Action<object>> action;
        private List<object> actionArg; 

        private KeyboardManager keyboardManager;
        private DebugManager debugManager; //instantiate on load
        private TextureManager textureManager; //instantiate on load 
        private RenderManager renderManager;
        private NetworkManager networkManager;
        private FontManager fontManager; 

        public Player Player { get; set; }
        public Terminal Terminal { get; private set; }
        public TextinputBox TerminalTexintput; 
        public Camera Camera { get; private set; } 
        public MousePointer MousePointer { get; private set; }

        public List<Sprite> _sprites { get; private set; }

        public static event EventHandler Initialized;
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }

        

        public delegate void AddSpriteEventHandler(Sprite sprite, object issuer);
        public event AddSpriteEventHandler SpriteAdded; 

        public GameManager()
        {
            action = new List<Action<object>>();
            actionArg = new List<object>();
        }

        public void InitDependencies(List<Sprite> sprites, Camera camera)
        {
            keyboardManager = KeyboardManager.Instance;
            textureManager = TextureManager.Instance;
            debugManager = DebugManager.Instance;
            fontManager = FontManager.Instance;
            networkManager = NetworkManager.Instance;

            keyboardManager.KeyPressed += debugManager.AddEvent;
            keyboardManager.KeyRealeased += debugManager.AddEvent;

            InitKeyEvent();

            this.Camera = camera; 
            _sprites = sprites;

            Terminal = new Terminal(textureManager.CreateTexture(700, 30, paint => Color.Black), fontManager.GetFont("Default")) { Position = new Vector2(0, 185) };
            TerminalTexintput = new TextinputBox(textureManager.CreateTexture(700, 35, paint => Color.Black * .8f), fontManager.GetFont("Default"), true) { IsHUD = true, Position = new Vector2(0, 695), Opacity = 0f, };
            TerminalTexintput.Validated += Terminal.OnTextinputValidation;
            MousePointer = new MousePointer(textureManager.LoadTexture("cursor"));

            _sprites.Add(Terminal);
            _sprites.Add(TerminalTexintput);
            _sprites.Add(MousePointer);

            
        }

        public void AddActionToUIThread(Action<object> action, object arg)
        {
            this.action.Add(action);
            this.actionArg.Add(arg);
        }


        private void OnQueried(string message)
        {
            if (networkManager.IsConnectedToAServer && (message = message.Trim()) != String.Empty)
            {
                networkManager.SendMessageToServer(message);
            }
        }

        public void Update(List<Sprite> sprites, GameTime gametime)
        {
            for(int i = 0; i < action.Count; i++)
            {
                action[i].Invoke(actionArg[i]); 
            }
            if(keyboardManager != null && textureManager != null && debugManager != null && networkManager != null )
            {
                keyboardManager.Update();
                debugManager.Update(sprites);
                if (networkManager.IsConnectedToAServer)
                {
                    networkManager.Update(gametime, Player);
                }
            }
            else
            {
                throw new NullReferenceException("Toutes les instances des controllers doivent être initialisées"); 
            }

        }

        private void InitKeyEvent()
        {
            keyboardManager.KeyPressed += OnKeyPressed;
            keyboardManager.KeyRealeased += OnKeyRealeased;
        }

        private void OnKeyRealeased(Keys[] keys, KeyboardManager KeyboardManager)
        {
            debugManager.AddEvent("Key realeased : " + new KeysArray(keys), KeyboardManager);
        }

        public void AddSprite(Sprite sprite, object issuer)
        {
            SpriteAdded?.Invoke(sprite, issuer); 
            _sprites.Add(sprite); 
        }

        public void UnFocusAll()
        {
            foreach (TextinputBox t in _sprites.Where(e => e is TextinputBox))
            {
                t.UnFocus();
            }
        }

        private void OnKeyPressed(Keys[] keys, KeyboardManager KeyboardManager)
        {
            if (KeyboardManager.Contains(keys, Keys.Enter))
            {
                TerminalTexintput.ToggleFocus(true); 
            }
            if (KeyboardManager.Contains(keys, Keys.OemQuestion))
            {
                Terminal.AddMessageToTerminal("This is a ne message", "Client", Color.Green); 
            }
            if (KeyboardManager.Contains(keys, Keys.F3))
            {
                debugManager.ToggleDebugMode(_sprites);
            }
            if (KeyboardManager.Contains(keys, Keys.F2))
            {
                Texture2D screenshot = renderManager.RenderSceneToTexture();

                Stream stream = File.Create(@"C:\Users\nicol\Desktop\image.png");
                screenshot.SaveAsPng(stream, (int)Camera.Bounds.X, (int)Camera.Bounds.Y);
                stream.Dispose();
            }
            if (KeyboardManager.Contains(keys, Keys.F5))
            {
                Camera.ToggleFollow();
            }
            if (KeyboardManager.Contains(keys, Keys.F1))
            {
                using (StreamReader sr = new StreamReader("ip.txt"))
                {
                    string ip = sr.ReadLine();
                    networkManager.Connect(ip);
                }
            }
            debugManager.AddEvent("Key pressed : " + new KeysArray(keys), KeyboardManager);
        }

        public void OnWindowResize(object sender, EventArgs e)
        {

        }

        public void RemoveSprite(Sprite sprite)
        {
            _sprites.Remove(sprite);
        }

        public void ClearScene()
        {
            for (int i = 0; i < _sprites.Count; i++)
            {
                var sprite = _sprites[i];
                if (!(sprite is Terminal || sprite is MousePointer))
                {
                    _sprites.Remove(sprite);
                    i--;
                }
            }
        }
    }
}
