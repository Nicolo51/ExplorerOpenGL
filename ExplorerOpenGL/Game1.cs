using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<Sprite> _sprites;
        Camera camera; 

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

            gameManager = GameManager.Instance;
            keyboardManager = KeyboardManager.Instance;
            textureManager = TextureManager.Instance;
            debugManager = DebugManager.Instance;
            fontManager = FontManager.Instance;
            networkManager = NetworkManager.Instance;
            renderManager = RenderManager.Instance;
            scripterManager = ScripterManager.Instance; 

            base.Initialize();
        }


        protected override void LoadContent()
        {

            //new Thread(() =>
            //{
            //    _sprites.Add(new Wall(Manager.TextureManager.LoadNoneContentLoadedTexture(@"D:\Mes documents\Images\Wlop\2018 September 1\2_Invitation_4k.jpg"))
            //    {
            //        Position = new Vector2(100, 100),
            //    });
            //}).Start();
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
            MessageBox.Show("Error", "Something went wrong and it's really a big deal. Je reprends en français parceque c'est quand même plus rigolo, en vrai faut faire quelque chose, ça va vrament pas la !", MessageBoxType.YesNo);
            Window.ClientSizeChanged += UpdateDisplay;
            Window.AllowUserResizing = true;
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
            if(_sprites == null)
                return;

            gameManager.Camera.Update();
            gameManager.Update(gameTime);
            debugManager.Update();
            keyboardManager.Update();
            networkManager.Update(gameTime); 

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
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (_sprites == null)
                return;
            spriteBatch.Begin(SpriteSortMode.BackToFront, transformMatrix: gameManager.Camera.Transform);

            foreach (Sprite sprite in _sprites.Where(e => !e.IsHUD))
            {
                sprite.Draw(spriteBatch);
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront);

            foreach (Sprite sprite in _sprites.Where(e => e.IsHUD))
            {
                sprite.Draw(spriteBatch);
            }

            if (DebugManager.Instance.IsDebuging)
                DebugManager.Instance.DebugDraw(spriteBatch);

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
