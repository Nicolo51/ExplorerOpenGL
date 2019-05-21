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

        public List<string> EventLogList { get; private set; }

        public Color TextColor { get; set; }

        //To Debug
        StringBuilder debugMessage; 
        MousePointer debugMouse; 

        public bool IsDebuging { get; private set; } 

        public DebugManager(Dictionary<string, SpriteFont> fonts)
        {
            Fonts = fonts; 
            debugMessage = new StringBuilder(); 
            IsDebuging = false; 
            TextColor = Color.White; 
        }

        public void Update(List<Sprite> sprites)
        {
            if (!IsDebuging)
                return;

            BuildDebugMessage(); 
        }

        public void ToggleDebugMode(List<Sprite> sprites)
        {
            IsDebuging = !IsDebuging; 
            if(IsDebuging)
            { 
                SortSpriteToDebug(sprites);
                BuildDebugMessage(); 
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

        private void BuildDebugMessage()
        {
            debugMessage.Clear();
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
            spriteBatch.DrawString(Fonts["Default"], debugMessage, Vector2.Zero , TextColor); 
        }

    }
}
