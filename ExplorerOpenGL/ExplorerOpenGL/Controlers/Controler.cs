using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers
{
    public class Controler
    {
        public Vector2 scales { get; set; }
        public KeyboardUtils KeyboardUtils;
        public DebugManager DebugManager; //instantiate on load
        public TextureManager TextureManager; //instantiate on load 

        List<Sprite> _sprites;
        Dictionary<string, SpriteFont> fonts;
        GraphicsDeviceManager graphics;

        public Controler(Dictionary<string, SpriteFont> Fonts, GraphicsDeviceManager Graphics, ContentManager content)
        {
            
            KeyboardUtils = new KeyboardUtils();
            TextureManager = new TextureManager(Graphics, content);
            DebugManager = new DebugManager(TextureManager, Fonts, Graphics);

            KeyboardUtils.KeyPressed += DebugManager.AddEvent;
            KeyboardUtils.KeyRealeased += DebugManager.AddEvent;
            
            fonts = Fonts;
            graphics = Graphics; 
        }

        public void Update(List<Sprite> sprites)
        {
            if(KeyboardUtils != null && TextureManager != null && DebugManager != null)
            {
                KeyboardUtils.Update();
                DebugManager.Update(sprites);
            }
            else
            {
                throw new NullReferenceException("Toutes les instances des controllers doivent être initialisées"); 
            }

        }
        public void UpdateDisplay(object sender, EventArgs e)
        {
            GameWindow window = sender as GameWindow;
            Vector2 Bounds = new Vector2(window.ClientBounds.Width, window.ClientBounds.Height); 
            if(Bounds.X > Bounds.Y)
            {
                window.Se
            }
            else
            {

            }

            graphics.PreferredBackBufferHeight = window.ClientBounds.Height;
            graphics.PreferredBackBufferWidth = window.ClientBounds.Width;
            graphics.ApplyChanges();

        }
    }
}
