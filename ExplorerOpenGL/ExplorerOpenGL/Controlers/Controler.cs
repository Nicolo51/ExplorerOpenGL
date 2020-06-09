using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers
{
    public class Controler
    {
        public Vector2 scales { get; set; }
        public KeyboardUtils KeyboardUtils;
        public DebugManager DebugManager; //instantiate on load
        public TextureManager TextureManager; //instantiate on load 
        public RenderManager RenderManager;
        public CommunicationClient CommunicationClient;
        public Player player { get; private set; }
        public int IDClient { get; private set; }
        public Camera camera { get; private set; } 

        List<Sprite> _sprites;
        Dictionary<string, SpriteFont> fonts;
        GraphicsDeviceManager graphics;

        public Controler(Dictionary<string, SpriteFont> Fonts, List<Sprite> sprites, GraphicsDeviceManager Graphics, ContentManager content, SpriteBatch spriteBatch)
        {
            KeyboardUtils = new KeyboardUtils();
            RenderManager = new RenderManager(sprites, Graphics, spriteBatch);
            TextureManager = new TextureManager(Graphics, content, spriteBatch, RenderManager);
            CommunicationClient = new CommunicationClient(); 
            DebugManager = new DebugManager(TextureManager, Fonts, Graphics);
            KeyboardUtils.KeyPressed += DebugManager.AddEvent;
            KeyboardUtils.KeyRealeased += DebugManager.AddEvent;

            _sprites = sprites; 
            fonts = Fonts;
            graphics = Graphics;

            camera = new Camera(new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
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

    }
}
