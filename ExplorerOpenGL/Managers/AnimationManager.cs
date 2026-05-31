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
        private bool playToEnd; 

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
        public bool Play(string animationName, bool playToEnd)
        {
            if (!animations.Keys.Contains(animationName))
                return false;

            return Play(animations[animationName], playToEnd);
        }
        public bool Play(Animation animation, bool playToEnd)
        {
            if (this.playToEnd && !currentAnimation.IsFinished)
                return false;
            
            if (animation == currentAnimation)
                return false;
            timer = 0f;
            currentAnimation = animation;
            currentAnimation.Play();
            this.playToEnd = playToEnd;
            return true; 
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
                Play(playAfter[currentAnimation], false); 
            }
            return currentAnimation.GetRectangle(gameTime, timer); 
        }
    }
}
