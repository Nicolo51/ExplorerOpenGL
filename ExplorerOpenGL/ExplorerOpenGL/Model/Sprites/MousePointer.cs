using ExplorerOpenGL.Controlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class MousePointer : Sprite
    {
        MouseState currentMouseState;
        MouseState prevMouseState;
        
        public new Rectangle HitBox { get { return new Rectangle(Position.ToPoint(), new Point(1, 1)); } }

        public MousePointer(Texture2D texture)
            : base()
        {

        }
        public MousePointer()
        {

        }

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            if(currentMouseState != null)
            {
                prevMouseState = currentMouseState; 
            }
            currentMouseState = Mouse.GetState(); 
            

            base.Update(gameTime, sprites, controler);
        }

        public override string ToString()
        {
            return "Position X : " + currentMouseState.Position.X + "\nPosition Y : " + currentMouseState.Position.Y;
        }
    }
}
