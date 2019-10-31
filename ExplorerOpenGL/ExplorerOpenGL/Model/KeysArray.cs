using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model
{
    public class KeysArray
    {
        public Keys[] Keys { get; set; }

        public KeysArray(Keys[] Keys)
        {
            this.Keys = Keys;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Keys pressed : ");
            foreach (Keys k in Keys)
            {
                sb.Append(k.ToString());
                if(Keys.Length > 1)
                {
                    sb.Append(","); 
                }
            }
            return sb.ToString(); 
        }

        public Keys[] GetArray()
        {
            return this.Keys; 
        }
    }
}
