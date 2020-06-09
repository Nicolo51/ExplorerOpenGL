using ExplorerOpenGL.Controlers;
using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
        Player player; 

        const int Height = 730;
        const int Width = 1360; 

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Window.AllowUserResizing = true; 
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

            var fonts = new Dictionary<string, SpriteFont>()
            {
                {"Default", Content.Load<SpriteFont>("Fonts/Default") },
            };

            Texture2D player = Content.Load<Texture2D>("player");
            Texture2D playerfeet = Content.Load<Texture2D>("playerfeet");
            Texture2D Sight = Content.Load<Texture2D>("sight");

            _sprites = new List<Sprite>();

            controler = new Controler(fonts, _sprites, graphics, Content, spriteBatch);

            _sprites.Add(new MousePointer(Sight, controler.camera));
            new Thread(() =>
            {
                _sprites.Add(new Wall(controler.TextureManager.LoadNoneContentLoadedTexture(@"D:\Mes documents\Images\Wlop\2018 September 1\2_Invitation_4k.jpg"))
                {
                    Position = new Vector2(100, 100),
                });
            }).Start(); 
            Player Player = new Player(player, playerfeet, (MousePointer)_sprites[0])
            {
                Position = new Vector2(200, 200),
                input = new Input()
                {
                    Down = Keys.S,
                    Up = Keys.Z,
                    Left = Keys.Q,
                    Right = Keys.D,
                }
            };
            this.player = Player; 

            _sprites.Add(Player);
            controler.camera.FollowSprite(Player);
            controler.camera.LookAt(0, 0); 

            Window.ClientSizeChanged += UpdateDisplay;
            Window.AllowUserResizing = true;

            controler.KeyboardUtils.KeyPressed += OnKeyPressed;
            controler.KeyboardUtils.KeyRealeased += OnKeyRealeased;
        }
        public void UpdateDisplay(object sender, EventArgs e)
        {
            GameWindow window = sender as GameWindow;
            Vector2 Bounds = new Vector2(window.ClientBounds.Width, window.ClientBounds.Height);
        }

        private void OnKeyRealeased(Keys[] keys)
        {
            controler.DebugManager.AddEvent("Key realeased : " + new KeysArray(keys));
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
            if(controler.KeyboardUtils.IsContaining(keys, Keys.F5))
            {
                controler.camera.ToggleFollow(); 
            }
            if (controler.KeyboardUtils.IsContaining(keys, Keys.F1))
            {
                controler.CommunicationClient.Start(player);
            }
            controler.DebugManager.AddEvent("Key pressed : " + new KeysArray(keys));
            
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
            controler.camera.Update(); 

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
            spriteBatch.Begin(SpriteSortMode.BackToFront, transformMatrix: controler.camera.Transform) ;


            for (int i = 1; i < _sprites.Count; i ++)
            {
                _sprites[i].Draw(spriteBatch);
            }

            spriteBatch.End();

            spriteBatch.Begin();
            _sprites[0].Draw(spriteBatch);
            if (controler.DebugManager.IsDebuging)
                controler.DebugManager.DebugDraw(spriteBatch);

            spriteBatch.End(); 

            base.Draw(gameTime);

            /*
             * // Somewhere accessible
const int TargetWidth = 480;
const int TargetHeight = 270;
Matrix Scale;

// Somewhere in initialisation
float scaleX = device.PreferredBackBufferWidth / TargetWidth;
float scaleY = device.PreferredBackBufferHeight / TargetHeight;
Scale = Matrix.CreateScale(new Vector3(scaleX, scaleY, 1));

// Somewhere with drawing
protected override void Draw(GameTime gameTime)
{
    SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Scale);
    Root.Draw(SpriteBatch, gameTime);
    SpriteBatch.End();

    base.Draw(gameTime);
}*/
        }
    }
}
