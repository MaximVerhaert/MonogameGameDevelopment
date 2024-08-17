using Code.Input;
using Code.Interfaces;
using Code.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _collisionDetector = new CollisionDetector(); // Initialize the collision detector

            // Initialize LevelManager
            _levelManager = new LevelManager(GraphicsDevice, _spriteBatch, Content);

            // Load all levels and set current level
            _levelManager.LoadLevels("../../../Data/map.csv");
            _levelManager.SetCurrentLevel("lvl1");

            // Load textures for astronaut
            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

            // Initialize game objects after setting the current level
            InitializeGameObjects(idleTexture, runningTexture);
        }

        private void InitializeGameObjects(Texture2D idleTexture, Texture2D runningTexture)
        {
            // Ensure Layers is initialized before passing to Astronaut
            if (_levelManager.Layers == null || !_levelManager.Layers.Any())
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
                _levelManager.SetCurrentLevel("lvl2"); ; // Example of switching to level 2 when 'L' key is pressed
            }

            astronaut.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_levelManager.MapRenderTarget, Vector2.Zero, Color.White);
            astronaut.Draw(_spriteBatch);
            DrawingHelper.DrawRectangleBorder(_spriteBatch, astronaut.Hitbox, Color.Red, 2, GraphicsDevice);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

