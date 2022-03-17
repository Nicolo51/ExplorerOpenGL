using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class FontManager
    {
        public static event EventHandler Initialized;

        private ContentManager content;

        private Dictionary<string, SpriteFont> loadedFonts; 

        private static FontManager instance; 
        public static FontManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new FontManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance; 
                }
                return instance;
            }
        }
        private FontManager()
        {
            loadedFonts = new Dictionary<string, SpriteFont>(); 
        }


        public void InitDependencies(ContentManager content)
        {
            this.content = content;
            InitFonts(); 
        }

        public SpriteFont GetFont(string font)
        {
            string fontTL = font.ToLower().Trim();
            if (loadedFonts.ContainsKey(fontTL))
                return loadedFonts[fontTL];
            try
            {
                SpriteFont spriteFont = content.Load<SpriteFont>("Fonts/" + font);
                loadedFonts.Add(fontTL, spriteFont);
                return spriteFont;
            }
            catch
            {
                return null; 
            }
        }

        public void InitFonts()
        {
            loadedFonts.Add("default", content.Load<SpriteFont>("Fonts/Default"));
            loadedFonts.Add("menu", content.Load<SpriteFont>("Fonts/Menu"));
        }
    }
}
