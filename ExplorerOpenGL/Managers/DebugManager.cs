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
        public Dictionary<string, SpriteFont> Fonts { get; set; }
        public Dictionary<string, Texture2D> Textures { get; set; }
        public List<LogElement> EventLogList { get; private set; }
        public Color TextColor { get; set; }
        public Vector2 MaxLogVec { get; set; } //???
        float scale = 1f;
        Texture2D OutlineDebugMessageTexture; 
        StringBuilder debugMessage;
        MousePointer debugMouse;
        GraphicsDeviceManager graphics;
        public bool IsDebuging { get; private set; } 
        private static DebugManager instance;


        public static event EventHandler Initialized;
        private TextureManager textureManager; 
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

        public void InitDependencies()
        {
            textureManager = TextureManager.Instance;
        }

        public void Update(List<Sprite> sprites)
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
                Vector2 temp = Fonts["Default"].MeasureString(EventLogList[i].Text);
                if (temp.X > MaxLogVec.X)
                {
                    MaxLogVec = new Vector2(temp.X, 0); 
                }
            }

            BuildDebugMessage(sprites);
            OutlineDebugMessageTexture = textureManager.OutlineText(debugMessage.ToString(), "Default", Color.Black, Color.White, 0);
        }

        public void ToggleDebugMode(List<Sprite> sprites)
        {
            IsDebuging = !IsDebuging; 
            if(IsDebuging)
            {
                EventLogList.Clear();
                SortSpriteToDebug(sprites);
                BuildDebugMessage(sprites); 
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
        public void AddEvent(object e, KeyboardManager KeyboardManager)
        {
            if (EventLogList.Count > 10)
            {
                EventLogList.RemoveAt(0);
            }
            if (e is KeysArray)
            {
                EventLogList.Add(new LogElement(e.ToString()));
            }
            if (e is string)
            {
                EventLogList.Add(new LogElement(e as string));
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

        private void BuildDebugMessage(List<Sprite> sprites)
        {
            debugMessage.Clear();

            debugMessage.Append("Window dimension : " + graphics.PreferredBackBufferHeight + ", " + graphics.PreferredBackBufferWidth +"\n");

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
                spriteBatch.DrawString(Fonts["Default"], EventLogList[i].Text, new Vector2(graphics.PreferredBackBufferWidth,  i * 20) , Color.White * EventLogList[i].opacity, 0f, MaxLogVec, 1f, SpriteEffects.None, 1f); 
            }
            spriteBatch.Draw(OutlineDebugMessageTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }
    }
}
