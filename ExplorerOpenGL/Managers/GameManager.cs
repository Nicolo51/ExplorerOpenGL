using ExplorerOpenGL2.Managers.Networking.EventArgs;
using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using ExplorerOpenGL2.View;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ExplorerOpenGL2.Managers
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
        private XmlManager xmlManager;
        private NetGameState netGameState;
        private GraphicsDeviceManager graphics; 

        private PauseMenu pauseMenu;

        public bool IsOnline { get { return networkManager.IsConnectedToAServer; } }

        public Player Player { get; set; }
        public Terminal Terminal { get; private set; }
        public Camera Camera { get; private set; } 
        public MousePointer MousePointer { get; private set; }

        public List<Sprite> sprites { get; private set; } //accessing without lock might crash the game or make it unstable
        public Dictionary<int, Sprite> spriteById { get; private set; }
        public Dictionary<int, Type> IdToSpriteType { get; set; }
        public Dictionary<Type, int> SpriteTypeToId{ get; set; }

        public Dictionary<int, Sprite> NetworkObjects { get; private set; }

        public int Height;
        public int Width;
        public GameState GameState { get; private set; }
        private GameState lastGameState;
        private bool hasGameStateChanged;
        private int IDS = 0; 

        public string CurrentMap { get; private set; }
        public int MainThreadID { get; set; }  
        
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
            action = new List<Action<object>>();
            actionArg = new List<object>();
            netGameState = new NetGameState(); 
        }

        public void InitDependencies(Camera camera, GraphicsDeviceManager graphics)
        {
            keyboardManager = KeyboardManager.Instance;
            textureManager = TextureManager.Instance;
            debugManager = DebugManager.Instance;
            fontManager = FontManager.Instance;
            scripterManager = ScripterManager.Instance; 
            xmlManager = XmlManager.Instance;

            this.graphics = graphics;
            Width = graphics.PreferredBackBufferWidth;
            Height = graphics.PreferredBackBufferHeight;

            keyboardManager.KeyPressed += OnKeyPressed;
            MainThreadID = Thread.CurrentThread.ManagedThreadId; 
            this.Camera = camera;
            this.sprites = new List<Sprite>();
            this.spriteById = new Dictionary<int, Sprite>();

            networkManager = NetworkManager.Instance;
            Terminal = new Terminal(textureManager.CreateTexture(700, 30, paint => Color.Black), fontManager.GetFont("Default")) { Position = new Vector2(0, 185) };
            
            MousePointer = new MousePointer(textureManager.LoadTexture("cursor"));
            pauseMenu = new PauseMenu();

            keyboardManager.KeyPressedSubTo(Keys.Escape, OnEscapePress);
            AddSprite(Terminal, this);
            AddSprite(MousePointer, this);


            IdToSpriteType = new Dictionary<int, Type>()
            {
                { 0, typeof(Sprite) },
                { 1, typeof(Player) },
                { 2, typeof(Wall) },
            };

            SpriteTypeToId = new Dictionary<Type, int>()
            {
                { typeof(Sprite), 0 },
                { typeof(Player), 1 },
                { typeof(Wall), 2 },
            };
            
            
        }

        private void OnEscapePress()
        {
            if ((GameState == GameState.Playing || GameState == GameState.OnlinePlaying) && !hasGameStateChanged)
            {
                pauseMenu = new PauseMenu();
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

        public void StartGame(string name, string ip = null, string mapName = null,  bool isServer = false)
        {
            Texture2D texture = textureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Green);
            CurrentMap = mapName; 
            //AddSprite(new Wall(texture) { Position = new Vector2(0, 100) }, this);
            //AddSprite(new Wall(texture) { Position = new Vector2(0, -150) }, this);
            //AddSprite(new Wall(texture) { Position = new Vector2(600, 0) }, this);
            //AddSprite(new Wall(texture) { Position = new Vector2(-100, 600) }, this);
            //AddSprite(new Wall(textureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige)) { Position = new Vector2(0, 100) }, this);
            //AddSprite(new Wall(textureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige)) { Position = new Vector2(0, 100) }, this);
            //AddSprite(new Wall(textureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige)) { Position = new Vector2(0, 100) }, this);

            if (!string.IsNullOrWhiteSpace(ip))
            {
                networkManager.PacketReceived += Connected;
                if (!networkManager.Connect(ip, name, isServer))
                {
                    return;
                }
                ChangeGameState(GameState.OnlinePlaying);

            }
            else
            {
                ChangeGameState(GameState.Playing);
                MousePointer.SetDefaultIcon(MousePointerType.Crosshair);
                MousePointer.SetCursorIcon(MousePointerType.Crosshair);
                AddSprite(Player, this);
            }
        }

        private void Connected(Networking.EventArgs.NetworkEventArgs e)
        {
            if(e is WelcomeEventArgs)
            {
                MousePointer.SetDefaultIcon(MousePointerType.Crosshair);
                MousePointer.SetCursorIcon(MousePointerType.Crosshair);
                CurrentMap = (e as WelcomeEventArgs).MapName;
                if (networkManager.IsServer)
                {
                    Sprite[] mapSprites = xmlManager.GenerateSpritesFromXml(xmlManager.LoadMap(CurrentMap));
                    AddSprites(mapSprites, this);
                }
                Terminal.AddMessageToTerminal("map and player loaded", "System", Color.Yellow);
                networkManager.PacketReceived -= Connected;
            }
        }

        public void StopGame()
        {
            Camera.FollowSprite(null);
            Camera.ToggleFollow(false); 
            ClearScene();
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
                    action.Clear(); 
                    actionArg.Clear();
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

            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] == null)
                    continue;
                lock (sprites[i])
                {
                    if (!sprites[i].IsEnable)
                        continue; 
                    if (sprites[i].IsRemove)
                    {
                        RemoveSprite(sprites[i]);
                        if (i < 0)
                            i--;
                        continue;
                    }
                    sprites[i].Update(sprites, gametime, netGameState);
                }
            }
            networkManager.Update(gametime, netGameState);
        }

        public void AddSprite(Sprite sprite, object issuer)
        {
            if (sprite is Player && (sprite as Player).input != null)
            {
                Player = sprite as Player;
                sprite.IsEnable = true;
            }

            if (sprites.Contains(sprite))
                return; 

            SpriteAdded?.Invoke(sprite, issuer);

            sprite.SetPosition(sprite.Position);
            int spriteid = GetId();
            if (spriteid != -1 && sprite.IsPartOfGameState)
            {
                sprite.ID = GetId();
                spriteById.Add(sprite.ID, sprite);
            }
            else if(sprite.ID > 0)
            {
                if (!spriteById.ContainsKey(sprite.ID))
                    spriteById.Add(sprite.ID, sprite);
            }

            sprite.IsEnable = true; 

            this.sprites.Add(sprite);
            this.sprites = sprites.OrderByDescending(s => s.LayerDepth).ToList();  
        }

        public void AddSprites(Sprite[] sprites, object issuer)
        {
            foreach (Sprite s in sprites)
                AddSprite(s, issuer); 
        }

        private void OnKeyPressed(KeysArray keys)
        {
            
            //if (keys.Contains(Keys.F2))
            //{
            //    Texture2D screenshot = renderManager.RenderSceneToTexture();

            //    Stream stream = File.Create(Environment.SpecialFolder.Desktop + "\\image.png");
            //    screenshot.SaveAsPng(stream, (int)Camera.Bounds.X, (int)Camera.Bounds.Y);
            //    stream.Dispose();
            //}
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

        public void SortSprites()
        {
            lock (sprites)
            {
                sprites = sprites.OrderByDescending(s => s.LayerDepth).ToList(); 
            }
        }

        public Sprite[] GetSprites()
        {
            lock (sprites)
                return sprites.ToArray(); 
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
            
            if (NetworkObjects.ContainsKey(id))
            {
                s = NetworkObjects[id];
                NetworkObjects.Remove(id);
            }
            if(s != null)
                RemoveSprite(s); 
        }

        public int GetIndexOf(Sprite sprite)
        {
            return sprites.IndexOf(sprite);
        }

        public void ClearScene()
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
            spriteById.Clear(); 
        }

        public void ToMainMenu()
        {
            if (networkManager.IsConnectedToAServer)
                networkManager.Disconnect();
            StopGame();
            new MainMenu().Show();
        }

        public int GetId()
        {
            if(networkManager.IsServer)
                return IDS++; 
            return -1;
        }

        internal Sprite GetSpriteById(int fromClient)
        {
            return sprites.FirstOrDefault(s => s.ID == fromClient);
        }

        internal void RemoveSprite(int fromClient)
        {
            GetSpriteById(fromClient).Remove();
        }

        public Player AddPlayer()
        {
            var player = CreatePlayer(); 
            AddSprite(player, this);
            return player; 
        }

        public Sprite[] GetPlayers()
        {
            return sprites.Where(s => s is Player).ToArray();
        }

        public void UpdateSprite(GameStateEventArgs gs)
        {           
            if (spriteById.ContainsKey(gs.ID) && spriteById[gs.ID].GetType() == IdToSpriteType[gs.Type])
            {
                spriteById[gs.ID].ReadGameState(gs.Packet);
                return; 
            }   

            Sprite sprite = CreateInstance(gs.Type);
            sprite.ID = gs.ID;
            AddSprite(sprite, this);
            1spriteById[gs.ID].ReadGameState(gs.Packet); 
        }
        public Sprite CreateInstance(int type)
        {
            Sprite sprite = null;
            switch (type) 
            { 
                case 0:
                    sprite = CreateSprite(); 
                    break;
                case 1:
                    sprite = CreatePlayer(); 
                    break;
                case 2:
                    sprite = CreateWall();
                    break;
            }
            return sprite;
        }

        public byte[] GetMap()
        {
            NetDataWriter mapPacket = new NetDataWriter();

            Sprite[] spritesToSend = sprites.Where(s => !s.IsHUD && s.GetType() != typeof(Player)).ToArray();

            string xml = xmlManager.GetMapXmlBySprites(spritesToSend, CurrentMap);
            
            return Encoding.UTF8.GetBytes(xml);
        }

        private Wall CreateWall()
        {
            return new Wall();
        }
        private Player CreatePlayer()
        {
            Animation walking = textureManager.GetAnimation("walk");
            Animation standing = textureManager.GetAnimation("idle");
            Animation running = textureManager.GetAnimation("run");
            Animation jump = textureManager.GetAnimation("jump");
            Animation falling = textureManager.GetAnimation("falling");
            jump.IsLooping = false;

            var player = new Player("???", textureManager.NormalizeHeights(walking, standing, running, jump, falling))
            {
                Position = new Vector2(0, 0),
                IsEnable = false,
            };
            return player;
        }

        private Sprite CreateSprite()
        {
            return null;
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
