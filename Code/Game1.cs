using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code.Map;
using Code.Input;
using Code.Interfaces;
using System;

namespace Code
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Astronaut astronaut;
        private Color _backgroundColor = Color.CornflowerBlue;
        private ILevelManager _levelManager;
        private ICollisionDetector _collisionDetector;
        private Camera _camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _collisionDetector = new CollisionDetector();

            // Initialize LevelManager
            _levelManager = new LevelManager(GraphicsDevice, _spriteBatch, Content);

            _camera = new Camera(GraphicsDevice.Viewport);


            // Load all levels and set current level
            _levelManager.LoadLevels("../../../Data/map.csv");
            _levelManager.SetCurrentLevel("lvl1");

            // Load textures for astronaut
            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

            // Initialize game objects
            InitializeGameObjects(idleTexture, runningTexture);
        }

        private void InitializeGameObjects(Texture2D idleTexture, Texture2D runningTexture)
        {
            // Ensure Layers is initialized before passing to Astronaut
            if (_levelManager.Layers == null)
            {
                throw new InvalidOperationException("Layers must be initialized before creating Astronaut.");
            }

            IMovementController movementController = new MovementController(
                initialSpeed: new Vector2(1, 1),
                initialAcceleration: new Vector2(1f, 1f),
                maxAcceleration: 5f
            );

            astronaut = new Astronaut(idleTexture, runningTexture, new KeyBoardReader(), movementController, _levelManager.Layers, _collisionDetector);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                _levelManager.SetCurrentLevel("lvl2"); // Example of switching to level 2 when 'L' key is pressed
            }

            astronaut.Update(gameTime);

            _camera.Update(astronaut.Position);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);

            // Begin SpriteBatch with the camera transform
            _spriteBatch.Begin(transformMatrix: _camera.Transform);

            // Draw the map
            _spriteBatch.Draw(_levelManager.MapRenderTarget, Vector2.Zero, Color.White);

            // Draw the astronaut
            astronaut.Draw(_spriteBatch);

            // Draw the hitbox (if needed)
            DrawingHelper.DrawRectangleBorder(_spriteBatch, astronaut.Hitbox, Color.Red, 2, GraphicsDevice);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
