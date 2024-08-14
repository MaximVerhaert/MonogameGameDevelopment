using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Animation
{
    public class Animatie
    {
        public AnimationFrame CurrentFrame { get; set; }

        private Dictionary<string, List<AnimationFrame>> animations;

        private List<AnimationFrame> currentAnimationFrames;
        
        private int counter;

        private double frameMovement = 0;

        public Animatie() {
            animations = new Dictionary<string, List<AnimationFrame>>();
        }


        public void AddAnimation(string animationName, List<AnimationFrame> frames)
        {
            animations[animationName] = frames;
            if (currentAnimationFrames == null)
            {
                currentAnimationFrames = frames;
                CurrentFrame = currentAnimationFrames[0];
            }
        }

        public void Play(string animationName)
        {
            //enkel veranderen van animatie als deze anders is dan de huidige animatie
            if (!animations.ContainsKey(animationName) || currentAnimationFrames == animations[animationName])
                return;

            // veranderen van de animatie
            currentAnimationFrames = animations[animationName];
            counter = 0;
            frameMovement = 0;
            CurrentFrame = currentAnimationFrames[0];
        }

        public void Update(GameTime gameTime)
        {
            if (currentAnimationFrames == null || currentAnimationFrames.Count == 0) return;

            CurrentFrame = currentAnimationFrames[counter];
            frameMovement += CurrentFrame.SourceRectangle.Width * gameTime.ElapsedGameTime.TotalSeconds;

            if (frameMovement >= CurrentFrame.SourceRectangle.Width / 9)
            {
                counter++;
                frameMovement = 0;
            }

            if (counter >= currentAnimationFrames.Count)
            {
                counter = 0;

            }
        }
    }
}
