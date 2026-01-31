using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using ExplorerOpenGL2.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ExplorerOpenGL2
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
        MouseManager mouseManager;
        XmlManager xmlManager;
        ShaderManager shaderManager;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Camera camera; 

        const int Height = 800;
        const int Width = 1280; 

        public Game1()
        {
            //this.InactiveSleepTime = TimeSpan.Zero; 
            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            //TargetElapsedTime = TimeSpan.FromSeconds(1d / 100); 
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
            camera = new Camera(new Vector2(Width, Height)); 
            DebugManager.Initialized += OnManagerInitialization;
            KeyboardManager.Initialized += OnManagerInitialization;
            GameManager.Initialized += OnManagerInitialization;
            NetworkManager.Initialized += OnManagerInitialization;
            RenderManager.Initialized += OnManagerInitialization;
            ScripterManager.Initialized += OnManagerInitialization;
            TextureManager.Initialized += OnManagerInitialization;
            FontManager.Initialized += OnManagerInitialization;
            MouseManager.Initialized += OnManagerInitialization;
            XmlManager.Initialized += OnManagerInitialization;
            ShaderManager.Initialized += OnManagerInitialization;

            Exiting += Game1_Exiting;

            gameManager = GameManager.Instance;
            keyboardManager = KeyboardManager.Instance;
            textureManager = TextureManager.Instance;
            debugManager = DebugManager.Instance;
            fontManager = FontManager.Instance;
            networkManager = NetworkManager.Instance;
            renderManager = RenderManager.Instance;
            scripterManager = ScripterManager.Instance;
            mouseManager = MouseManager.Instance;
            xmlManager = XmlManager.Instance;
            shaderManager = ShaderManager.Instance; 


            base.Initialize();
        }

        private void Game1_Exiting(object sender, EventArgs e)
        {
            //timeManager.StopUpdateThread();
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
            //gameManager.Camera.LookAt(0, 0);
            
            Window.ClientSizeChanged += UpdateDisplay;
            Window.AllowUserResizing = true;

            Texture2D attack1 = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/attack1"), 10));
            Texture2D attack2 = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/attack2"), 4));
            Texture2D attack3 = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/attack3"), 4));
            Texture2D climb = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/climb"), 4));
            Texture2D craft = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/craft"), 4));
            Texture2D death = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/death"), 4));
            Texture2D hurt = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/hurt"), 4));
            Texture2D jump = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/jump"), 4));
            Texture2D run = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/run"), 4));
            Texture2D walk = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/walk"), 4));
            Texture2D push = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/push"), 4));
            Texture2D idle = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/idle"), 4), true);
            Texture2D falling = textureManager.TrimAnimation(textureManager.ScaleTexture(textureManager.LoadTexture("Robber/falling"), 4), true);

            textureManager.LoadAnimation(attack1, 3, 750, "attack1", AlignOptions.Top);
            textureManager.LoadAnimation(attack2, 3, 750, "attack2", AlignOptions.Top);
            textureManager.LoadAnimation(attack3, 3, 750,"attack3", AlignOptions.Top);
            textureManager.LoadAnimation(climb, 3, 750,"climb", AlignOptions.Top);
            textureManager.LoadAnimation(craft, 3, 750,"craft", AlignOptions.Top);
            textureManager.LoadAnimation(death, 3, 750,"death", AlignOptions.Top);
            textureManager.LoadAnimation(hurt, 3, 750,"hurt", AlignOptions.Top);
            textureManager.LoadAnimation(idle, 4, 750,"idle", AlignOptions.Bottom);
            textureManager.LoadAnimation(jump, 6, 750, "jump", AlignOptions.Top);
            textureManager.LoadAnimation(run, 6, 750,"run", AlignOptions.Top);
            textureManager.LoadAnimation(walk, 6, 750,"walk", AlignOptions.Bottom);
            textureManager.LoadAnimation(push, 4, 750,"push", AlignOptions.Top);
            textureManager.LoadAnimation(falling, 5, 750, "falling", AlignOptions.Top);
            //Texture2D t0 = textureManager.LoadNoneContentLoadedTexture (@"C:\Users\nicol\Desktop\Light Bandit\Run\LightBandit_Run_0.png"); 
            //Texture2D t1 = textureManager.LoadNoneContentLoadedTexture(@"C:\Users\nicol\Desktop\Light Bandit\Run\LightBandit_Run_1.png");

            //Texture2D tm = textureManager.CreateAnimationFromTextures(t0, t1);

            //FileStream fs = new FileStream(@"C:\Users\nicol\Desktop\Light Bandit\Run\Out.png", FileMode.OpenOrCreate);

            //tm.SaveAsPng(fs, tm.Width, tm.Height);
            //fs.Close(); 

            new MainMenu().Show();
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
            var sprites = gameManager.GetSprites();
            if(sprites == null)
                return;
            //gameManager.MousePointer.Update(sprites);
            
            textureManager.Update(); 
            gameManager.Update(gameTime);
            debugManager.Update(gameTime);
            keyboardManager.Update();
            mouseManager.Update(gameManager.GetSprites());
            //timeManager.Update(gameTime); 

            

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
                    dm.InitDependencies(graphics);
                    break;
                case KeyboardManager km:
                    Window.TextInput += KeyboardManager.Instance.OnTextInput;
                    km.InitDependencies(); 
                    break;
                case GameManager m:
                    m.InitDependencies(camera, graphics); 
                    break;
                case NetworkManager nm:
                    nm.InitDependencies(); 
                    break;
                case ScripterManager sm:
                    sm.InitDependencies();
                    break;
                case RenderManager rm:
                    rm.InitDependencies(graphics, spriteBatch);
                    break;
                case FontManager fm:
                    fm.InitDependencies(Content);
                    break;
                case MouseManager mm:
                    mm.InitDependencies(gameManager.MousePointer); 
                    break;
                case XmlManager xm:
                    xm.InitDependencies();
                    break;
                case ShaderManager sm:
                    sm.InitDependencies(graphics, Content, spriteBatch);
                    break;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            lock (gameManager.sprites)
            {
                //float la = timeManager.LerpAmount;
                gameManager.Camera.Update(1f);
                
                Sprite[] sprites = gameManager.sprites.ToArray(); 
                if (sprites == null)
                    return;
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: gameManager.Camera.Transform);

                for (int i = 0; i < sprites.Length; i++)
                {
                    if (!sprites[i].IsHUD)
                    {
                        //while (timeManager.IsUpdating) ;
                        sprites[i].Draw(spriteBatch, gameTime, 1f);
                    }
                }

                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                for (int i = 0; i < sprites.Length; i++)
                {
                    
                    if (sprites[i].IsHUD)
                    {
                        //while (timeManager.IsUpdating);
                        sprites[i].Draw(spriteBatch, gameTime, 1f);
                    }
                }

                if (debugManager.IsDebuging)
                    debugManager.DebugDraw(spriteBatch);

                spriteBatch.End();
            }
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
