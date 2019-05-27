using ExplorerOpenGL.Controlers;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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

            controler = new Controler(fonts, graphics, Content);

            _sprites = new List<Sprite>(); 

            var loading = new Thread(() =>
            {
                List<Sprite> sprites = new List<Sprite>()
                {
                new SideMenu(controler.TextureManager.CreateBorderedTexture(400, 730,5,0, e => new Color(167, 216, 134), e => new Color(131, 186, 94)), fonts["Default"])
                {
                    Position = new Vector2(0,0),
                    name = "Test",
                    layerDepth = .5f,
                },
                
                new MousePointer(),
                };
                _sprites = sprites;
            });
            loading.Start();

            Window.ClientSizeChanged += controler.UpdateDisplay;
            Window.AllowUserResizing = true;

            controler.KeyboardUtils.KeyPressed += OnKeyPressed;

        }

        private void OnKeyPressed(Keys[] keys)
        {
            if(controler.KeyboardUtils.IsContaining(keys, Keys.F3))
            {
                controler.DebugManager.ToggleDebugMode(_sprites); 
            }
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
