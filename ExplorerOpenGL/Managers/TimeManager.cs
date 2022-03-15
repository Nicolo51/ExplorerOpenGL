using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class TimeManager
    {
        public DateTime StartedTime { get; private set; }
        public DateTime LastDrawUpdateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime LastDrawTime { get; set; }
        public TimeSpan ElapsedUpdate { get { return DateTime.Now - LastUpdateTime; } }
        public TimeSpan ElapsedDraw { get { return DateTime.Now - LastDrawTime; } }
        public TimeSpan ElapsedDrawUpdate { get { return DateTime.Now - LastDrawUpdateTime; } }
        public float LerpAmount { get { float la = (float)((LastDrawTime - LastUpdateTime).TotalMilliseconds / TickRate); return(la < 0)?  0: la; } }

        Thread UpdateThread;
        public int TickRate { get; private set; } = 16;

        public static event EventHandler Initialized;
        private static TimeManager instance; 
        public static TimeManager Instance { get
            {
                if (instance == null)
                {
                    instance = new TimeManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance; 
            }
        }

        private GameManager gameManager;
        private DebugManager debugManager; 
        
        private TimeManager()
        {
            StartedTime = DateTime.Now;
            LastDrawUpdateTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            LastDrawTime = DateTime.Now;

            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance; 
            
        }
        public void StartUpdateThread()
        {
            UpdateThread = new Thread(new ThreadStart(Update));
            UpdateThread.Start(); 
        }

        public void Update()
        {

            while (true)
            {
                Sprite[] sprites; 
                lock (gameManager.sprites)
                {
                    sprites = gameManager.sprites.ToArray(); 
                }
                //Texture2D texture = TextureManager.Instance.OutlineText("coucou", "Default", Color.Black, Color.Black, 1);
                //Texture2D texture = TextureManager.Instance.CreateBorderedTexture(100, 100, 10, 3, backgroundPaint => Color.BlueViolet, backgroundPaint => Color.Yellow);
                //Texture2D texture = TextureManager.Instance.CreateTexture(100, 100, paint => Color.Red);
                //Texture2D texture = TextureManager.Instance.TextureText("coucou", "Default", Color.Red);

                //sprites.Add(new Wall(texture));
                LastUpdateTime = DateTime.Now;
                //Parallel.For(0, sprites.Count, (int i) =>
                //{
                //    if (sprites[i] == null)
                //        return;
                //    sprites[i].Update(sprites);
                //});
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i] == null)
                        continue;
                    lock (sprites[i])
                    {
                        if (sprites[i].IsRemove)
                        {
                            gameManager.RemoveSprite(sprites[i]);
                            continue;
                        }
                        sprites[i].Update(sprites);
                    }
                }
                if ((DateTime.Now - LastUpdateTime).TotalMilliseconds > TickRate)
                    continue; 
                else
                {
                    int sleepTime = Convert.ToInt32(TickRate - (DateTime.Now - LastUpdateTime).TotalMilliseconds);
                    if (sleepTime < 1)
                        continue;
                    Thread.Sleep(sleepTime);
                }
            }
        }

        public void StopUpdateThread()
        {
            UpdateThread.Abort();
        }
    }
}
