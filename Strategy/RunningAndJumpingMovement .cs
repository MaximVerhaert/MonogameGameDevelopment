using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Strategy
{
    public class RunningAndJumpingMovement : IMovementStrategy
    {
        public Vector2 Move(Vector2 position, Vector2 velocity, bool isGrounded, GameTime gameTime, Vector2 direction, float jumpStrength)
        {
            // Example movement logic
            if (isGrounded && direction.Y < 0) // Jumping logic
            {
                velocity.Y = -jumpStrength;
            }

            // Apply horizontal movement
            velocity.X = direction.X * 200f; // Adjust 200f for movement speed

            // Apply gravity
            if (!isGrounded)
            {
                velocity.Y += 9.8f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Calculate new position
            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            return position;
        }
    }
}
