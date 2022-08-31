using ExplorerOpenGL.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class AnimationManager
    {
        public Texture2D Texture { get { return currentAnimation.Texture; } }
        private Dictionary<string, Animation> animations;
        private Animation currentAnimation; 

        public AnimationManager()
        {
            animations = new Dictionary<string, Animation>(); 
        }

        public void Play(string animationName, bool looping = true)
        {
            if (!animations.Keys.Contains(animationName))
                return;

            currentAnimation = animations[animationName]; 
        }

        public void Stop()
        {

        }

        public void Add(string animationName, Animation animation)
        {
            animations[animationName] = animation; 
        }

        public void Remove(string animationName)
        {

        }
        public void Remove(Animation animation)
        {

        }
        public Rectangle GetRectangle(GameTime gameTime)
        {
            return currentAnimation.GetRectangle(gameTime); 
        }
    }
}
