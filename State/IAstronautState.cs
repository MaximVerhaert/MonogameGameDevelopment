using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.State
{
    public interface IAstronautState
    {
        void Enter(Astronaut astronaut);
        void HandleInput(Astronaut astronaut, Vector2 direction);
        void Update(Astronaut astronaut, GameTime gameTime);
        void Exit(Astronaut astronaut);
    }
}
