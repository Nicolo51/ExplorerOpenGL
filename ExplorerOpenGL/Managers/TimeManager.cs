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
        public TimeSpan LastUpdateTime { get; set; }
        public TimeSpan ElapsedUpdate { get { return gameTime.TotalGameTime - LastUpdateTime; } }
        public TimeSpan ElapsedDraw { get { return gameTime.ElapsedGameTime; } }
        public TimeSpan ElapsedBetweenUpdates { get; private set; }
        public TimeSpan TotalTime { get { return gameTime.TotalGameTime;  } }
        public float LerpAmount { get { float la = (float)((gameTime.TotalGameTime - LastUpdateTime).TotalMilliseconds / TickRate); return(la < 0)?  0: la; } }
        public int AverageFps { get; set; }

        Thread UpdateThread;
        public int TickRate { get; private set; } = 16; //Targeted update timer in ms 

        private GameTime gameTime; 

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
            gameManager = GameManager.Instance;
            debugManager = DebugManager.Instance;
            gameTime = new GameTime(); 
            
        }

        public void InitDependencies()
        {
             
        }

        public void StartUpdateThread()
        {
            UpdateThread = new Thread(new ThreadStart(UpdateSprite));
            UpdateThread.Start(); 
        }

        public void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;
        }

        public void UpdateSprite()
        {
            byte count = 0;
            TimeSpan countLastDraw = TimeSpan.FromSeconds(0); 
            while (true)
            {
                count++;
                if (count == 50)
                {
                    AverageFps = (int)(1000/(countLastDraw.TotalMilliseconds/ 50));
                    gameManager.SortSprites();
                    countLastDraw = TimeSpan.Zero; 
                    count = 0; 
                }
                countLastDraw += gameTime.ElapsedGameTime; 


                Sprite[] sprites;
                sprites = gameManager.GetSprites();                
                ElapsedBetweenUpdates = gameTime.TotalGameTime - LastUpdateTime; 
                LastUpdateTime = gameTime.TotalGameTime;
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
                            if (i < 0)
                                i--;
                            continue;
                        }
                        sprites[i].Update(sprites);
                    }
                }
                if ((gameTime.TotalGameTime - LastUpdateTime).TotalMilliseconds > TickRate)
                    continue; 
                else
                {
                    int sleepTime = Convert.ToInt32(TickRate - (gameTime.TotalGameTime - LastUpdateTime).TotalMilliseconds);
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
