using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code.Interfaces;
using Code.Map;
using Code.Input;
using System;
using System.Collections.Generic;
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
        private Camera _camera;
        private MainMenu _mainMenu;
        private GameState _currentState;
        private string _lastPlayedLevel = "lvl1";
        private double _lastClickTime = 0;
        private const double ClickCooldown = 200; // milliseconds

        // Use LevelLocationManager to fetch level starting positions
        private ILevelLocationManager _levelLocationManager;
        private Dictionary<string, Vector2> levelStartingPositions;

        private bool _hasCompletedLevel; // Flag to indicate level completion


        public enum GameState
        {
            Menu,
            Playing
        }

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
            _levelManager = new LevelManager(GraphicsDevice, _spriteBatch, Content);
            _camera = new Camera(GraphicsDevice.Viewport);

            // Instantiate and use LevelLocationManager to read starting positions
            _levelLocationManager = new LevelLocationManager();
            levelStartingPositions = _levelLocationManager.ReadStartingPositions("../../../Data/map.csv");

            _levelManager.LoadLevels("../../../Data/map.csv");
            _levelManager.SetCurrentLevel(_lastPlayedLevel);

            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

            InitializeGameObjects(idleTexture, runningTexture);

            SpriteFont font = Content.Load<SpriteFont>("Fonts/SpaceFont");
            _mainMenu = new MainMenu(GraphicsDevice, font);
            _mainMenu.PlayRequested += OnPlayRequested;
            _mainMenu.ExitRequested += OnExitRequested;

            _currentState = GameState.Menu;
            _hasCompletedLevel = false; // Initialize the flag

        }

        private void InitializeGameObjects(Texture2D idleTexture, Texture2D runningTexture)
        {
            if (_levelManager.Layers == null)
            {
                throw new InvalidOperationException("Layers must be initialized before creating Astronaut.");
            }

            Vector2 startingPosition = new Vector2(128, 256); // Default position if not found
            bool positionFound = false;

            // Iterate through the dictionary to find a match
            foreach (var kvp in levelStartingPositions)
            {
                // Remove the colon from the key for comparison
                string keyWithoutColon = kvp.Key.TrimEnd(':');

                // Check if the current key (without colon) matches _lastPlayedLevel
                if (keyWithoutColon.Equals(_lastPlayedLevel, StringComparison.OrdinalIgnoreCase))
                {
                    startingPosition = kvp.Value; // Update startingPosition
                    positionFound = true;
                    break; // Exit the loop once a match is found
                }
            }

            if (!positionFound)
            {
                Console.WriteLine($"Starting position for level '{_lastPlayedLevel}' not found. Using default position.");
            }
            else
            {
                Console.WriteLine($"Starting position for level '{_lastPlayedLevel}' set to {startingPosition}.");
            }

            IMovementController movementController = new MovementController(
                initialSpeed: new Vector2(1, 1),
                initialAcceleration: new Vector2(1f, 1f),
                maxAcceleration: 5f
            );

            // Pass the starting position to the Astronaut constructor
            astronaut = new Astronaut(idleTexture, runningTexture, new KeyBoardReader(), movementController, _levelManager.Layers, _collisionDetector, startingPosition);
        }



        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            switch (_currentState)
            {
                case GameState.Menu:
                    _mainMenu.Update(mouseState, gameTime);
                    break;
                case GameState.Playing:
                    UpdateGame(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdateGame(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                _currentState = GameState.Menu;
                return; // Exit early if returning to the menu
            }

            astronaut.Update(gameTime);

            // Check for collision with DeployableFinish layer
            Rectangle astronautHitbox = astronaut.Hitbox; // Assuming Astronaut has a Hitbox property
            var (isColliding, tileBounds) = _collisionDetector.CheckCollision(astronautHitbox, _levelManager.Layers, 6);

            if (isColliding)
            {
                Console.WriteLine("Collision detected with DeployableFinish. Transitioning to next level.");

                // Determine the next level
                string nextLevel = GetNextLevel(_lastPlayedLevel);
                if(nextLevel == "lvl1")
                {
                    _mainMenu.ShowVictoryMessage("Congratulations! You Have beaten the game!");
                    _currentState = GameState.Menu;
                }
                _lastPlayedLevel = nextLevel;
                _levelManager.SetCurrentLevel(nextLevel);

                // Reload textures if necessary
                Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
                Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

                // Initialize game objects to reset position
                InitializeGameObjects(idleTexture, runningTexture);

                return; // Exit early to process the new level setup
            }

            _camera.Update(astronaut.Position);
        }





        private string GetNextLevel(string currentLevel)
        {
            // Extract all levels from the dictionary and remove any colons
            var levels = levelStartingPositions.Keys
                .Select(key => key.TrimEnd(':'))
                .ToList();

            // Find the index of the current level
            int index = levels.IndexOf(currentLevel);

            // Check if the index is valid and return the next level, or loop back to the first level
            if (index >= 0 && index < levels.Count - 1)
            {
                return levels[index + 1];
            }
            return levels.First(); // Default to the first level if no next level is found
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);

            switch (_currentState)
            {
                case GameState.Menu:
                    _mainMenu.Draw(_spriteBatch);
                    break;
                case GameState.Playing:
                    _spriteBatch.Begin(transformMatrix: _camera.Transform);
                    _spriteBatch.Draw(_levelManager.MapRenderTarget, Vector2.Zero, Color.White);
                    astronaut.Draw(_spriteBatch);
                    DrawingHelper.DrawRectangleBorder(_spriteBatch, astronaut.Hitbox, Color.Red, 2, GraphicsDevice);
                    _spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        private void OnPlayRequested(object sender, string levelName)
        {
            if (levelName == "last")
            {
                levelName = _lastPlayedLevel;
            }

            _lastPlayedLevel = levelName;
            _levelManager.SetCurrentLevel(levelName);

            // Reload textures if necessary, or simply reset position
            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

            InitializeGameObjects(idleTexture, runningTexture);

            _currentState = GameState.Playing;
        }

        private void OnExitRequested(object sender, EventArgs e)
        {
            Exit();
        }
    }
}
