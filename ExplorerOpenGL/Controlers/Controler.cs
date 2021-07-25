using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers
{
    public class Controler
    {
        public KeyboardUtils KeyboardUtils;
        public DebugManager DebugManager; //instantiate on load
        public TextureManager TextureManager; //instantiate on load 
        public RenderManager RenderManager;
        public NetwokManager NetwokManager;
        public Vector2 Bounds; 
        public Player Player { get; set; }
        public Chat Chat { get; private set; }
        public Camera Camera { get; private set; } 
        public MousePointer MousePointer { get; private set; }

        List<Sprite> _sprites;
        Dictionary<string, SpriteFont> fonts;
        GraphicsDeviceManager graphics;

        public Controler(Dictionary<string, SpriteFont> Fonts, List<Sprite> sprites, GraphicsDeviceManager Graphics, ContentManager content, SpriteBatch spriteBatch, Vector2 Bounds)
        {
            KeyboardUtils = new KeyboardUtils();
            RenderManager = new RenderManager(sprites, Graphics, spriteBatch);
            TextureManager = new TextureManager(Graphics, content, spriteBatch, RenderManager);
            DebugManager = new DebugManager(TextureManager, Fonts, Graphics);
            NetwokManager = new NetwokManager(this); 

            KeyboardUtils.KeyPressed += DebugManager.AddEvent;
            KeyboardUtils.KeyRealeased += DebugManager.AddEvent;

            this.Bounds = Bounds; 
            _sprites = sprites; 
            fonts = Fonts;
            graphics = Graphics;

            Camera = new Camera(new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            Chat = new Chat(TextureManager.CreateTexture(700, 500, paint => Color.Black), fonts["Default"], new TextinputBox(TextureManager.CreateTexture(700, 100, paint => Color.Red), fonts["Default"]));

            KeyboardUtils.KeyPressed += Chat.KeyboardListener;
            Chat.TextinputBox.OnValidation += OnQueried; 

            MousePointer = new MousePointer(TextureManager.LoadTexture("sight")); 
            _sprites.Add(Chat);
            _sprites.Add(MousePointer);
            InitKeyEvent(); 
        }

        private void OnQueried(string message)
        {
            if (NetwokManager.IsConnectedToAServer && (message = message.Trim()) != String.Empty)
            {
                NetwokManager.SendMessageToServer(message);
            }
        }

        public void Update(List<Sprite> sprites, GameTime gametime)
        {
            if(KeyboardUtils != null && TextureManager != null && DebugManager != null && NetwokManager != null )
            {
                KeyboardUtils.Update();
                DebugManager.Update(sprites);
                if (NetwokManager.IsConnectedToAServer)
                {
                    NetwokManager.Update(gametime, Player);
                }
            }
            else
            {
                throw new NullReferenceException("Toutes les instances des controllers doivent être initialisées"); 
            }

        }

        private void InitKeyEvent()
        {
            KeyboardUtils.KeyPressed += OnKeyPressed;
            KeyboardUtils.KeyRealeased += OnKeyRealeased;
        }

        private void OnKeyRealeased(Keys[] keys, KeyboardUtils keyboardUtils)
        {
            DebugManager.AddEvent("Key realeased : " + new KeysArray(keys), keyboardUtils);
        }

        public void AddSprite(Sprite sprite)
        {
            _sprites.Add(sprite); 
        }

        private void OnKeyPressed(Keys[] keys, KeyboardUtils keyboardUtils)
        {
            if (KeyboardUtils.Contains(keys, Keys.OemQuestion))
            {
                Chat.AddMessageToChat("This is a ne message", "Client", Color.Green); 
            }
            if (KeyboardUtils.Contains(keys, Keys.F3))
            {
                DebugManager.ToggleDebugMode(_sprites);
            }
            if (KeyboardUtils.Contains(keys, Keys.F2))
            {
                Texture2D screenshot = RenderManager.RenderSceneToTexture();

                Stream stream = File.Create(@"C:\Users\nicol\Desktop\image.png");
                screenshot.SaveAsPng(stream, (int)Bounds.X, (int)Bounds.Y);
                stream.Dispose();
            }
            if (KeyboardUtils.Contains(keys, Keys.F5))
            {
                Camera.ToggleFollow();
            }
            if (KeyboardUtils.Contains(keys, Keys.F1))
            {
                using (StreamReader sr = new StreamReader("ip.txt"))
                {
                    string ip = sr.ReadLine();
                    NetwokManager.Connect(ip);
                }
            }
            DebugManager.AddEvent("Key pressed : " + new KeysArray(keys), keyboardUtils);
        }


    }
}
