using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controllers
{
    public class DebugManager
    {
        public Dictionary<string, SpriteFont> Fonts { get; set; }
        public Dictionary<string, Texture2D> Textures { get; set; }
        private List<Sprite> _sprites;

        public Color TextColor { get; set; }

        //To Debug
        StringBuilder debugMessage; 
        MousePointer debugMouse; 

        public bool IsDebuging { get; private set; } 

        public DebugManager(List<Sprite> sprites, Dictionary<string, SpriteFont> fonts)
        {
            Fonts = fonts; 
            debugMessage = new StringBuilder(); 
            IsDebuging = false; 
            TextColor = Color.White; 
            _sprites = sprites; 
        }

        public void Update()
        {
            if (!IsDebuging)
                return;

            BuildDebugMessage(); 
        }

        public void ToggleDebugMode()
        {
            IsDebuging = !IsDebuging; 
            if(IsDebuging)
            { 
                SortSpriteToDebug();
                BuildDebugMessage(); 
            }
        }

        public void SortSpriteToDebug()
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
