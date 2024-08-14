using Code.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Code.Animation;
using Microsoft.Xna.Framework.Input;
using Code.Input;



namespace Code
{
    public class Astronaut:IGameObject
    {
        Texture2D astronautTexture;
        private Animatie animatie;
        private Vector2 position;
        private IMovementController movementController;
        private IInputReader inputReader;

        public Astronaut(Texture2D texture, IInputReader reader, IMovementController movementController){
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
            this.movementController = movementController;
            this.inputReader = reader;
        }

        public void Update(GameTime gameTime)
        {
            Move(gameTime);
            animatie.Update(gameTime);
        }

        private void Move(GameTime gameTime)
        {
            var direction = inputReader.ReadInput();
            var movement = movementController.UpdateMovement(direction, gameTime);
            position += movement;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(astronautTexture, position, animatie.CurrentFrame.SourceRectangle, Color.White, 0, new Vector2(0,0), 0.5f, SpriteEffects.None,0);
        }
    }
}
