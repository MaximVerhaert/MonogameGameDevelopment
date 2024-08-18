using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code.Interfaces;
using Code.Map;
using Code.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;


namespace Code
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteBatch _uiBatch; // Add this field to your class

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
        private SoundEffect laserEffect;
        private SoundEffect gameEffect;
        private SoundEffect completeEffect;

        private SoundEffect collectEffect;
        private SoundEffect damageEffect;
        private SoundEffect deathEffect;

        private float soundEffectVolume = 0.25f;

        private int _points = 0;
        private double _lastCollectedTime = 0; // To track time since last collection

        private Texture2D _collectableSpriteSheet;

        private List<Vector2> _collectedCoinPositions = new List<Vector2>(); // Track positions of collected coins


        // Inside LoadContent



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
            _uiBatch = new SpriteBatch(GraphicsDevice); // Initialize the UI batch

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

            SpriteFont font = Content.Load<SpriteFont>(@"Fonts\SpaceFont");
            laserEffect = Content.Load<SoundEffect>(@"Sounds\laser-shot");
            gameEffect = Content.Load<SoundEffect>(@"Sounds\game");
            completeEffect = Content.Load<SoundEffect>(@"Sounds\complete");

            collectEffect = Content.Load<SoundEffect>(@"Sounds\collect");
            damageEffect = Content.Load<SoundEffect>(@"Sounds\damage");
            deathEffect = Content.Load<SoundEffect>(@"Sounds\death");

            _mainMenu = new MainMenu(GraphicsDevice, font, laserEffect);
            _mainMenu.PlayRequested += OnPlayRequested;
            _mainMenu.ExitRequested += OnExitRequested;

            _currentState = GameState.Menu;
            _hasCompletedLevel = false; // Initialize the flag

            _collectableSpriteSheet = Content.Load<Texture2D>("CollectableForegroundGray(64x64)x2");

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

            // Prevent multiple collections within 1 second
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastCollectedTime > 1000)
            {
                var (isCollidingWithItem, itemBounds, coinPos) = _collisionDetector.CheckCollision(astronaut.Hitbox, _levelManager.Layers, 7);

                if (isCollidingWithItem)
                {
                    // Check if the coin has already been collected
                    if (!_collectedCoinPositions.Contains(coinPos))
                    {
                        // Add the coin's position to the list for rendering the sprite
                        _collectedCoinPositions.Add(coinPos);

                        collectEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);
                        _points++; // Increase points
                        _lastCollectedTime = gameTime.TotalGameTime.TotalMilliseconds; // Update last collected time

                        // Optionally remove the coin from the level if required
                        // _levelManager.RemoveTileFromLayer(7, coinPos);
                    }
                }
            }

            // Check for collision with DeployableFinish layer (layer 6)
            var (isCollidingWithFinish, finishTileBounds, portal_pos) = _collisionDetector.CheckCollision(astronaut.Hitbox, _levelManager.Layers, 6);

            if (isCollidingWithFinish)
            {
                Console.WriteLine("Collision detected with DeployableFinish. Transitioning to next level.");

                // Determine the next level
                string nextLevel = GetNextLevel(_lastPlayedLevel);
                if (nextLevel == "lvl1")
                {
                    _mainMenu.ShowVictoryMessage($"Congratulations! You have beaten the game with {_points} points!");
                    _currentState = GameState.Menu;
                    completeEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);
                    _points = 0; // Reset points after the game ends
                }
                _lastPlayedLevel = nextLevel;
                _levelManager.SetCurrentLevel(nextLevel);

                // Clear collected coin positions
                _collectedCoinPositions.Clear();

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
                    _spriteBatch.Begin();
                    _mainMenu.Draw(_spriteBatch); // Ensure MainMenu.Draw does not call Begin again
                    _spriteBatch.End();
                    break;

                case GameState.Playing:
                    // Draw the game world with camera transformation
                    _spriteBatch.Begin(transformMatrix: _camera.Transform);
                    _spriteBatch.Draw(_levelManager.MapRenderTarget, Vector2.Zero, Color.White);
                    astronaut.Draw(_spriteBatch);
                    DrawingHelper.DrawRectangleBorder(_spriteBatch, astronaut.Hitbox, Color.Red, 2, GraphicsDevice);

                    // Draw the collected coin sprites
                    foreach (var coinPos in _collectedCoinPositions)
                    {
                        // Calculate the position of the sprite in the spritesheet (second image)
                        Rectangle sourceRectangle = new Rectangle(64, 0, 64, 64);

                        // Convert tile position to world position (64x64 tile size)
                        Vector2 worldPos = coinPos * 64;

                        // Draw the coin sprite on top of the collected coin
                        _spriteBatch.Draw(_collectableSpriteSheet, worldPos, sourceRectangle, Color.White);
                    }

                    _spriteBatch.End();

                    // Draw the UI elements (e.g., points counter) on top of the game world
                    _uiBatch.Begin(); // Ensure _uiBatch.Begin does not overlap with _spriteBatch.Begin
                    SpriteFont font = Content.Load<SpriteFont>(@"Fonts\SpaceFont");
                    _uiBatch.DrawString(font, $"Points: {_points}", new Vector2(10, 10), Color.White);
                    _uiBatch.End();
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
            gameEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);

            // Clear collected coin positions
            _collectedCoinPositions.Clear();

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
