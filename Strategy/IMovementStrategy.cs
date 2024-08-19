using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Strategy
{
    public interface IMovementStrategy
    {
        Vector2 Move(Vector2 position, Vector2 velocity, bool isGrounded, GameTime gameTime, Vector2 direction, float jumpStrength);
    }

}

