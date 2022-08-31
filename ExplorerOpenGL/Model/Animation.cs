using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ExplorerOpenGL.Model
{
    public class Animation
    {
        public Texture2D Texture { get; private set; }
        public string Name { get; private set; }
        public string playAfter; 

        private bool isLooping; 
        private int nbrFrames;
        private float loopTime;
        public Vector2 Bounds{ get { return new Vector2(Texture.Width / nbrFrames, Texture.Height / nbrFrames); } }
        private int width { get { return Texture.Width / nbrFrames;  } }
        private int height { get { return Texture.Height;  } }
        private int idFrame;
        private float TimeBetweenFrames; 
        public bool IsPlaying { get; private set; }
        public float timer;
        public Animation(Texture2D texture, int nbrFrames, float loopTime, string name, bool isLooping = true, string playAfter = null)
        {
            if (nbrFrames < 1)
                throw new Exception("nbrFrames must be greater than 1");
            if (loopTime < 0)
                throw new Exception("loopTime must be greater than 0 sec");

            this.Name = name; 
            this.playAfter = playAfter;
            this.isLooping = isLooping; 
            this.Texture = texture;
            this.nbrFrames = nbrFrames;
            this.loopTime = loopTime;

            idFrame = 0;
            timer = 0f;
            TimeBetweenFrames = loopTime / nbrFrames;
        }

        public void AddAfter(string animationName)
        {
            playAfter = animationName; 
        }

        public Rectangle GetRectangle(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            float aTimer = timer % loopTime;
            if(timer < loopTime || isLooping)
                idFrame = (int)(aTimer / TimeBetweenFrames);
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
