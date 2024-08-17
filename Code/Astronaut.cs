﻿using Microsoft.Xna.Framework;
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
    public class Astronaut : IGameObject
    {
        public Vector2 Position => position;
        private Rectangle idleHitbox = new Rectangle(4, 0, 21, 32);
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
                    // Define offsets for running animation
                    xOffset = isFacingLeft ? -baseHitbox.Width + 22 : 0;
                }
                else
                {
                    baseHitbox = idleHitbox;
                    // Define offsets for idle animation
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
        private IMovementController movementController;
        private IInputReader inputReader;
        private bool isFacingLeft = false;
        private Vector2 velocity;
        private float gravity = 9.8f;
        private float jumpStrength = 300f;
        private bool isGrounded = false;
        private bool isJumping = false;
        private float jumpTimer = 0f;
        private float maxJumpTime = 0.3f; // Maximum time for the jump

        // List of layers to check for collision
        private List<TileMap> layers;
        public List<TileMap> Floorlayers;

        // List to track the current standing tiles (layer, tile position)
        private List<(TileMap Layer, Point TilePosition)> currentStandingTiles = new List<(TileMap, Point)>();
        private List<(TileMap Layer, Point TilePosition)> previousStandingTiles = new List<(TileMap, Point)>();

        private ICollisionDetector _collisionDetector;


        public Astronaut(Texture2D idleTexture, Texture2D runningTexture, IInputReader reader, IMovementController movementController, List<TileMap> layers, ICollisionDetector collisionDetector)
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

            position = new Vector2(128, 32 + 256 + 0);
            this.movementController = movementController;
            this.inputReader = reader;
            this.layers = layers ?? new List<TileMap>();


            _collisionDetector = collisionDetector;

        }

        public void Update(GameTime gameTime)
        {
            Move(gameTime);
            animatie.Update(gameTime);
            CheckCollisionWithFloorLayer(Floorlayers);
            CheckCollisionWithCeilingLayer(Floorlayers);
        }

        private Point GetStandingTilePosition()
        {
            // Assume each tile is 64x64 pixels, adjust if different
            int tileX = (int)position.X / 64;
            int tileY = (int)(position.Y + Hitbox.Height) / 64; // Bottom of the character
            return new Point(tileX, tileY);
        }


        private void OnTileChange()
        {
            // Placeholder method for any action to be performed on tile change
            // Recalculate gravity when the tile changes
            isGrounded = false;  // Reset grounded state on tile change
            velocity.Y = gravity; // Apply gravity initially

            // You can also handle other tile change logic here
            Console.WriteLine("Tile changed! Gravity recalculated."); ;
        }

        private void Move(GameTime gameTime)
        {
            var direction = inputReader.ReadInput();

            // Horizontal movement
            velocity.X = movementController.UpdateMovement(direction, gameTime).X;

            // If the character moves horizontally, check and update the standing tile
            if (velocity.X != 0)
            {
                var newStandingTile = GetStandingTilePosition();
                var currentLayer = layers.FirstOrDefault(l => l.ZIndex == 3); // Assuming floor layer ZIndex is 3

                // Check if the new standing tile is different from the current one
                if (currentStandingTiles.Count == 0 || currentStandingTiles[0].TilePosition != newStandingTile)
                {
                    // Call the method when the tile changes
                    OnTileChange();

                    // Update the current standing tile
                    currentStandingTiles.Clear(); // Clear previous standing tiles
                    currentStandingTiles.Add((currentLayer, newStandingTile));
                }
            }

            // Apply gravity to vertical velocity only if not grounded
            position.X += velocity.X;

            //Jumping logic
            if (isGrounded && direction.Y < 0 && !isJumping) // Jump on up input
            {
                isJumping = true;
                isGrounded = false;
                velocity.Y = -jumpStrength;
                jumpTimer = 0f;
            }

            // Continue jumping if holding the jump key, but limit jump duration
            if (isJumping)
            {
                jumpTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (jumpTimer > maxJumpTime)
                {
                    isJumping = false;
                }
            }

            // Apply velocity to position
            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update animation state
            if (direction == Vector2.Zero && isGrounded)
            {
                SetAnimationState("Idle", idleTexture);
            }
            else if (isGrounded)
            {
                SetAnimationState("Running", runningTexture);
                isFacingLeft = direction.X < 0;
            }
            else
            {
                velocity.Y += gravity;
            }
        }


        private void CheckCollisionWithFloorLayer(List<TileMap> floorLayers)
        {
            // Define a new rectangle that represents only the bottom of the hitbox
            Rectangle bottomHitbox = new Rectangle(
                Hitbox.X,
                Hitbox.Bottom - 1,  // Just the bottom edge of the hitbox
                Hitbox.Width,
                1  // Height of 1 pixel to check just the bottom edge
            );

            bool wasGrounded = isGrounded; // Keep track of previous grounded state
            isGrounded = false; // Reset grounded status before checking

            // Use CollisionDetector to check for collisions with the bottom hitbox
            var collisionResult = _collisionDetector.CheckCollision(bottomHitbox, floorLayers);

            if (collisionResult.isColliding)
            {
                Rectangle tileBounds = collisionResult.tileBounds;
                isGrounded = true;

                // Adjust position to be on top of the floor
                if (!wasGrounded)
                {
                    velocity.Y = 0; // Stop downward movement
                    position.Y = tileBounds.Top - Hitbox.Height; // Adjust position to be on top of the floor
                }
            }

            // Recalculate gravity only if the astronaut is no longer grounded
            if (!isGrounded && wasGrounded)
            {
                OnTileChange();
            }
        }


        private void CheckCollisionWithCeilingLayer(List<TileMap> ceilingLayers)
        {
            // Define a new rectangle that represents only the top of the hitbox
            Rectangle topHitbox = new Rectangle(
                Hitbox.X,
                Hitbox.Top,  // The top edge of the hitbox
                Hitbox.Width,
                1  // Height of 1 pixel to check just the top edge
            );

            // Use CollisionDetector to check for collisions with the top hitbox
            var collisionResult = _collisionDetector.CheckCollision(topHitbox, ceilingLayers);

            if (collisionResult.isColliding)
            {
                Rectangle tileBounds = collisionResult.tileBounds;

                // Adjust position to be below the ceiling
                if (velocity.Y < 0) // Only if moving upwards
                {
                    velocity.Y = 0; // Stop upward movement
                    position.Y = tileBounds.Bottom; // Set position to be below the ceiling
                }
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
