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
                {"Menu", Content.Load<SpriteFont>("Fonts/Menu") },
            };

            _sprites = new List<Sprite>();

            controler = new Controler(Window, fonts, _sprites, graphics, Content, spriteBatch, new Vector2(Width, Height));
             
            Texture2D player = controler.TextureManager.LoadTexture("player");
            Texture2D playerfeet = controler.TextureManager.LoadTexture("playerfeet");

            //new Thread(() =>
            //{
            //    _sprites.Add(new Wall(controler.TextureManager.LoadNoneContentLoadedTexture(@"D:\Mes documents\Images\Wlop\2018 September 1\2_Invitation_4k.jpg"))
            //    {
            //        Position = new Vector2(100, 100),
            //    });
            //}).Start();
            //Player Player = new Player(player, playerfeet, controler.MousePointer, "Nicolas", controler.TextureManager)
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
            //controler.Player = this.player;
            //_sprites.Add(Player);
            //_sprites.Add(new Wall(controler.TextureManager.CreateTexture(1000, 50, paint => (paint % 2 == 0)? Color.White : Color.Black)));
            //_sprites.Add(new Button(controler.TextureManager.CreateTexture(200, 200, paint => Color.Black), controler.TextureManager.CreateTexture(200, 200, paint => Color.Red), fonts["Default"])); 
            //controler.Camera.FollowSprite(Player);
            controler.Camera.LookAt(0, 0); 

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
            controler.Camera.Update();

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
            controler.Update(_sprites, gameTime);

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
            spriteBatch.Begin(SpriteSortMode.BackToFront, transformMatrix: controler.Camera.Transform);

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
