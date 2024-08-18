using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Code.Animation;
using Code.Input;
using Code.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Code.Code;
using System;

namespace Code
{
    public class Enemy : IGameObject
    {
        public Vector2 Position => position;
        private Rectangle idleHitbox = new Rectangle(4, 0, 24, 32);
        private Rectangle runningHitbox = new Rectangle(6, 0, 24, 32);

        public Rectangle Hitbox
        {
            get
            {
                // Determine the base hitbox and offset based on the current animation
                Rectangle baseHitbox;
                int xOffset;

                if (animatie.CurrentAnimationName == "Running")
                {
                    baseHitbox = runningHitbox;
                    xOffset = isFacingLeft ? -baseHitbox.Width + 22 : 0;
                }
                else
                {
                    baseHitbox = idleHitbox;
                    xOffset = isFacingLeft ? -baseHitbox.Width + 26 : 0;
                }

                return new Rectangle(
                    (int)position.X + xOffset + baseHitbox.X,
                    (int)position.Y + baseHitbox.Y,
                    baseHitbox.Width,
                    baseHitbox.Height
                );
            }
        }

        private Texture2D idleTexture;
        private Texture2D runningTexture;
        private Texture2D currentTexture;
        private Animatie animatie;
        private Vector2 position;
        private Vector2 velocity;
        private float gravity = 9.8f;
        private bool isFacingLeft = false;
        private bool isGrounded = false;
        private List<TileMap> layers;
        private ICollisionDetector _collisionDetector;

        public Enemy(Texture2D idleTexture, Texture2D runningTexture, Vector2 startingPosition, List<TileMap> layers, ICollisionDetector collisionDetector)
        {
            this.idleTexture = idleTexture;
            this.runningTexture = runningTexture;
            this.position = startingPosition;

            animatie = new Animatie();
            animatie.AddAnimation("Idle", CreateIdleAnimationFrames());
            animatie.AddAnimation("Running", CreateRunningAnimationFrames());
            animatie.Play("Idle");
            currentTexture = idleTexture;

            this.layers = layers ?? new List<TileMap>();
            _collisionDetector = collisionDetector;
        }

        private List<AnimationFrame> CreateIdleAnimationFrames()
        {
            return new List<AnimationFrame>
            {
                new AnimationFrame(new Rectangle(0, 0, 64, 64)),
                new AnimationFrame(new Rectangle(64, 0, 64, 64)),
                // Add more frames as needed
            };
        }

        private List<AnimationFrame> CreateRunningAnimationFrames()
        {
            return new List<AnimationFrame>
            {
                new AnimationFrame(new Rectangle(0, 0, 64, 64)),
                new AnimationFrame(new Rectangle(64, 0, 64, 64)),
                new AnimationFrame(new Rectangle(128, 0, 64, 64)),
                new AnimationFrame(new Rectangle(172, 0, 64, 64)),

                // Add more frames as needed
            };
        }

        public void Update(GameTime gameTime)
        {
            Move(gameTime);
            animatie.Update(gameTime);
            CheckCollisionWithFloorLayer(layers);
            CheckCollisionWithCeilingLayer(layers);
        }

        private void Move(GameTime gameTime)
        {
            // Simple enemy movement logic (e.g., move left and right)
            // Replace this with more complex AI or movement patterns as needed
            //velocity.X = isFacingLeft ? -1f : 1f;

            //if (position.X < 0 || position.X > 800) // Example screen bounds
            //{
            //    isFacingLeft = !isFacingLeft;
            //}

            position.X += velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Apply gravity
            if (!isGrounded)
            {
                velocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            position.Y += velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update animation state
            if (velocity.X == 0 && isGrounded)
            {
                SetAnimationState("Idle", idleTexture);
            }
            else if (isGrounded)
            {
                SetAnimationState("Running", runningTexture);
            }
        }

        private void CheckCollisionWithFloorLayer(List<TileMap> layers)
        {
            Rectangle bottomHitbox = new Rectangle(Hitbox.X, Hitbox.Bottom - 1, Hitbox.Width, 1);
            bool wasGrounded = isGrounded;
            isGrounded = false;

            var collisionResult = _collisionDetector.CheckCollision(bottomHitbox, layers, 3);
            if (collisionResult.isColliding)
            {
                Rectangle tileBounds = collisionResult.tileBounds;
                isGrounded = true;

                if (!wasGrounded)
                {
                    velocity.Y = 0;
                    position.Y = tileBounds.Top - Hitbox.Height;
                }
            }

            if (!isGrounded && wasGrounded)
            {
                isGrounded = false;
                velocity.Y = gravity;
            }
        }

        private void CheckCollisionWithCeilingLayer(List<TileMap> layers)
        {
            Rectangle topHitbox = new Rectangle(Hitbox.X, Hitbox.Top, Hitbox.Width, 1);
            var collisionResult = _collisionDetector.CheckCollision(topHitbox, layers, 3);

            if (collisionResult.isColliding)
            {
                Rectangle tileBounds = collisionResult.tileBounds;

                if (velocity.Y < 0)
                {
                    velocity.Y = 0;
                    position.Y = tileBounds.Bottom;
                }
            }
        }

        private void SetAnimationState(string animationName, Texture2D texture)
        {
            animatie.Play(animationName);
            currentTexture = texture;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects spriteEffect = isFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(
                currentTexture,
                position,
                animatie.CurrentFrame.SourceRectangle,
                Color.White,
                0,
                Vector2.Zero,
                0.5f,
                spriteEffect,
                0
            );
        }
    }
}
