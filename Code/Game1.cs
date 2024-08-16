using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Code;
using Code.Input;
using Code.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Astronaut astronaut;

        private List<TileMap> layers;
        private Dictionary<string, Texture2D> textures;
        private RenderTarget2D _mapRenderTarget;

        private Color _backgroundColor = Color.CornflowerBlue;
        private bool _isCollidingWithFloor = false;

        private string _currentLevel;
        private Dictionary<string, List<TileMap>> _levels;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load textures
            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

            // Initialize game objects
            InitializeGameObjects(idleTexture, runningTexture);

            textures = new Dictionary<string, Texture2D>();

            // Load all levels
            _levels = TileMap.LoadLevelsFromCsv("../../../Data/map.csv");

            // Load and cache textures for the first level (default level)
            SetCurrentLevel("lvl1");
        }

        private void SetCurrentLevel(string levelName)
        {
            if (_levels.ContainsKey(levelName))
            {
                _currentLevel = levelName;
                layers = _levels[_currentLevel];

                // Load and cache textures for the new level
                foreach (var layer in layers)
                {
                    if (!textures.ContainsKey(layer.TextureName))
                    {
                        textures[layer.TextureName] = Content.Load<Texture2D>(layer.TextureName);
                    }

                    var texture = textures[layer.TextureName];
                    layer.TextureStore = TileMap.GenerateTextureStore(texture, 64); // Adjust as needed
                }

                // Render the map for the new level
                var viewport = GraphicsDevice.Viewport;
                _mapRenderTarget = new RenderTarget2D(GraphicsDevice, viewport.Width, viewport.Height);

                GraphicsDevice.SetRenderTarget(_mapRenderTarget);
                GraphicsDevice.Clear(Color.Transparent);

                _spriteBatch.Begin();
                RenderStaticLayers();
                _spriteBatch.End();

                GraphicsDevice.SetRenderTarget(null); // Reset to default
            }
            else
            {
                Console.WriteLine($"Level '{levelName}' not found.");
            }
        }

        private void SwitchLevel(string levelName)
        {
            SetCurrentLevel(levelName);
        }



        private void InitializeGameObjects(Texture2D idleTexture, Texture2D runningTexture)
        {
            // Initialize movement controller with appropriate values
            IMovementController movementController = new MovementController(
                initialSpeed: new Vector2(1, 1),
                initialAcceleration: new Vector2(1f, 1f),
                maxAcceleration: 5f
            );

            // Pass the layers parameter to the Astronaut constructor
            astronaut = new Astronaut(idleTexture, runningTexture, new KeyBoardReader(), movementController, layers);
        }


        private void RenderStaticLayers()
        {
            var viewport = GraphicsDevice.Viewport;
            var visibleArea = new Rectangle(0, 0, viewport.Width, viewport.Height);

            foreach (var layer in layers.OrderBy(l => l.ZIndex))
            {
                var texture = textures[layer.TextureName];

                foreach (var item in layer.TileMapData)
                {
                    int tileIndex = item.Value.TileIndex - 1;  // Access TileIndex and subtract 1
                    int rotation = item.Value.Rotation;        // Access Rotation


                    if (tileIndex < 0 || tileIndex >= layer.TextureStore.Count)
                    {
                        continue;
                    }

                    Rectangle destination = new Rectangle((int)item.Key.X * 64, (int)item.Key.Y * 64, 64, 64);

                    if (destination.Intersects(visibleArea))
                    {
                        Rectangle source = layer.TextureStore[tileIndex];

                        // Convert rotation to radians
                        float rotationRadians = MathHelper.ToRadians(rotation);

                        // Center point for rotation (middle of the tile)
                        Vector2 origin = new Vector2(source.Width / 2f, source.Height / 2f);

                        // Calculate the adjusted destination rectangle
                        Rectangle adjustedDestination = new Rectangle(
                            destination.X + (int)origin.X,
                            destination.Y + (int)origin.Y,
                            destination.Width,
                            destination.Height
                        );

                        // Draw the tile with rotation
                        _spriteBatch.Draw(
                            texture,
                            destinationRectangle: adjustedDestination,
                            sourceRectangle: source,
                            color: Color.White,
                            rotation: rotationRadians,
                            origin: origin,
                            effects: SpriteEffects.None,
                            layerDepth: 0f
                        );
                    }

                }
            }

            astronaut.Floorlayers = layers;
        }



        private void CheckCollisionWithFloorLayer()
        {
            var astronautHitbox = astronaut.Hitbox;

            _isCollidingWithFloor = false;

            foreach (var layer in layers.Where(l => l.ZIndex == 3)) // Assuming the floor layer has ZIndex = 3
            {
                foreach (var item in layer.TileMapData)
                {
                    int tileIndex = item.Value.TileIndex - 1;  // Access TileIndex and subtract 1

                    if (tileIndex < 0 || tileIndex >= layer.TextureStore.Count)
                        continue;

                    Rectangle tileBounds = new Rectangle((int)item.Key.X * 64, (int)item.Key.Y * 64, 64, 64);
                    if (astronautHitbox.Intersects(tileBounds))
                    {
                        _isCollidingWithFloor = true;
                        break;
                    }
                }

                if (_isCollidingWithFloor)
                    break;
            }

            _backgroundColor = _isCollidingWithFloor ? Color.Red : Color.Green;
        }





        private void DrawRectangleBorder(Rectangle rectangle, Color color, int borderThickness)
        {
            Texture2D pixel = CreateSinglePixelTexture(color);

            // Draw the top border
            _spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, borderThickness), color);

            // Draw the bottom border
            _spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - borderThickness, rectangle.Width, borderThickness), color);

            // Draw the left border
            _spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, borderThickness, rectangle.Height), color);

            // Draw the right border
            _spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - borderThickness, rectangle.Y, borderThickness, rectangle.Height), color);
        }

        private Texture2D CreateSinglePixelTexture(Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }







        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                SwitchLevel("lvl2"); // Example of switching to level 2 when 'L' key is pressed
            }

            astronaut.Update(gameTime);

            CheckCollisionWithFloorLayer();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);

            _spriteBatch.Begin();

            // Draw cached map layers
            _spriteBatch.Draw(_mapRenderTarget, Vector2.Zero, Color.White);

            // Draw the astronaut
            astronaut.Draw(_spriteBatch);

            // Draw the hitbox border
            DrawRectangleBorder(astronaut.Hitbox, Color.Red, 2); // Adjust thickness if needed

            _spriteBatch.End();

            base.Draw(gameTime);
        }



    }
}
