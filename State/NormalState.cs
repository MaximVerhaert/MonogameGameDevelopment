using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.State
{
    public class NormalState : IAstronautState
    {
        public void Enter(Astronaut astronaut)
        {
            // Code to handle what happens when entering the NormalState
        }

        public void Exit(Astronaut astronaut)
        {
            // Code to handle what happens when exiting the NormalState
        }

        public void HandleInput(Astronaut astronaut, Vector2 input)
        {
            // Code to handle input while in NormalState
        }

        public void Update(Astronaut astronaut, GameTime gameTime)
        {
            // Update logic for NormalState
            astronaut.Move(gameTime);
        }
    }
}
