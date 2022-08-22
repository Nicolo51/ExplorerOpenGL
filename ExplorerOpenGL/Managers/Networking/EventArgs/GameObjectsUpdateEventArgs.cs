using ExplorerOpenGL.Managers.Networking.NetworkObject;
using ExplorerOpenGL.Model.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.EventArgs
{
    public class GameObjectsUpdateEventArgs : NetworkEventArgs
    {
        public NetworkGameObject[] networkGameObjects { get; set; }
    }
}
