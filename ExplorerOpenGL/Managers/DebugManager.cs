using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers
{
    public class DebugManager
    {
        public static List<LogElement> EventLogList { get; private set; } = new List<LogElement>();
        public static Color TextColor { get; set; } = Color.White;
        public static Vector2 MaxLogVec { get; set; } //???
        static float scale = 1f;
        static float timer = 0f; 

        static float fpsTimer = 0f;
        static int countfps = 0;
        static float fps = 0f;
        static StringBuilder debugMessage = new StringBuilder(); 
        static MousePointer debugMouse;
        static GraphicsDeviceManager Graphics;
        static Sprite[] sprites;

        public static Texture2D debugTexture;
        public static bool IsDebuging { get; private set; } = false;

        public static void InitDependencies(GraphicsDeviceManager graphics)
        {
            debugTexture = TextureManager.CreateTexture(11,11, paint => (paint % 2 == 0) ? Color.Red : Color.Transparent);
            Graphics = graphics; 
            KeyboardManager.KeyPressedSubTo(Keys.F3, ToggleDebugMode);
            KeyboardManager.KeyRealeased += AddEvent;
            KeyboardManager.KeyPressed += AddEvent;
        }

        public static void Update(GameTime gameTime)
        {
            if (!IsDebuging)
                return;

            fpsTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (fpsTimer > 1000)
            {
                fps = countfps;
                fpsTimer = 0;
                countfps = 0;
            }
            countfps++; 
            sprites = GameManager.GetSprites(); 
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
                    Vector2 temp = FontManager.GetFont("Default").MeasureString(logList[i].Text);
                    if (temp.X > MaxLogVec.X)
                    {
                        MaxLogVec = new Vector2(temp.X, 0);
                    }
                }
                BuildDebugMessage(sprites, gameTime); ;
                timer = 0f;
            }
            else
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; 
        }

        public static void ToggleDebugMode()
        {
            sprites = GameManager.GetSprites(); 
            IsDebuging = !IsDebuging; 
            if(IsDebuging)
            {
                lock(EventLogList)
                    EventLogList.Clear();
                SortSpriteToDebug();
            }
        }

        public static void AddEventToTerminal(object e)
        {
            GameManager.Terminal.AddMessageToTerminal(e.ToString());
        }
        public static void AddEvent(object e)
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

        public static void SortSpriteToDebug()
        {
            ClearDebugMember();
            debugMouse = sprites.FirstOrDefault(e => e is MousePointer) as MousePointer;
        }

        private static void BuildDebugMessage(Sprite[] sprites, GameTime gameTime)
        {
             
            debugMessage.Clear();

            debugMessage.Append("Window dimension : " + Graphics.PreferredBackBufferHeight + ", " + Graphics.PreferredBackBufferWidth + "\n");
            debugMessage.Append("ID Main Thread = " + Thread.CurrentThread.ManagedThreadId + "\n");
            debugMessage.Append("Total Time : " + gameTime.TotalGameTime.TotalSeconds.ToString("#.#") + "s\n");
            debugMessage.Append("GameState : " + GameManager.GameState + " \n");
            debugMessage.Append("IsConnected : " + NetworkManager.IsConnectedToAServer + "\n");
            debugMessage.Append("Fps : " + fps.ToString("#") + " \n");
            debugMessage.Append("Elapse update = " + gameTime.ElapsedGameTime.TotalMilliseconds.ToString("#.##") + "\n");
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
                debugMessage.Append(debugMouse.ToString() +" \n");
            else
                debugMessage.Append("No MouseCursor Detected :(\n");

            debugMessage.Append($"{GameManager.Player}\n");
        }
        private static void ClearDebugMember()
        {
            debugMouse = null; 
        }
        public static void DebugDraw(SpriteBatch spriteBatch)
        {
            LogElement[] logList;
            lock (EventLogList)
                logList = EventLogList.ToArray(); 
            for(int i = 0; i < logList.Length; i++)
            {
                spriteBatch.DrawString(FontManager.GetFont("Default"), logList[i].Text, new Vector2(Graphics.PreferredBackBufferWidth,  i * scale * 20) , Color.Black * logList[i].opacity, 0f, MaxLogVec, scale, SpriteEffects.None, 1f); 
            }
            lock (debugMessage)
                spriteBatch.DrawString(FontManager.GetFont("Default"), debugMessage, Vector2.Zero, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }
    }
}
