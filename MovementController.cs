using Code.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code
{
    public class MovementController : IMovementController
    {
        private Vector2 originalSpeed;
        private Vector2 speed;
        private Vector2 acceleration;
        private float maxAcceleration;

        public MovementController(Vector2 initialSpeed, Vector2 initialAcceleration, float maxAcceleration)
        {
            originalSpeed = initialSpeed;
            speed = initialSpeed;
            acceleration = initialAcceleration;
            this.maxAcceleration = maxAcceleration;
        }

        public void SetMaxAcceleration(float maxAcceleration)
        {
            this.maxAcceleration = maxAcceleration;
        }

        public Vector2 UpdateMovement(Vector2 direction, GameTime gameTime)
        {
            if (direction == Vector2.Zero){
                speed = originalSpeed;
            }
            else{
                speed += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                speed = AccelerationLimit(speed, maxAcceleration);
            }
            return direction * speed;
        }

        private Vector2 AccelerationLimit(Vector2 v, float max)
        {
            if (v.Length() > max)
            {
                var ratio = max / v.Length();
                v *= ratio;
            }
            return v;
        }
    }
}
