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
        private bool isFacingLeft = false;
        private bool isGrounded = false;
        private List<TileMap> layers;
        private ICollisionDetector _collisionDetector;
        public int Level { get; private set; }

        private float jumpStrength = 60f;
        private float jumpInterval = 0.5f; // Time in seconds between jumps
        private float jumpTimer = 0f;
        private float gravity = 9.81f; // Increase this value




        public Enemy(Texture2D idleTexture, Texture2D runningTexture, Vector2 startingPosition, List<TileMap> layers, ICollisionDetector collisionDetector, int level)
        {
            Level = level;
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
                new AnimationFrame(new Rectangle(192, 0, 64, 64)),

                // Add more frames as needed
            };
        }

        public void Update(GameTime gameTime)
        {
            jumpTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (jumpTimer >= jumpInterval)
            {
                Jump(gameTime);
                jumpTimer = 0f;
            }

            Move(gameTime);
            animatie.Update(gameTime);
            CheckCollisionWithFloorLayer(layers);
            CheckCollisionWithCeilingLayer(layers);
        }

        private void Move(GameTime gameTime)
        {
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
        private void Jump(GameTime gameTime)
        {
            if (isGrounded)
            {
                // Start the jump
                velocity.Y = -jumpStrength;
                isGrounded = false;

                // Face left when jumping
                isFacingLeft = true;

                // Set the animation to running when jumping
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
                    // Reset velocity and position when landing
                    velocity.Y = 0;
                    position.Y = tileBounds.Top - Hitbox.Height;

                    // Face right when landing
                    isFacingLeft = true;

                    // Set the animation to idle when grounded
                    SetAnimationState("Idle", idleTexture);
                }
            }

            if (!isGrounded && wasGrounded)
            {
                // While in the air, apply gravity
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
                isFacingLeft = false;


                if (velocity.Y < 0)
                {
                    // When hitting the ceiling, reset vertical velocity and position
                    velocity.Y = 0;
                    position.Y = tileBounds.Bottom;

                    // Set the animation to running when hitting the ceiling
                    SetAnimationState("Running", runningTexture);
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
