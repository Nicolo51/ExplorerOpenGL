using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers
{
    public class ThreadManager
    {
        public static event EventHandler Initialized;
        private static ThreadManager instance;
        public static ThreadManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ThreadManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }
        
        GameManager gameManager; 

        private ThreadManager()
        {
            this.gameManager = GameManager.Instance; 
        }

        public void InitDependencies(GameManager gameManager)
        {
        }

        public void StartThread(Func<object> func, Action<object> callback)
        {
            new Thread(() =>
            {
                var result = func.Invoke();
                gameManager.AddActionToUIThread(callback, result);
            }).Start(); 
        }
    }
}
