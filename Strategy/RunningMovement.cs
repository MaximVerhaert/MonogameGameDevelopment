using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Strategy
{
    public class RunningMovement : IMovementStrategy
    {
        public Vector2 Move(Vector2 position, Vector2 velocity, bool isGrounded, GameTime gameTime)
        {
            // Running movement logic here
            // If needed, you can use the isGrounded parameter to adjust the behavior
            if (isGrounded)
            {
                // Movement logic when grounded
                position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            return position;
        }
    }
}

