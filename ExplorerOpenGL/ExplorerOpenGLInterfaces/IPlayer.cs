using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGLInterfaces
{
    public interface IPlayer
    {
        Vector2 getDirection();
        Vector2 getPosition(); 
        IPlayer GetOnlineInfo();
    }
}
