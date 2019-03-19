using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controllers
{
    public static class Controller
    {
        public static KeyboardUtils KeyboardUtils = new KeyboardUtils();
        public static DebugManager DebugManager; //instantiate on load
        public static TextureManager TextureManager; //instantiate on load 

        public static void Update()
        {
            if(KeyboardUtils != null && TextureManager != null && DebugManager != null)
            {
                KeyboardUtils.Update();
                DebugManager.Update();
            }
            else
            {
                throw new NullReferenceException("Toutes les instances des controllers doivent être initialisées"); 
            }

        }
    }
}
