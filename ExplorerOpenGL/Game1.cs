using ExplorerOpenGL.Managers;
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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

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
            Window.IsBorderless = false; 
            
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            ConstantManager.Init(); 
            graphics.PreferredBackBufferHeight = ConstantManager.HEIGHT.GetValue<int>();
            graphics.PreferredBackBufferWidth = ConstantManager.WIDTH.GetValue<int>();
            graphics.IsFullScreen = ConstantManager.FULLSCREEN.GetValue<bool>(); 

        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            Rectangle bounds = (Rectangle)sender.GetType().GetProperty("ClientBounds").GetValue(sender); 
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Exiting += Game1_Exiting;
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
            //GameManager.Camera.LookAt(0, 0);
            
            Window.ClientSizeChanged += UpdateDisplay;
            Window.AllowUserResizing = true;
            InitManager();

            Texture2D attack1 = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/attack1"), 4));
            Texture2D attack2 = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/attack2"), 4));
            Texture2D attack3 = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/attack3"), 4));
            Texture2D climb = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/climb"), 4));
            Texture2D craft = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/craft"), 4));
            Texture2D death = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/death"), 4));
            Texture2D hurt = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/hurt"), 4));
            Texture2D jump = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/jump"), 4));
            Texture2D run = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/run"), 4));
            Texture2D walk = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/walk"), 4));
            Texture2D push = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/push"), 4));
            Texture2D idle = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/idle"), 4), true);
            Texture2D falling = TextureManager.TrimAnimation(TextureManager.ScaleTexture(TextureManager.LoadTexture("Robber/falling"), 4), true);

            TextureManager.LoadAnimation(attack1, 6, 400, "attack1", AlignOptions.Top);
            TextureManager.LoadAnimation(attack2, 6, 400, "attack2", AlignOptions.Top);
            TextureManager.LoadAnimation(attack3, 6, 400, "attack3", AlignOptions.Top);
            TextureManager.LoadAnimation(climb, 3, 750,"climb", AlignOptions.Top);
            TextureManager.LoadAnimation(craft, 3, 750,"craft", AlignOptions.Top);
            TextureManager.LoadAnimation(death, 3, 750,"death", AlignOptions.Top);
            TextureManager.LoadAnimation(hurt, 3, 750,"hurt", AlignOptions.Top);
            TextureManager.LoadAnimation(idle, 4, 750,"idle", AlignOptions.Bottom);
            TextureManager.LoadAnimation(jump, 6, 750, "jump", AlignOptions.Top);
            TextureManager.LoadAnimation(run, 6, 750,"run", AlignOptions.Top);
            TextureManager.LoadAnimation(walk, 6, 750,"walk", AlignOptions.Bottom);
            TextureManager.LoadAnimation(push, 4, 750,"push", AlignOptions.Top);
            TextureManager.LoadAnimation(falling, 5, 750, "falling", AlignOptions.Top);
            //Texture2D t0 = TextureManager.LoadNoneContentLoadedTexture (@"C:\Users\nicol\Desktop\Light Bandit\Run\LightBandit_Run_0.png"); 
            //Texture2D t1 = TextureManager.LoadNoneContentLoadedTexture(@"C:\Users\nicol\Desktop\Light Bandit\Run\LightBandit_Run_1.png");

            //Texture2D tm = TextureManager.CreateAnimationFromTextures(t0, t1);

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
            var sprites = GameManager.GetSprites();
            if(sprites == null)
                return;
            //GameManager.MousePointer.Update(sprites);
            
            TextureManager.Update(); 
            GameManager.Update(gameTime);
            DebugManager.Update(gameTime);
            KeyboardManager.Update();
            MouseManager.Update(GameManager.GetSprites());
            //timeManager.Update(gameTime); 

            

            base.Update(gameTime);
        }

        private void InitManager()
        {
            Window.TextInput += KeyboardManager.OnTextInput;
            RenderManager.InitDependencies(graphics, spriteBatch);
            FontManager.InitDependencies(Content);
            MouseManager.InitDependencies(GameManager.MousePointer); 
            XmlManager.InitDependencies();
            ShaderManager.InitDependencies(graphics, Content, spriteBatch);
            NetworkManager.InitDependencies();
            KeyboardManager.InitDependencies();
            TextureManager.InitDependencies(graphics, Content, spriteBatch); 
            GameManager.InitDependencies(graphics, this); 
            DebugManager.InitDependencies(graphics);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            lock (GameManager.sprites)
            {
                //float la = timeManager.LerpAmount;
                GameManager.Camera.Update(1f);
                
                Sprite[] sprites = GameManager.sprites.ToArray(); 
                if (sprites == null)
                    return;
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: GameManager.Camera.Transform);

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

                if (DebugManager.IsDebuging)
                    DebugManager.DebugDraw(spriteBatch);

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
