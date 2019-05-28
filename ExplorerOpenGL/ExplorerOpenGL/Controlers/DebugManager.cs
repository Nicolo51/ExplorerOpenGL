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

namespace ExplorerOpenGL.Controlers
{
    public class DebugManager
    {
        public Dictionary<string, SpriteFont> Fonts { get; set; }
        public Dictionary<string, Texture2D> Textures { get; set; }

        TextureManager textureManager; 

        public List<string> EventLogList { get; private set; }

        public Texture2D backgroundTexture { get; private set; }

        public Color TextColor { get; set; }

        float scale = 1f;

        Texture2D OutlineDebugMessageTexture; 

        //To Debug
        StringBuilder debugMessage;
        MousePointer debugMouse;
        GraphicsDeviceManager graphics; 

        public bool IsDebuging { get; private set; } 

        public DebugManager(TextureManager textureManager,  Dictionary<string, SpriteFont> fonts, GraphicsDeviceManager graphics)
        {
            this.textureManager = textureManager;
            this.backgroundTexture = backgroundTexture; 
            this.graphics = graphics; 
            Fonts = fonts; 
            debugMessage = new StringBuilder(); 
            IsDebuging = false; 
            TextColor = Color.White; 
        }

        public void Update(List<Sprite> sprites)
        {
            if (!IsDebuging)
                return;

            BuildDebugMessage(sprites);
            OutlineDebugMessageTexture = textureManager.OutlineText(debugMessage.ToString(), Fonts["Default"], Color.White, Color.Black, 10);
        }

        public void ToggleDebugMode(List<Sprite> sprites)
        {
            IsDebuging = !IsDebuging; 
            if(IsDebuging)
            { 
                SortSpriteToDebug(sprites);
                BuildDebugMessage(sprites); 
            }
        }

        public void AddEvent(object input)
        {
            if(input is KeysArray)
            {
                EventLogList.Add(input.ToString()); 
            }
        }

        public void SortSpriteToDebug(List<Sprite> _sprites)
        {
            ClearDebugMember();
            foreach(var sprite in _sprites)
            {
                if(sprite is MousePointer)
                {
                    debugMouse = (MousePointer)sprite; 
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
            Vector2 stringDimension = Fonts["Default"].MeasureString(debugMessage);

            //spriteBatch.Draw(textureManager.CreateTexture((int)stringDimension.X, (int)stringDimension.Y, paint => Color.Black), Vector2.Zero, null, Color.White * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            //spriteBatch.Draw(textureManager.OutlineText(debugMessage.ToString(), Fonts["Default"], spriteBatch, Color.White, Color.Black, 5), Vector2.Zero, null, Color.White * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            //spriteBatch.DrawString(Fonts["Default"], debugMessage, Vector2.Zero, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, .98f);
            //spriteBatch.DrawString(Fonts["Default"], debugMessage, new Vector2( 3,3), Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, .99f);
            //textureManager.OutlineText(debugMessage.ToString(), Fonts["Default"], spriteBatch, Color.White, Color.Black, 5);
            spriteBatch.Draw(OutlineDebugMessageTexture, Vector2.Zero, null, Color.White * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            //spriteBatch.DrawString(Fonts["Default"], debugMessage, Vector2.Zero, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}
