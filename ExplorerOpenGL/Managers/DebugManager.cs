using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class DebugManager
    {

        public List<LogElement> EventLogList { get; private set; }
        public Color TextColor { get; set; }
        public Vector2 MaxLogVec { get; set; } //???
        float scale = 2f;
        StringBuilder debugMessage;
        MousePointer debugMouse;
        GraphicsDeviceManager graphics;
        private List<Sprite> sprites;
        public bool IsDebuging { get; private set; } 
        private static DebugManager instance;


        public static event EventHandler Initialized;
        private KeyboardManager keyboardManager;
        private FontManager fontManager;
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

        public void InitDependencies(GraphicsDeviceManager graphics, List<Sprite> sprites)
        {
            keyboardManager = KeyboardManager.Instance;
            fontManager = FontManager.Instance;

            keyboardManager.KeyPressedSubTo(Keys.F3, ToggleDebugMode);
            keyboardManager.KeyRealeased += AddEvent;
            keyboardManager.KeyPressed += AddEvent;
            this.sprites = sprites;
            this.graphics = graphics;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsDebuging)
                return;

            MaxLogVec = Vector2.Zero; 
            for(int i = 0; i < EventLogList.Count; i++)
            {
                EventLogList[i].Update(); 
                if(EventLogList[i].IsRemove)
                {
                    EventLogList.RemoveAt(i);
                    continue; 
                }
                Vector2 temp = fontManager.GetFont("Default").MeasureString(EventLogList[i].Text);
                if (temp.X > MaxLogVec.X)
                {
                    MaxLogVec = new Vector2(temp.X, 0); 
                }
            }
            BuildDebugMessage(sprites, gameTime);
        }

        public void ToggleDebugMode()
        {
            IsDebuging = !IsDebuging; 
            if(IsDebuging)
            {
                EventLogList.Clear();
                SortSpriteToDebug(sprites);
            }
        }

        public void AddEvent(object e)
        {
            if (EventLogList.Count > 10)
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

        public void SortSpriteToDebug(List<Sprite> _sprites)
        {
            ClearDebugMember();
            foreach(var sprite in _sprites)
            {
                if(sprite is MousePointer pointer)
                {
                    debugMouse = pointer; 
                }
            }
        }

        private void BuildDebugMessage(List<Sprite> sprites, GameTime gameTime)
        {
            debugMessage.Clear();

            debugMessage.Append("Window dimension : " + graphics.PreferredBackBufferHeight + ", " + graphics.PreferredBackBufferWidth +"\n");
            debugMessage.Append("Total Time : " + gameTime.TotalGameTime.TotalSeconds.ToString("#.#") + "s\n");

            debugMessage.Append("Sprite Count = " + sprites.Count.ToString() + "\n");
            Dictionary<Type, int> debugTypeList = new Dictionary<Type, int>();
            foreach(Sprite sprite in sprites)
            {
                Type t = sprite.GetType();
                if (debugTypeList.ContainsKey(t))
                    debugTypeList[t]++;
                else
                    debugTypeList.Add(t, 1); 
            }

            for(int i = 0;  i < debugTypeList.Count; i++)
            {
                debugMessage.Append("  - " + debugTypeList.ElementAt(i).Key.Name.ToString() + " : " + debugTypeList.ElementAt(i).Value.ToString() + "\n");
            }

           
            if (debugMouse != null)
                debugMessage.Append(debugMouse.ToString());
            else
                debugMessage.Append("No MouseCursor Detected :("); 
        }

        private void ClearDebugMember()
        {
            debugMouse = null; 
        }

        public void DebugDraw(SpriteBatch spriteBatch)
        {
            for(int i = 0; i < EventLogList.Count; i++)
            {
                spriteBatch.DrawString(fontManager.GetFont("Default"), EventLogList[i].Text, new Vector2(graphics.PreferredBackBufferWidth,  i * scale * 20) , Color.White * EventLogList[i].opacity, 0f, MaxLogVec, scale, SpriteEffects.None, 1f); 
            }
            spriteBatch.DrawString(fontManager.GetFont("Default"), debugMessage, Vector2.Zero, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }
    }
}
