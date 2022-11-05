using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class DebugManager
    {
        public List<LogElement> EventLogList { get; private set; }
        public Color TextColor { get; set; }
        public Vector2 MaxLogVec { get; set; } //???
        float scale = 1f;
        float timer = 0f; 
        StringBuilder debugMessage;
        MousePointer debugMouse;
        GraphicsDeviceManager graphics;
        private Sprite[] sprites;
        public bool IsDebuging { get; private set; } 

        private NetworkManager networkManager; 
        private KeyboardManager keyboardManager;
        private FontManager fontManager;
        private TimeManager timeManager;
        private GameManager gameManager;
        public Texture2D debugTexture; 

        private static DebugManager instance;
        public static event EventHandler Initialized;
        public static DebugManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new DebugManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance; 
                }
                return instance;
            }
        } 

        private DebugManager()
        {
            debugMessage = new StringBuilder(); 
            IsDebuging = false; 
            TextColor = Color.White;
            EventLogList = new List<LogElement>();
        }

        public void InitDependencies(GraphicsDeviceManager graphics)
        {
            keyboardManager = KeyboardManager.Instance;
            fontManager = FontManager.Instance;
            timeManager = TimeManager.Instance;
            gameManager = GameManager.Instance;
            networkManager = NetworkManager.Instance;
            debugTexture = TextureManager.Instance.CreateTexture(11,11, paint => (paint % 2 == 0) ? Color.Red : Color.Transparent); 

            keyboardManager.KeyPressedSubTo(Keys.F3, ToggleDebugMode);
            keyboardManager.KeyRealeased += AddEvent;
            keyboardManager.KeyPressed += AddEvent;
            this.graphics = graphics;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsDebuging)
                return;

            sprites = gameManager.GetSprites(); 
            if (timer > 16)
            {
                MaxLogVec = Vector2.Zero;
                LogElement[] logList;
                lock (EventLogList)
                    logList = EventLogList.ToArray(); 
                for (int i = 0; i < logList.Length; i++)
                {
                    logList[i].Update();
                    if (logList[i].IsRemove)
                    {
                        lock(EventLogList)
                            EventLogList.Remove(logList[i]);
                        continue;
                    }
                    Vector2 temp = fontManager.GetFont("Default").MeasureString(logList[i].Text);
                    if (temp.X > MaxLogVec.X)
                    {
                        MaxLogVec = new Vector2(temp.X, 0);
                    }
                }
                BuildDebugMessage(sprites, gameTime);
                timer = 0f;
            }
            else
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; 
        }

        public void ToggleDebugMode()
        {
            sprites = gameManager.GetSprites(); 
            IsDebuging = !IsDebuging; 
            if(IsDebuging)
            {
                lock(EventLogList)
                    EventLogList.Clear();
                SortSpriteToDebug();
            }
        }

        public void AddEvent(object e)
        {
            lock (EventLogList)
            {
                if (EventLogList.Count > 15)
                {
                    EventLogList.RemoveAt(0);
                }
                switch (e)
                {
                    case KeysArray k:
                        EventLogList.Add(new LogElement(k.ToString()));
                        break;
                    case string s:
                        EventLogList.Add(new LogElement(s.ToString()));
                        break;
                    default:
                        EventLogList.Add(new LogElement(e.ToString()));
                        break;
                }
            }
        }

        public void SortSpriteToDebug()
        {
            ClearDebugMember();
            debugMouse = sprites.FirstOrDefault(e => e is MousePointer) as MousePointer;
        }

        private void BuildDebugMessage(Sprite[] sprites, GameTime gameTime)
        {
            lock (debugMessage)
            {
                debugMessage.Clear();

                debugMessage.Append("Window dimension : " + graphics.PreferredBackBufferHeight + ", " + graphics.PreferredBackBufferWidth + "\n");
                debugMessage.Append("ID Main Thread = " + Thread.CurrentThread.ManagedThreadId + "\n");
                debugMessage.Append("Total Time : " + gameTime.TotalGameTime.TotalSeconds.ToString("#.#") + "s\n");
                debugMessage.Append("GameState : " + gameManager.GameState + " \n");
                debugMessage.Append("IsConnected : " + networkManager.IsConnectedToAServer + "\n");
                debugMessage.Append("Fps : " + (1000 / gameTime.ElapsedGameTime.TotalMilliseconds).ToString("#") + " \n");
                debugMessage.Append("Elapse update = " + timeManager.ElapsedBetweenUpdates.TotalMilliseconds.ToString("#.##") + "\n");
                debugMessage.Append("Sprite Count = " + sprites.Length.ToString() + "\n");

                Dictionary<Type, int> debugTypeList = new Dictionary<Type, int>();
                foreach (Sprite sprite in sprites)
                {
                    Type t;
                    if (Monitor.TryEnter(sprite))
                    {
                        t = sprite.GetType();
                        Monitor.Exit(sprite);
                    }
                    else
                        continue;

                    if (debugTypeList.ContainsKey(t))
                        debugTypeList[t]++;
                    else
                        debugTypeList.Add(t, 1);
                }

                for (int i = 0; i < debugTypeList.Count; i++)
                {
                    debugMessage.Append("  - " + debugTypeList.ElementAt(i).Key.Name.ToString() + " : " + debugTypeList.ElementAt(i).Value.ToString() + "\n");
                }

                if (debugMouse != null)
                    debugMessage.Append(debugMouse.ToString());
                else
                    debugMessage.Append("No MouseCursor Detected :(");
            }
        }
        private void ClearDebugMember()
        {
            debugMouse = null; 
        }
        public void DebugDraw(SpriteBatch spriteBatch)
        {
            LogElement[] logList;
            lock (EventLogList)
                logList = EventLogList.ToArray(); 
            for(int i = 0; i < logList.Length; i++)
            {
                spriteBatch.DrawString(fontManager.GetFont("Default"), logList[i].Text, new Vector2(graphics.PreferredBackBufferWidth,  i * scale * 20) , Color.White * logList[i].opacity, 0f, MaxLogVec, scale, SpriteEffects.None, 1f); 
            }
            lock(debugMessage)
                spriteBatch.DrawString(fontManager.GetFont("Default"), debugMessage, Vector2.Zero, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }
    }
}
