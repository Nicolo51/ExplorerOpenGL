using ExplorerOpenGL2.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers
{
    public class AnimationManager
    {
        public Texture2D  Texture { get { return currentAnimation.Texture; } }
        private Dictionary<string, Animation> animations;
        private Dictionary<Animation, Animation> playAfter; 
        public Animation currentAnimation { get; private set; }
        public int Count { get => animations.Count;  }

        private float timer; 

        public AnimationManager()
        {
            timer = 0f; 
            animations = new Dictionary<string, Animation>();
            playAfter = new Dictionary<Animation, Animation>(); 
        }

        public void PlayAfterAnimation(string animationName, string afterAnimationName)
        {
            if (animations.ContainsKey(animationName) && animations.ContainsKey(afterAnimationName))
                PlayAfterAnimation(animations[animationName], animations[afterAnimationName]); 
        }

        public void PlayAfterAnimation(Animation animation, Animation afterAnimation)
        {
            playAfter.Add(animation, afterAnimation);
        }

        public Vector2 GetBounds()
        {
            return new Vector2(currentAnimation.Bounds.X, currentAnimation.Bounds.Y);
        }
        public void Play(string animationName, bool looping = true)
        {
            if (!animations.Keys.Contains(animationName))
                return;

            Play(animations[animationName]);
        }
        public void Play(Animation animation)
        {
            if (animation == currentAnimation)
                return;
            timer = 0f;
            currentAnimation = animation;
            currentAnimation.Play();
        }
        public void Stop()
        {
            timer = 0f; 
            currentAnimation.Stop(); 
        }

        public void Add(string animationName, Animation animation)
        {
            animations[animationName] = animation; 
        }

        public void Add(Animation animation)
        {
            Add(animation.Name, animation);
        }

        public void Remove(string animationName)
        {
            if (animations.ContainsKey(animationName))
                animations.Remove(animationName); 
        }
        public void Remove(Animation animation)
        {
            Remove(animation.Name); 
        }
        public Rectangle GetRectangle(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (currentAnimation.IsFinished && playAfter.ContainsKey(currentAnimation))
            {
                Play(playAfter[currentAnimation]); 
            }
            return currentAnimation.GetRectangle(gameTime, timer); 
        }
    }
}
