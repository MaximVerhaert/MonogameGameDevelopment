using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Code.Animation;
using Code.Input;
using Code.Interfaces;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;

namespace Code
{
    public class Astronaut : IGameObject
    {
        private Texture2D idleTexture;
        private Texture2D runningTexture;

        private Texture2D currentTexture;
        private Animatie animatie;
        private Vector2 position;
        private IMovementController movementController;
        private IInputReader inputReader;
        private bool isFacingLeft = false;


        public Astronaut(Texture2D idleTexture, Texture2D runningTexture, IInputReader reader, IMovementController movementController)
        {
            this.idleTexture = idleTexture;
            this.runningTexture = runningTexture;

            animatie = new Animatie();

            animatie.AddAnimation("Idle", new List<AnimationFrame>
            {
                new AnimationFrame(new Rectangle(0, 0, 64, 64)),
                new AnimationFrame(new Rectangle(64, 0, 64, 64)),
                new AnimationFrame(new Rectangle(128, 0, 64, 64)),
                new AnimationFrame(new Rectangle(192, 0, 64, 64)),
                new AnimationFrame(new Rectangle(256, 0, 64, 64)),
                new AnimationFrame(new Rectangle(320, 0, 64, 64)),
                new AnimationFrame(new Rectangle(384, 0, 64, 64)),
                new AnimationFrame(new Rectangle(448, 0, 64, 64)),
                new AnimationFrame(new Rectangle(512, 0, 64, 64))
            });

            animatie.AddAnimation("Running", new List<AnimationFrame>
            {
                new AnimationFrame(new Rectangle(0, 0, 64, 64)),
                new AnimationFrame(new Rectangle(64, 0, 64, 64)),
                new AnimationFrame(new Rectangle(128, 0, 64, 64)),
                new AnimationFrame(new Rectangle(192, 0, 64, 64)),
                new AnimationFrame(new Rectangle(256, 0, 64, 64)),
                new AnimationFrame(new Rectangle(320, 0, 64, 64)),
                new AnimationFrame(new Rectangle(384, 0, 64, 64)),
                new AnimationFrame(new Rectangle(448, 0, 64, 64)),
                new AnimationFrame(new Rectangle(512, 0, 64, 64)),
                new AnimationFrame(new Rectangle(576, 0, 64, 64)),
                new AnimationFrame(new Rectangle(640, 0, 64, 64)),
                new AnimationFrame(new Rectangle(704, 0, 64, 64))
            });

            animatie.Play("Idle");
            currentTexture = idleTexture;

            position = new Vector2(0, 32);
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

            if (direction == Vector2.Zero)
            {
                SetAnimationState("Idle", idleTexture);
            }
            else
            {
                SetAnimationState("Running", runningTexture);

                // kijk richting aanpassen gebaseerd op beweging
                isFacingLeft = direction.X < 0;
            }
        }

        private void SetAnimationState(string animationName, Texture2D texture)
        {
            animatie.Play(animationName);
            currentTexture = texture;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            //verticaal sprite sheet flippen om richting aan te geven
            SpriteEffects spriteEffect = isFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;


            _spriteBatch.Draw(
                currentTexture,
                position,
                animatie.CurrentFrame.SourceRectangle,
                Color.White,
                0,
                new Vector2(0, 0),
                0.5f,
                spriteEffect,
                0
            );
        }

    }
}
