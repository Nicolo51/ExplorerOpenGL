using ExplorerOpenGL.Controllers;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ExplorerOpenGL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<Sprite> _sprites;

        const int Height = 730;
        const int Width = 1360; 
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = Height;
            graphics.PreferredBackBufferWidth = Width;
            IsMouseVisible = false; 
            graphics.IsFullScreen = false;
        }

        protected override void Initialize()
        {

            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Controller.TextureManager = new TextureManager(graphics);
            var TextureButton = Controller.TextureManager.CreateTexture(101, 101, test);

            var fonts = new Dictionary<string, SpriteFont>()
            {
                {"Default", Content.Load<SpriteFont>("Fonts/Default") },
            };

            _sprites = new List<Sprite>()
            {
                new Button(TextureButton)
                {
                    Position = new Vector2(100,100),
                },
                new MousePointer()
            };

            Controller.DebugManager = new DebugManager(_sprites, fonts);

            Controller.KeyboardUtils.KeyPressed += OnKeyPressed; 

        }

        private void OnKeyPressed(Keys[] keys)
        {
            if(Controller.KeyboardUtils.IsContaining(keys, Keys.F3))
            {
                Controller.DebugManager.ToggleDebugMode(); 
            }
        }

        public Color test(int input)
        {
            if (input % 2 == 1)
            {
                return Color.Red;
            }
            else
                return Color.Black; 
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
            for (int i = 0; i < _sprites.Count; i++)
            {
                if (_sprites[i].IsRemove)
                {
                    _sprites.RemoveAt(i);
                    i--; 
                }
                _sprites[i].Update(gameTime, _sprites);
            }
            // TODO: Add your update logic here
            Controller.Update(); 

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.BackToFront);

            foreach(var sprite in _sprites)
            {
                sprite.Draw(spriteBatch);
            }

            if(Controller.DebugManager.IsDebuging)
                Controller.DebugManager.DebugDraw(spriteBatch); 

            spriteBatch.End(); 
            base.Draw(gameTime);
        }
    }
}
