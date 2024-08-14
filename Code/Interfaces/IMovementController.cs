using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Interfaces
{
    public interface IMovementController
    {
        Vector2 UpdateMovement(Vector2 direction, GameTime gameTime);
        void SetMaxAcceleration(float maxAcceleration);
    }
}
