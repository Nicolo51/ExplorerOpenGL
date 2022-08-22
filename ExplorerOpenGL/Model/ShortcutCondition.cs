using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model
{
    public class ShortcutCondition
    {
        public Keys[] InitialKeys;
        public Keys TriggerKey; 

        public ShortcutCondition(Keys[] initialKeys, Keys triggerKey)
        {
            InitialKeys = initialKeys;
            TriggerKey = triggerKey; 
        }
        
        public bool InitialKeysPressed()
        {
            var ks = Keyboard.GetState(); 
            foreach(var k in InitialKeys)
            {
                if (ks.IsKeyUp(k))
                    return false; 
            }
            return true; 
        }
    }
}
