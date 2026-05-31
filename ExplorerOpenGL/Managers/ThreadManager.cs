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
        public static void StartThread(Func<object> func, Action<object> callback)
        {
            new Thread(() =>
            {
                var result = func.Invoke();
                GameManager.AddActionToUIThread(callback, result);
            }).Start(); 
        }
    }
}
