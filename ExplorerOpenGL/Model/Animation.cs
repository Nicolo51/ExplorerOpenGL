using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model
{
    public class Animation
    {
        private Texture2D texture;
        private int nbrFrames;
        private TimeSpan loopTime; 
        public Animation(Texture2D texture, int nbrFrames, TimeSpan loopTime)
        {
            if (nbrFrames < 1)
                throw new Exception("nbrFrames must be greater than 1"); 
            if( loopTime < TimeSpan.Zero )
                throw new Exception("loopTime must be greater than 0 sec");

            this.texture = texture;
            this.nbrFrames = nbrFrames;
            this.loopTime = loopTime; 
        }
    }
}
