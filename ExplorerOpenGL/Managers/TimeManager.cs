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

        Timer UpdateTimer; 

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
        public void StartUpdateTimer(double UpdateRate)
        {
            UpdateTimer = new Timer(new TimerCallback(Update));
            UpdateTimer.Change(100, 16);
        }

        public void Update(object state)
        {
            List<Sprite> sprites = gameManager._sprites; 
            Texture2D texture = TextureManager.Instance.OutlineTextThread("coucou", "Default", Color.Black, Color.Black, 1);
            sprites.Add(new Wall(texture));
            LastUpdateTime = DateTime.Now; 
            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i].IsRemove)
                {
                    sprites.RemoveAt(i);
                    i--;
                }
                sprites[i].Update(sprites);
            }
        }
    }
}
