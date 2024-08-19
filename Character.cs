using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Code.Animation;
using Code.Input;
using Code.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Code;
using System;

namespace Code
{
    public abstract class Character : IGameObject
    {
        public Vector2 Position => position;
        protected Rectangle idleHitbox;
        protected Rectangle runningHitbox;
        protected Texture2D idleTexture;
        protected Texture2D runningTexture;
        protected Texture2D currentTexture;
        protected Animatie animatie;
        protected Vector2 position;
        protected Vector2 velocity;
        protected bool isFacingLeft = false;
        protected bool isGrounded = false;
        protected float gravity;
        protected ICollisionDetector _collisionDetector;
        protected List<TileMap> layers;

        protected Character(Texture2D idleTexture, Texture2D runningTexture, Vector2 startingPosition, List<TileMap> layers, ICollisionDetector collisionDetector)
        {
            this.idleTexture = idleTexture;
            this.runningTexture = runningTexture;
            this.position = startingPosition;
            this.layers = layers ?? new List<TileMap>();
            _collisionDetector = collisionDetector;

            animatie = new Animatie();
            animatie.AddAnimation("Idle", CreateIdleAnimationFrames());
            animatie.AddAnimation("Running", CreateRunningAnimationFrames());
            animatie.Play("Idle");
            currentTexture = idleTexture;
        }

        protected abstract List<AnimationFrame> CreateIdleAnimationFrames();
        protected abstract List<AnimationFrame> CreateRunningAnimationFrames();



        public virtual void Update(GameTime gameTime)
        {
            animatie.Update(gameTime);
            CheckCollisionWithFloorLayer();
            CheckCollisionWithCeilingLayer();
            CheckCollisionWithLeftSideLayer();
            CheckCollisionWithRightSideLayer();
        }



        protected void CheckCollisionWithFloorLayer()
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
                    isFacingLeft = true;
                    SetAnimationState("Idle", idleTexture);
                }
            }

            if (!isGrounded && wasGrounded)
            {
                isGrounded = false;
                velocity.Y = gravity;
            }
        }

        protected void CheckCollisionWithCeilingLayer()
        {
            Rectangle topHitbox = new Rectangle(Hitbox.X, Hitbox.Top, Hitbox.Width, 1);
            var collisionResult = _collisionDetector.CheckCollision(topHitbox, layers, 3);

            if (collisionResult.isColliding)
            {
                Rectangle tileBounds = collisionResult.tileBounds;
                isFacingLeft = false;

                if (velocity.Y < 0)
                {
                    velocity.Y = 0;
                    position.Y = tileBounds.Bottom;
                    SetAnimationState("Running", runningTexture);
                }
            }
        }

        protected void CheckCollisionWithLeftSideLayer()
        {
            Rectangle leftHitbox = new Rectangle(Hitbox.Left - 1, Hitbox.Top, 1, Hitbox.Height);
            var collisionResult = _collisionDetector.CheckCollision(leftHitbox, layers, 3);

            if (collisionResult.isColliding)
            {
                // Adjust position to be inside the wall
                position.X = collisionResult.tileBounds.Right;
                velocity.X = 0; // Stop horizontal movement
            }
        }

        protected void CheckCollisionWithRightSideLayer()
        {
            Rectangle rightHitbox = new Rectangle(Hitbox.Right, Hitbox.Top, 1, Hitbox.Height);
            var collisionResult = _collisionDetector.CheckCollision(rightHitbox, layers, 3);

            if (collisionResult.isColliding)
            {
                // Adjust position to be inside the wall
                position.X = collisionResult.tileBounds.Left - Hitbox.Width;
                velocity.X = 0; // Stop horizontal movement
            }
        }


        protected void SetAnimationState(string animationName, Texture2D texture)
        {
            animatie.Play(animationName);
            currentTexture = texture;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }
        public virtual Rectangle Hitbox
        {
            get
            {
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
    }

}
