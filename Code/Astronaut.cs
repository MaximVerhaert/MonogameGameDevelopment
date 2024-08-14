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
        }

        public void Update()
        {
            animatie.Update();
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(astronautTexture, new Vector2(10, 10), animatie.CurrentFrame.SourceRectangle, Color.White);
        }
    }
}
