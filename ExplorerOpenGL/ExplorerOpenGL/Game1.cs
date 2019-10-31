using ExplorerOpenGL.Controlers;
using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ExplorerOpenGL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        Controler controler; 
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<Sprite> _sprites;

        const int Height = 730;
        const int Width = 1360; 

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Window.AllowUserResizing = true; 
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = Height;
            graphics.PreferredBackBufferWidth = Width;
            IsMouseVisible = true; 
            graphics.IsFullScreen = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var fonts = new Dictionary<string, SpriteFont>()
            {
                {"Default", Content.Load<SpriteFont>("Fonts/Default") },
            };

            Texture2D player = Content.Load<Texture2D>("player");
            Texture2D playerfeet = Content.Load<Texture2D>("playerfeet");

            _sprites = new List<Sprite>();

            controler = new Controler(fonts, _sprites, graphics, Content, spriteBatch);

            _sprites.Add(new MousePointer());


            _sprites.Add(new Player(player, playerfeet, (MousePointer)_sprites[0])
            {
                Position = new Vector2(200, 200),
                input = new Input()
                {
                    Down = Keys.S,
                    Up = Keys.Z,
                    Left = Keys.Q,
                    Right = Keys.D,
                }
            });


            Window.ClientSizeChanged += controler.UpdateDisplay;
            Window.AllowUserResizing = true;

            controler.KeyboardUtils.KeyPressed += OnKeyPressed;
            controler.KeyboardUtils.KeyRealeased += OnKeyRealeased;
        }

        private void OnKeyRealeased(Keys[] keys)
        {
            controler.DebugManager.AddEvent(new KeysArray(keys));

        }

        private void OnKeyPressed(Keys[] keys)
        {
            if(controler.KeyboardUtils.IsContaining(keys, Keys.F3))
            {
                controler.DebugManager.ToggleDebugMode(_sprites);
            }
            if(controler.KeyboardUtils.IsContaining(keys, Keys.F2))
            {
                Texture2D screenshot = controler.RenderManager.RenderSceneToTexture();

                Stream stream = File.Create(@"C:\Users\Nicolas Descotes\Desktop\image.png");
                screenshot.SaveAsPng(stream, Width, Height);
                stream.Dispose(); 
            }

            controler.DebugManager.AddEvent(new KeysArray(keys)); 
        }



        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if(_sprites == null)
                return;

            for (int i = 0; i < _sprites.Count; i++)
            {
                if (_sprites[i].IsRemove)
                {
                    _sprites.RemoveAt(i);
                    i--; 
                }
                _sprites[i].Update(gameTime, _sprites, controler);
            }
            // TODO: Add your update logic here
            controler.Update(_sprites); 

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (_sprites == null)
                return; 
            spriteBatch.Begin(SpriteSortMode.BackToFront);


            foreach(var sprite in _sprites)
            {
                sprite.Draw(spriteBatch);
            }

            if(controler.DebugManager.IsDebuging)
                controler.DebugManager.DebugDraw(spriteBatch); 

            spriteBatch.End(); 
            base.Draw(gameTime);
        }
    }
}
