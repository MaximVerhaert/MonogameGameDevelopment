using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Strategy
{
    public class JumpingMovement : IMovementStrategy
    {
        private Random random = new Random();

        public Vector2 Move(Vector2 position, Vector2 velocity, bool isGrounded, GameTime gameTime, Vector2 direction, float jumpStrength)
        {
            // If the enemy is grounded, choose a random direction (left or right)
            if (isGrounded)
            {
                direction = random.Next(2) == 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
                velocity.X = direction.X * 10; // Move 10px left or right
            }

            // Apply jump strength if the enemy is grounded
            if (isGrounded)
            {
                velocity.Y = -jumpStrength;
                isGrounded = false;
            }

            // Apply gravity
            velocity.Y += 9.81f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update position
            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            return position;
        }
    }
}

