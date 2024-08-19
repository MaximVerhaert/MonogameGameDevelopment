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
        public Vector2 Move(Vector2 position, Vector2 velocity, bool isGrounded, GameTime gameTime)
        {
            if (isGrounded)
                velocity.Y = -60f;

            velocity.Y += 9.81f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            return position;
        }
    }
}
