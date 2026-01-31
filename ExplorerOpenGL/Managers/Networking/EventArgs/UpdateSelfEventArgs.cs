using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking.EventArgs
{
    public class UpdateSelfEventArgs : NetworkEventArgs
    {
        public Vector2  Position { get; set; }
        public int Health { get; set; }
        public string Name { get; set; }
    }
}
