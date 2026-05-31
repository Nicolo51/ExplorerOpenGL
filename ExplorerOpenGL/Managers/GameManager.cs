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
        private static List<Action<object>> actions = new List<Action<object>>();
        private static List<object> actionArg = new List<object>(); 

        private static NetGameState netGameState = new NetGameState();
        private static GraphicsDeviceManager Graphics;
        private static Game1 Game; 

        private static PauseMenu pauseMenu;

        public static bool IsOnline { get { return NetworkManager.IsConnectedToAServer; } }

        public static Player Player { get; set; }
        public static Terminal Terminal { get; private set; }
        public static Camera Camera { get; private set; } 
        public static MousePointer MousePointer { get; private set; }

        public static List<Sprite> sprites { get; private set; } //accessing without lock might crash the game or make it unstable
        public static Dictionary<int, Sprite> spriteById { get; private set; }
        public static Dictionary<int, Type> IdToSpriteType { get; set; }
        public static Dictionary<Type, int> SpriteTypeToId{ get; set; }
        public static Dictionary<int, Sprite> NetworkObjects { get; private set; } = new Dictionary<int, Sprite>();

        public static int Height { get { return Graphics.PreferredBackBufferHeight;  } }
        public static int Width { get { return Graphics.PreferredBackBufferWidth; } }
        public static GameState GameState { get; private set; }
        private static GameState lastGameState;
        private static bool hasGameStateChanged = false;
        private static int IDS = 0; 

        public static string CurrentMap { get; private set; }
        public static int MainThreadID { get; set; }  
        
        public delegate void AddSpriteEventHandler(Sprite sprite);
        public static event AddSpriteEventHandler SpriteAdded;

        public delegate void RemoveSpriteEventHandler(Sprite sprite);
        public static event AddSpriteEventHandler SpriteRemoved;


        public static void InitDependencies(GraphicsDeviceManager graphics, Game1 game)
        {
            Graphics = graphics;
            Game = game; 
            KeyboardManager.KeyPressed += OnKeyPressed;
            MainThreadID = Thread.CurrentThread.ManagedThreadId; 
            Camera = new Camera(new Vector2(Width, Height));
            sprites = new List<Sprite>();
            spriteById = new Dictionary<int, Sprite>();

            Terminal = new Terminal(TextureManager.CreateTexture(700, 30, paint => Color.Black), FontManager.GetFont("Default")) { Position = new Vector2(0, 185) };
            
            MousePointer = new MousePointer(TextureManager.LoadTexture("cursor"));
            pauseMenu = new PauseMenu();

            KeyboardManager.KeyPressedSubTo(Keys.Escape, OnEscapePress);
            AddSprite(Terminal);
            AddSprite(MousePointer);

            TextureManager.InitDefaultTextures();

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

        private static void OnEscapePress()
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

        public static void AddActionToUIThread(Action<object> action, object arg)
        {
            lock (action)
            {
                lock (actionArg)
                {
                    actions.Add(action);
                    actionArg.Add(arg);

                }
            }
        }

        public static void StartGame(string name, string ip = null, string mapName = null,  bool isServer = false)
        {
            Texture2D texture = TextureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Green);
            CurrentMap = mapName; 
            //AddSprite(new Wall(texture) { Position = new Vector2(0, 100) });
            //AddSprite(new Wall(texture) { Position = new Vector2(0, -150) });
            //AddSprite(new Wall(texture) { Position = new Vector2(600, 0) });
            //AddSprite(new Wall(texture) { Position = new Vector2(-100, 600) });
            //AddSprite(new Wall(TextureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige)) { Position = new Vector2(0, 100) });
            //AddSprite(new Wall(TextureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige)) { Position = new Vector2(0, 100) });
            //AddSprite(new Wall(TextureManager.CreateBorderedTexture(300, 75, 5, 0, paint => Color.Black, paint => Color.Beige)) { Position = new Vector2(0, 100) });

            if (!string.IsNullOrWhiteSpace(ip))
            {
                NetworkManager.PacketReceived += Connected;
                if (!NetworkManager.Connect(ip, name, isServer))
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
                AddSprite(Player);
            }
        }

        private static void Connected(Networking.EventArgs.NetworkEventArgs e)
        {
            if(e is WelcomeEventArgs)
            {
                MousePointer.SetDefaultIcon(MousePointerType.Crosshair);
                MousePointer.SetCursorIcon(MousePointerType.Crosshair);
                CurrentMap = (e as WelcomeEventArgs).MapName;
                if (NetworkManager.IsServer)
                {
                    Sprite[] mapSprites = XmlManager.GenerateSpritesFromXml(XmlManager.LoadMap(CurrentMap));
                    AddSprites(mapSprites);
                }
                Terminal.AddMessageToTerminal("map and player loaded", "System", Color.Yellow);
                NetworkManager.PacketReceived -= Connected;
            }
        }

        public static void SetViewport(int width, int height)
        {
            Graphics.PreferredBackBufferHeight = height;
            Graphics.PreferredBackBufferWidth = width;

            Terminal.SetPosition(new Vector2((float)width, (float)height)); 
            Camera.SetBounds(width, height);
            Graphics.ApplyChanges(); 
        }

        public static void ToggleFullScreen(bool isFullScreen)
        {
            Graphics.IsFullScreen = isFullScreen;
            Graphics.ApplyChanges(); 
        }

        public static void StopGame()
        {
            Camera.FollowSprite(null);
            Camera.ToggleFollow(false); 
            ClearScene();
            RemoveSprite(Player);
            Player = null; 
            MousePointer.SetDefaultIcon(MousePointerType.Arrow);
        }

        public static void Exit()
        {
            Game.Exit();
        }

        public static void Update(GameTime gametime)
        {
            hasGameStateChanged = false; 
            lock (actions)
            {
                lock (actionArg)
                {
                    for (int i = 0; i < actions.Count; i++)
                    {
                        actions[i].Invoke(actionArg[i]);
                    }
                    actions.Clear(); 
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
            NetworkManager.Update(gametime, netGameState);
        }

        public static void AddSprite(Sprite sprite)
        {
            if (sprite is Player && (sprite as Player).input != null)
            {
                Player = sprite as Player;
                sprite.IsEnable = true;
            }

            if (sprites.Contains(sprite))
                return; 

            SpriteAdded?.Invoke(sprite);

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

            sprites.Add(sprite);
            sprites = sprites.OrderByDescending(s => s.LayerDepth).ToList();  
        }

        public static void AddSprites(Sprite[] sprites)
        {
            foreach (Sprite s in sprites)
                AddSprite(s); 
        }

        private static void OnKeyPressed(KeysArray keys)
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

        public static void OnWindowResize(object sender, EventArgs e)
        {

        }

        public static void RemoveSprite(Sprite sprite)
        {
            SpriteRemoved?.Invoke(sprite); 
            sprites.Remove(sprite);
        }

        public static void ChangeGameState(GameState gameState)
        {
            if (gameState == GameState)
                return; 
            lastGameState = GameState; 
            GameState = gameState;
            hasGameStateChanged = true;
        }

        public static void ChangeToLastGameState()
        {
            GameState temps = GameState;
            GameState = lastGameState;
            lastGameState = temps;
            hasGameStateChanged = true; 
        }

        public static void SortSprites()
        {
            lock (sprites)
            {
                sprites = sprites.OrderByDescending(s => s.LayerDepth).ToList(); 
            }
        }

        public static Sprite[] GetSprites()
        {
            lock (sprites)
                return sprites.ToArray(); 
        }

        public static Sprite GetNetworkObject(int id)
        {
            lock (NetworkObjects)
            {
                if (NetworkObjects.ContainsKey(id))
                    return NetworkObjects[id];
                return null; 
            }
        }

        public static void RemoveNetworkObjects(int id)
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

        public static int GetIndexOf(Sprite sprite)
        {
            return sprites.IndexOf(sprite);
        }

        public static void ClearScene()
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                if (!(sprite is Terminal || sprite is MousePointer))
                {
                    sprites[i].Remove();
                    SpriteRemoved?.Invoke(sprites[i]);
                    sprites.RemoveAt(i);
                    i--;
                }
            }
            spriteById.Clear(); 
        }

        public static void ToMainMenu()
        {
            if (NetworkManager.IsConnectedToAServer)
                NetworkManager.Disconnect();
            StopGame();
            new MainMenu().Show();
        }

        public static int GetId()
        {
            if(NetworkManager.IsServer)
                return IDS++; 
            return -1;
        }

        public static Sprite GetSpriteById(int fromClient)
        {
            return sprites.FirstOrDefault(s => s.ID == fromClient);
        }

        public static void RemoveSprite(int fromClient)
        {
            GetSpriteById(fromClient).Remove();
        }

        public static Player AddPlayer()
        {
            var player = CreatePlayer(); 
            AddSprite(player);
            return player; 
        }

        public static Sprite[] GetPlayers()
        {
            return sprites.Where(s => s is Player).ToArray();
        }

        public static void UpdateSprite(GameStateEventArgs gs)
        {           
            if (spriteById.ContainsKey(gs.ID) && spriteById[gs.ID].GetType() == IdToSpriteType[gs.Type])
            {
                spriteById[gs.ID].ReadGameState(gs.Packet);
                return; 
            }   

            Sprite sprite = CreateInstance(gs.Type);
            sprite.ID = gs.ID;
            AddSprite(sprite);
            spriteById[gs.ID].ReadGameState(gs.Packet); 
        }
        public static Sprite CreateInstance(int type)
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

        public static byte[] GetMap()
        {
            NetDataWriter mapPacket = new NetDataWriter();

            Sprite[] spritesToSend = sprites.Where(s => !s.IsHUD && s.GetType() != typeof(Player)).ToArray();

            string xml = XmlManager.GetMapXmlBySprites(spritesToSend, CurrentMap);
            
            return Encoding.UTF8.GetBytes(xml);
        }

        static Wall CreateWall()
        {
            return new Wall();
        }
        static Player CreatePlayer()
        {
            Animation walking = TextureManager.GetAnimation("walk");
            Animation standing = TextureManager.GetAnimation("idle");
            Animation running = TextureManager.GetAnimation("run");
            Animation jump = TextureManager.GetAnimation("jump");
            Animation falling = TextureManager.GetAnimation("falling");
            jump.IsLooping = false;

            var player = new Player("???", TextureManager.NormalizeHeights(walking, standing, running, jump, falling))
            {
                Position = new Vector2(0, 0),
                IsEnable = false,
            };
            return player;
        }

        static Sprite CreateSprite()
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
