using Code.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Code.Animation;


namespace Code
{
    public class Astronaut:IGameObject
    {
        Texture2D astronautTexture;
        Animatie animatie;
        private Vector2 position;
        private Vector2 speed;
        private Vector2 acceleration;

        public Astronaut(Texture2D texture) {
            astronautTexture = texture;
            animatie = new Animatie();
            animatie.AddFrame(new AnimationFrame(new Rectangle(0, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(64, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(128, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(192, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(256, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(320, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(384, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(448, 0, 64, 64)));
            animatie.AddFrame(new AnimationFrame(new Rectangle(512, 0, 64, 64)));
            position = new Vector2(10, 10);
            speed = new Vector2(1, 1);
            acceleration = new Vector2(0.1f, 0.1f);
        }

        public void Update(GameTime gametime)
        {
            Move();
            animatie.Update(gametime);
        }

        private void Move()
        {
            position += speed;
            speed += acceleration;
            speed = AccelerationLimit(speed, 5);

            if(position.X > 600 || position.X < 0)
            {
                speed.X *= -1;
                acceleration.X *= -1;
            }
            if (position.Y > 400 || position.Y < 0)
            {
                speed.Y *= -1;
                acceleration.Y *= -1;
            }
        }

        private Vector2 AccelerationLimit(Vector2 v, float max)
        {
            if(v.Length() > max)
            {
                var ratio = max / v.Length();
                v.X *= ratio;
                v.Y *= ratio;
            }
            return v;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(astronautTexture, position, animatie.CurrentFrame.SourceRectangle, Color.White, 0, new Vector2(0,0), 0.5f, SpriteEffects.None,0);
        }
    }
}
