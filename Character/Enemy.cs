using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Code.Animation;
using Code.Input;
using Code.Interfaces;
using Code.State;
using Code.Strategy;
using System.Collections.Generic;
using System.Linq;
using Code;
using System;

namespace Code
{
    public class Enemy : Character
    {
        public int Level { get; private set; }
        private IEnemyState currentState;
        private IMovementStrategy movementStrategy;

        private float jumpStrength = 60f;
        private float jumpInterval = 0.5f;
        private float jumpTimer = 0f;

        private float directionChangeTimer = 0f;
        private float directionChangeRange = 2f; // Range for direction change interval
        private float directionChangeInterval;
        private Random random = new Random();

        public Enemy(Texture2D idleTexture, Texture2D runningTexture, Vector2 startingPosition, List<TileMap> layers, ICollisionDetector collisionDetector, int level)
            : base(idleTexture, runningTexture, startingPosition, layers, collisionDetector)
        {
            Level = level;
            SetState(level);
            SetMovementStrategy(level);

            // Initialize directionChangeInterval with a random value within the specified range
            directionChangeInterval = GetRandomDirectionChangeInterval();
        }

        private void SetState(int level)
        {
            switch (level)
            {
                case 1:
                    currentState = new Level1State();
                    break;
                case 2:
                    currentState = new Level2State();
                    break;
                case 3:
                    currentState = new Level3State();
                    break;
            }
        }

        private void SetMovementStrategy(int level)
        {
            switch (level)
            {
                case 1:
                    movementStrategy = new RunningMovement();
                    break;
                case 2:
                    movementStrategy = new RunningMovement();
                    break;
                case 3:
                    movementStrategy = new JumpingMovement();
                    break;
            }
        }

        protected override List<AnimationFrame> CreateIdleAnimationFrames()
        {
            return new List<AnimationFrame>
        {
            new AnimationFrame(new Rectangle(0, 0, 64, 64)),
            new AnimationFrame(new Rectangle(64, 0, 64, 64)),
        };
        }

        protected override List<AnimationFrame> CreateRunningAnimationFrames()
        {
            return new List<AnimationFrame>
        {
            new AnimationFrame(new Rectangle(0, 0, 64, 64)),
            new AnimationFrame(new Rectangle(64, 0, 64, 64)),
            new AnimationFrame(new Rectangle(128, 0, 64, 64)),
            new AnimationFrame(new Rectangle(192, 0, 64, 64)),
        };
        }

        public override void Update(GameTime gameTime)
        {
            currentState.Update(this, gameTime);
            // Add collision detection for damage here
        }

        public void UpdateLevel1Behavior(GameTime gameTime)
        {
            directionChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (directionChangeTimer >= directionChangeInterval)
            {
                isFacingLeft = !isFacingLeft;
                directionChangeTimer = 0f;
            }

            // Idle behavior
            if (velocity.X == 0 && isGrounded)
            {
                SetAnimationState("Idle", idleTexture);
            }
            else if (isGrounded)
            {
                SetAnimationState("Running", runningTexture);
            }
        }

        public void UpdateLevel2Behavior(GameTime gameTime)
        {
            directionChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (directionChangeTimer >= directionChangeInterval)
            {
                isFacingLeft = !isFacingLeft;
                directionChangeTimer = 0f;
                // Set new random interval between 1 and 3 seconds
                directionChangeInterval = GetRandomDirectionChangeInterval();
            }

            // Move left or right based on direction
            velocity.X = isFacingLeft ? -10 : 10;

            // Apply gravity
            if (!isGrounded)
            {
                velocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            position.X += velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
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

        public void UpdateLevel3Behavior(GameTime gameTime)
        {
            directionChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (directionChangeTimer >= directionChangeInterval)
            {
                isFacingLeft = !isFacingLeft;
                directionChangeTimer = 0f;
                directionChangeInterval = GetRandomDirectionChangeInterval();
            }

            // Move left or right based on direction
            velocity.X = isFacingLeft ? -10 : 10;

            // Apply gravity
            if (!isGrounded)
            {
                velocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            position.X += velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
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

        private float GetRandomDirectionChangeInterval()
        {
            // Generate a random float between 1 and 3
            return (float)(random.NextDouble() * 4 + 1);
        }

        private void Jump(GameTime gameTime)
        {
            if (isGrounded)
            {
                velocity.Y = -jumpStrength;
                isGrounded = false;
                // No need to change facing direction or animation here
            }
        }

        // Implement the abstract Draw method
        public override void Draw(SpriteBatch spriteBatch)
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
