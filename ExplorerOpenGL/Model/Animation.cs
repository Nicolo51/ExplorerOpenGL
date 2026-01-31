using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ExplorerOpenGL2.Model
{
    public class Animation : ICloneable
    {
        public Texture2D Texture { get; private set; }
        public string Name { get; private set; }
        public bool IsLooping;  
        public int FrameCount { get; private set; }
        public float LoopTime { get; private set; }
        public Vector2 Bounds{ get { return new Vector2(Texture.Width / FrameCount, Texture.Height); } }
        private int width { get { return Texture.Width / FrameCount;  } }
        private int height { get { return Texture.Height;  } }
        private int idFrame;
        private float TimeBetweenFrames; 
        public bool IsPlaying { get; private set; }
        public bool IsFinished { get; private set; }
        public AlignOptions AlignOption; 
        public Animation(Texture2D texture, int nbrFrames, float loopTime, string name, AlignOptions alignOption, bool isLooping = true)
        {
            if (nbrFrames < 1)
                throw new Exception("nbrFrames must be greater than 1");
            if (loopTime < 0)
                throw new Exception("loopTime must be greater than 0 sec");


            AlignOption = alignOption; 
            this.Name = name;
            IsFinished = false;  
            this.IsLooping = isLooping; 
            this.Texture = texture;
            this.FrameCount = nbrFrames;
            this.LoopTime = loopTime;

            idFrame = 0;
            TimeBetweenFrames = loopTime / nbrFrames;
        }

        public Rectangle GetRectangle(GameTime gameTime, float timer)
        {
            
            float aTimer = timer % LoopTime;
            if (timer < LoopTime || IsLooping)
                idFrame = (int)(aTimer / TimeBetweenFrames);
            else
                IsFinished = true; 
            return new Rectangle((int)idFrame * width, 0, width, height); 
        }

        public void Play()
        {
            IsPlaying = true;
            IsFinished = false;
            idFrame = 0;
        }

        public void Stop()
        {
            IsPlaying = false;
            IsFinished = true;
            idFrame = 0;
        }

        public object Clone()
        {
            return this.MemberwiseClone(); 
        }
    }
}
