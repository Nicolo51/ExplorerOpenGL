using ExplorerOpenGL.Managers.Networking.NetworkObject;
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
        private DebugManager debugManager;
        private TextureManager textureManager; 
        private RenderManager renderManager;
        private NetworkManager networkManager;
        private FontManager fontManager;
        private ScripterManager scripterManager;
        private PauseMenu pauseMenu;

        public bool IsOnline { get { return networkManager.IsConnectedToAServer; } }

        private bool isGameStarted;  
        public Player Player { get; set; }
        public Terminal Terminal { get; private set; }
        public Camera Camera { get; private set; } 
        public MousePointer MousePointer { get; private set; }

        public List<Sprite> sprites { get; private set; }
        public Dictionary<int, Sprite> NetworkObjects { get; private set; }

        public int Height;
        public int Width;
        public GameState GameState { get; private set; }
        private GameState lastGameState;
        private bool hasGameStateChanged; 

        public int MainThreadID;  

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

        private GameManager()
        {
            NetworkObjects = new Dictionary<int, Sprite>(); 
            hasGameStateChanged = false; 
            Height = 730;
            Width = 1360;
            action = new List<Action<object>>();
            actionArg = new List<object>();
            isGameStarted = false;
        }

        public void InitDependencies(List<Sprite> sprites, Camera camera)
        {
            keyboardManager = KeyboardManager.Instance;
            textureManager = TextureManager.Instance;
            debugManager = DebugManager.Instance;
            fontManager = FontManager.Instance;
            networkManager = NetworkManager.Instance;
            scripterManager = ScripterManager.Instance; 

            keyboardManager.KeyPressed += OnKeyPressed;
            MainThreadID = Thread.CurrentThread.ManagedThreadId; 
            this.Camera = camera; 
            this.sprites = sprites;

            Terminal = new Terminal(textureManager.CreateTexture(700, 30, paint => Color.Black), fontManager.GetFont("Default")) { Position = new Vector2(0, 185) };
            
            MousePointer = new MousePointer(textureManager.LoadTexture("cursor"));
            pauseMenu = new PauseMenu();

            keyboardManager.KeyPressedSubTo(Keys.Escape, OnEscapePress);

            AddSprite(Terminal, this);
            AddSprite(MousePointer, this);
        }

        private void OnEscapePress()
        {
            if ((GameState == GameState.Playing || GameState == GameState.OnlinePlaying) && !hasGameStateChanged)
            {
                pauseMenu.Show();
                //Camera.FollowSprite(pauseMenu.ResumeButton);
            }
            else if (GameState == GameState.Pause)
            {
                pauseMenu.Close();
            }
        }

        public void AddActionToUIThread(Action<object> action, object arg)
        {
            lock (action)
            {
                lock (actionArg)
                {
                    this.action.Add(action);
                    this.actionArg.Add(arg);

                }
            }
        }


        public void StartGame(string name, string ip = null )
        {
            if (isGameStarted)
                return;
            Player Player = new Player(textureManager.LoadTexture("player"), textureManager.LoadTexture("playerfeet"), name)
            {
                Position = new Vector2(0, 0),
                input = new Input()
                {
                    Down = Keys.S,
                    Up = Keys.Z,
                    Left = Keys.Q,
                    Right = Keys.D,
                }
            };
            var wall = new Wall(new Animation(textureManager.LoadTexture("animTest"), 8, 500));
            AddSprite(wall, this);
            if(!string.IsNullOrWhiteSpace(ip))
            {
                if(!networkManager.Connect(ip, name))
                {
                    return; 
                }
                ChangeGameState(GameState.OnlinePlaying);
            }
            else
                ChangeGameState(GameState.Playing);
            AddSprite(Player, this);
            MousePointer.SetDefaultIcon(MousePointerType.Crosshair);
            MousePointer.SetCursorIcon(MousePointerType.Crosshair);
        }

        public void StopGame()
        {
            if (!isGameStarted)
                return;
            Camera.FollowSprite(null);
            RemoveSprite(Player);
            Player = null; 
            MousePointer.SetDefaultIcon(MousePointerType.Arrow);
        }

        public void Update(GameTime gametime)
        {
            hasGameStateChanged = false; 
            lock (action)
            {
                lock (actionArg)
                {
                    for (int i = 0; i < action.Count; i++)
                    {
                        action[i].Invoke(actionArg[i]);
                    }
                }
            }
            if (Player != null)
            {
                lock (Player)
                {
                    if (Player.IsRemove)
                        Player = null;
                }
            }
        }

        public void UpdateNetworkObjects(NetworkGameObject ngo)
        {
            lock (NetworkObjects)
               NetworkObjects[ngo.ID].NetworkUpdate(ngo);
        }

        public void AddSprite(Sprite sprite, object issuer)
        {
            if (sprite is Player && issuer is GameManager)
                Player = sprite as Player;
           
            SpriteAdded?.Invoke(sprite, issuer);
            sprite.SetPosition(sprite.Position);
            lock (this.sprites)
                this.sprites.Add(sprite); 
        }

        public void AddNetworkObject(Sprite s)
        {
            lock (NetworkObjects)
            {
                if (NetworkObjects.Keys.Contains(s.ID))
                    return; 
                NetworkObjects.Add(s.ID, s);
            }
            AddSprite(s, networkManager);
        }

        private void OnKeyPressed(KeysArray keys)
        {
            
            if (keys.Contains(Keys.F2))
            {
                Texture2D screenshot = renderManager.RenderSceneToTexture();

                Stream stream = File.Create(Environment.SpecialFolder.Desktop + "\\image.png");
                screenshot.SaveAsPng(stream, (int)Camera.Bounds.X, (int)Camera.Bounds.Y);
                stream.Dispose();
            }
            if (keys.Contains(Keys.F5))
            {
                if (Player != null)
                {
                    Camera.FollowSprite(Player);
                } 
                Camera.ToggleFollow();
            }
            if (keys.Contains(Keys.F1))
            {
                ClearScene(); 
            }
        }

        public void OnWindowResize(object sender, EventArgs e)
        {

        }

        public void RemoveSprite(Sprite sprite)
        {
            lock (sprites)
            {
                this.sprites.Remove(sprite);
            }
        }

        public void ChangeGameState(GameState gameState)
        {
            if (gameState == GameState)
                return; 
            lastGameState = GameState; 
            GameState = gameState;
            hasGameStateChanged = true;
        }

        public void ChangeToLastGameState()
        {
            GameState temps = GameState;
            GameState = lastGameState;
            lastGameState = temps;
            hasGameStateChanged = true; 
        }

        public Sprite[] GetSprites()
        {
            lock (sprites)
                return sprites.ToArray(); 
        }

        public Sprite[] GetNetworkObjects()
        {
            lock (NetworkObjects)
                return NetworkObjects.Values.ToArray();
        }

        public Sprite GetNetworkObject(int id)
        {
            lock (NetworkObjects)
            {
                if (NetworkObjects.ContainsKey(id))
                    return NetworkObjects[id];
                return null; 
            }
        }

        public void RemoveNetworkObjects(int id)
        {
            Sprite s = null;
            lock (NetworkObjects)
            {
                if (NetworkObjects.ContainsKey(id))
                {
                    s = NetworkObjects[id];
                    NetworkObjects.Remove(id);
                }
            }
            if(s != null)
                RemoveSprite(s); 
        }

        public void ClearScene()
        {
            lock (sprites)
            {
                for (int i = 0; i < this.sprites.Count; i++)
                {
                    var sprite = this.sprites[i];
                    if (!(sprite is Terminal || sprite is MousePointer))
                    {
                        sprites[i].Remove(); 
                        sprites.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void ToMainMenu()
        {
            if (networkManager.IsConnectedToAServer)
                networkManager.Disconnect();
            ClearScene();
            StopGame(); 
            new MainMenu().Show();
        }
    }

    public enum GameState
    {
        Playing, 
        OnlinePlaying, 
        Pause, 
        MainMenu, 
        Typing,
    }
}
