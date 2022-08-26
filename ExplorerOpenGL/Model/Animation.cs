using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ExplorerOpenGL.Model
{
    public class Animation
    {
        public Texture2D Texture { get; private set; }
        private int nbrFrames;
        private float loopTime;
        public Vector2 Bounds{ get { return new Vector2(Texture.Width / nbrFrames, Texture.Height / nbrFrames); } }
        private int width { get { return Texture.Width / nbrFrames;  } }
        private int height { get { return Texture.Height;  } }
        private int idFrame;
        private float TimeBetweenFrames; 
        public bool IsPlaying { get; private set; }
        public float timer; 
        public Animation(Texture2D texture, int nbrFrames, float loopTime)
        {
            idFrame = 0;
            timer = 0f; 
            TimeBetweenFrames = (float)loopTime / nbrFrames; 
            if (nbrFrames < 1)
                throw new Exception("nbrFrames must be greater than 1"); 
            if( loopTime < 0 )
                throw new Exception("loopTime must be greater than 0 sec");

            this.Texture = texture;
            this.nbrFrames = nbrFrames;
            this.loopTime = loopTime; 
        }
        public Rectangle GetRectangle(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            timer = timer % loopTime;


            idFrame = (int)(timer/TimeBetweenFrames);
            return new Rectangle((int)idFrame * width, 0, width, height); 
        }

        public void Play()
        {
            IsPlaying = true; 
        }

        public void Stop()
        {
            IsPlaying = false; 
        }
    }
}
