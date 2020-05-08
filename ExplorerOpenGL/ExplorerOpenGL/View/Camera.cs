using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    public class Camera
    {
        public Matrix Transform { get; set; }
        public Vector2 Bounds { get; set; }
        public Sprite spriteToFollow { get; private set; } 

        public Camera(Vector2 bounds)
        {
            this.Bounds = bounds; 
        }

        public void Update()
        {
            Matrix position = Matrix.CreateTranslation(
                -spriteToFollow.Position.X ,
                -spriteToFollow.Position.Y ,
                0);

            Matrix offset = Matrix.CreateTranslation(
                    Bounds.X/ 2,
                    Bounds.Y / 2,
                    0);

            Transform = position *  offset;
        }

        public void Follow(Sprite target)
        {
            spriteToFollow = target; 
        }
    }
}
