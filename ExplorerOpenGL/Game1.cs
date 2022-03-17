using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ExplorerOpenGL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GameManager gameManager;
        KeyboardManager keyboardManager;
        DebugManager debugManager;
        TextureManager textureManager; 
        RenderManager renderManager;
        NetworkManager networkManager;
        FontManager fontManager;
        ScripterManager scripterManager;
        TimeManager timeManager; 

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<Sprite> _sprites;
        Camera camera; 

        const int Height = 730;
        const int Width = 1360; 

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true; 
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = Height;
            graphics.PreferredBackBufferWidth = Width;
            IsMouseVisible = false; 
            graphics.IsFullScreen = false;
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _sprites = new List<Sprite>();
            camera = new Camera(new Vector2(Width, Height)); 
            DebugManager.Initialized += OnManagerInitialization;
            KeyboardManager.Initialized += OnManagerInitialization;
            NetworkManager.Initialized += OnManagerInitialization;
            RenderManager.Initialized += OnManagerInitialization;
            ScripterManager.Initialized += OnManagerInitialization;
            TextureManager.Initialized += OnManagerInitialization;
            GameManager.Initialized += OnManagerInitialization;
            FontManager.Initialized += OnManagerInitialization;
            TimeManager.Initialized += OnManagerInitialization;
            Exiting += Game1_Exiting;

            gameManager = GameManager.Instance;
            keyboardManager = KeyboardManager.Instance;
            textureManager = TextureManager.Instance;
            debugManager = DebugManager.Instance;
            fontManager = FontManager.Instance;
            networkManager = NetworkManager.Instance;
            renderManager = RenderManager.Instance;
            scripterManager = ScripterManager.Instance;
            timeManager = TimeManager.Instance; 

            timeManager.StartUpdateThread();

            base.Initialize();
        }

        private void Game1_Exiting(object sender, EventArgs e)
        {
            timeManager.StopUpdateThread();
        }

        protected override void LoadContent()
        {
            

            //Player Player = new Player(player, playerfeet, Manager.MousePointer, "Nicolas", Manager.TextureManager)
            //{
            //    Position = new Vector2(0, 0),
            //    input = new Input()
            //    {
            //        Down = Keys.S,
            //        Up = Keys.Z,
            //        Left = Keys.Q,
            //        Right = Keys.D,
            //    }
            //};
            //this.player = Player;
            //Manager.Player = this.player;
            //_sprites.Add(Player);
            //_sprites.Add(new Wall(Manager.TextureManager.CreateTexture(1000, 50, paint => (paint % 2 == 0)? Color.White : Color.Black)));
            //_sprites.Add(new Button(Manager.TextureManager.CreateTexture(200, 200, paint => Color.Black), Manager.TextureManager.CreateTexture(200, 200, paint => Color.Red), fonts["Default"])); 
            //Manager.Camera.FollowSprite(Player);
            gameManager.Camera.LookAt(0, 0);
            
            Window.ClientSizeChanged += UpdateDisplay;
            Window.AllowUserResizing = true;

            new Thread(() =>
            {
                new MainMenu().Show();
            }).Start();
        }

       

        public void UpdateDisplay(object sender, EventArgs e)
        {
            GameWindow window = sender as GameWindow;
            Vector2 Bounds = new Vector2(window.ClientBounds.Width, window.ClientBounds.Height);
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
            timeManager.LastDrawUpdateTime = DateTime.Now;
                if(_sprites == null)
                    return;
            //gameManager.MousePointer.Update(_sprites);
            gameManager.Camera.Update();
            textureManager.Update(); 
            gameManager.Update(gameTime);
            debugManager.Update(gameTime);
            keyboardManager.Update();
            networkManager.Update(gameTime);

            base.Update(gameTime);
        }

        private void OnManagerInitialization(object sender, EventArgs e)
        {
            switch (sender)
            {
                case TextureManager tm:
                    tm.InitDependencies(graphics, Content, spriteBatch); 
                    break;
                case DebugManager dm:
                    dm.InitDependencies(graphics, _sprites);
                    break;
                case KeyboardManager km:
                    Window.TextInput += KeyboardManager.Instance.OnTextInput;
                    km.InitDependencies(); 
                    break;
                case GameManager m:
                    m.InitDependencies(_sprites, camera); 
                    break;
                case NetworkManager nm:
                    nm.InitDependencies(); 
                    break;
                case ScripterManager sm:
                    sm.InitDependencies();
                    break;
                case RenderManager rm:
                    rm.InitDependencies(graphics, _sprites, spriteBatch);
                    break;
                case FontManager fm:
                    fm.InitDependencies(Content);
                    break;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            timeManager.LastDrawTime = DateTime.Now;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            float la = timeManager.LerpAmount;
            Sprite[] sprites = gameManager.GetSprites(); 
            if (sprites == null)
                return;
            spriteBatch.Begin(SpriteSortMode.BackToFront, transformMatrix: gameManager.Camera.Transform);

            for (int i = 0; i < sprites.Length; i++)
            {
                bool lockAcquired = false;
                try
                {
                    Monitor.TryEnter(sprites[i], 1, ref lockAcquired);
                    if (lockAcquired)
                    {
                        if (!sprites[i].IsHUD)
                            sprites[i].Draw(spriteBatch, la);
                    }
                    else
                        debugManager.AddEvent("Draw skipped" + i);
                }
                finally
                {
                    if (lockAcquired)
                        Monitor.Exit(sprites[i]); 
                }
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront);

            for (int i = 0; i < sprites.Length; i++)
            {
                bool lockAcquired = false;
                try
                {
                    Monitor.TryEnter(sprites[i], 1, ref lockAcquired);
                    if (lockAcquired)
                    {
                        if (sprites[i].IsHUD)
                            sprites[i].Draw(spriteBatch, la);
                    }
                    else
                        debugManager.AddEvent("Draw skipped" + i);
                }
                finally
                {
                    if (lockAcquired)
                        Monitor.Exit(sprites[i]);
                }
            }

            if (DebugManager.Instance.IsDebuging)
                DebugManager.Instance.DebugDraw(spriteBatch);
                
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
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
