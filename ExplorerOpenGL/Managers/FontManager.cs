using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers
{
    public class FontManager
    {

        private static ContentManager Content;

        private static Dictionary<string, SpriteFont> loadedFonts = new Dictionary<string, SpriteFont>();

        public static void InitDependencies(ContentManager content)
        {
            Content = content;
            InitFonts(); 
        }

        public static SpriteFont GetFont(string font = "default")
        {
            string fontTL = font.ToLower().Trim();
            if (loadedFonts.ContainsKey(fontTL))
                return loadedFonts[fontTL];
            try
            {
                SpriteFont spriteFont = Content.Load<SpriteFont>("Fonts/" + font);
                loadedFonts.Add(fontTL, spriteFont);
                return spriteFont;
            }
            catch
            {
                return null; 
            }
        }

        public static void InitFonts()
        {
            loadedFonts.Add("default", Content.Load<SpriteFont>("Fonts/Default"));
            loadedFonts.Add("menu", Content.Load<SpriteFont>("Fonts/Menu"));
        }
    }
}
