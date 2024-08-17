using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Code
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        public Vector2 Position { get; private set; }
        private readonly Viewport _viewport;

        public Camera(Viewport viewport)
        {
            _viewport = viewport;
        }

        public void Update(Vector2 targetPosition)
        {
            // Center the camera on the target (player)
            Position = targetPosition - new Vector2(_viewport.Width / 2f, _viewport.Height / 2f);

            // Create the transform matrix
            Transform = Matrix.CreateTranslation(
                -Position.X,
                -Position.Y,
                0);
        }
    }
}