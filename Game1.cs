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
        private Dictionary<int, (Texture2D IdleTexture, Texture2D RunningTexture)> enemyTextures;


        private Color _backgroundColor = Color.CornflowerBlue;
        private ILevelManager _levelManager;
        private ICollisionDetector _collisionDetector;
        private Camera _camera;
        private MainMenu _mainMenu;
        private GameState _currentState;
        private string _lastPlayedLevel = "lvl1";
        private double _lastClickTime = 0;
        private const double ClickCooldown = 200; // milliseconds
        private double _lastEnemyCollisionTime = 0;
        private const double EnemyCollisionCooldown = 3000; // 3 seconds in milliseconds


        // Use LevelLocationManager to fetch level starting positions
        private ILevelLocationManager _levelLocationManager;
        private Dictionary<string, Vector2> levelStartingPositions;
        private Dictionary<string, List<(int Level, Vector2 Position)>> levelEnemies; // Declare levelEnemies
        private List<Enemy> enemies = new List<Enemy>();


        private bool _hasCompletedLevel; // Flag to indicate level completion
        private SoundEffect laserEffect;
        private SoundEffect gameEffect;
        private SoundEffect completeEffect;

        private SoundEffect collectEffect;
        private SoundEffect damageEffect;
        private SoundEffect deathEffect;

        private float soundEffectVolume = 0.25f;

        private int _points = 0;
        private double _lastCollectedCoinTime = 0; // To track time since last collection

        private int _health = 1;
        private double _lastCollectedHealthTime = 0; // To track time since last collection

        private Texture2D _collectableSpriteSheet;

        private Texture2D _healthSpriteSheet;


        private List<Vector2> _collectedCoinPositions = new List<Vector2>(); // Track positions of collected coins
        private List<Vector2> _collectedHealthPositions = new List<Vector2>(); // Track positions of collected coins



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
            levelEnemies = _levelLocationManager.ReadEnemies("../../../Data/map.csv");

            enemyTextures = new Dictionary<int, (Texture2D, Texture2D)>
{
    { 1, (Content.Load<Texture2D>("Enemy1Idle(64x64)x2"), Content.Load<Texture2D>("Enemy1Running(64x64)x4")) },
    { 2, (Content.Load<Texture2D>("Enemy2Idle(64x64)x2"), Content.Load<Texture2D>("Enemy2Running(64x64)x4")) },
    { 3, (Content.Load<Texture2D>("Enemy3Idle(64x64)x2"), Content.Load<Texture2D>("Enemy3Running(64x64)x4")) }
};



            _levelManager.LoadLevels("../../../Data/map.csv");
            _levelManager.SetCurrentLevel(_lastPlayedLevel);

            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

            // Initialize game objects, but don't load enemies here
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
            _healthSpriteSheet = Content.Load<Texture2D>("Health(64x64)x3");
        }

        private Enemy CreateEnemy(int level, Vector2 position)
        {
            if (enemyTextures == null)
            {
                throw new InvalidOperationException("Enemy textures dictionary is not initialized.");
            }

            if (enemyTextures.TryGetValue(level, out var textures))
            {
                if (textures.IdleTexture == null || textures.RunningTexture == null)
                {
                    throw new InvalidOperationException($"Textures for enemy level {level} are not loaded correctly.");
                }

                Console.WriteLine($"Creating enemy at position: {position}");
                return new Enemy(textures.IdleTexture, textures.RunningTexture, position, _levelManager.Layers, _collisionDetector, level);
            }
            else
            {
                throw new ArgumentException($"No textures found for enemy level {level}");
            }
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

            foreach (var enemy in enemies)
            {
                enemy.Update(gameTime);
            }

            // Check for health drop and transition to end screen if health is 0 or below
            if (_health <= 0)
            {
                _mainMenu.ShowVictoryMessage("You died");
                _currentState = GameState.Menu; // Switch to menu to show end screen
                deathEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);
                _points = 0; // Optionally reset points
                _health = 1; // Optionally reset health
                return; // Exit early to process the end screen
            }

            // Existing coin and health collection logic
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastCollectedCoinTime > 1000)
            {
                var (isCollidingWithItem, itemBounds, coinPos) = _collisionDetector.CheckCollision(astronaut.Hitbox, _levelManager.Layers, 7);

                if (isCollidingWithItem)
                {
                    if (!_collectedCoinPositions.Contains(coinPos))
                    {
                        _collectedCoinPositions.Add(coinPos);
                        collectEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);
                        _points++;
                        _lastCollectedCoinTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
            }

            if (_health <= 2 && gameTime.TotalGameTime.TotalMilliseconds - _lastCollectedHealthTime > 1000)
            {
                var (isCollidingWithItem, itemBounds, healthPos) = _collisionDetector.CheckCollision(astronaut.Hitbox, _levelManager.Layers, 8);

                if (isCollidingWithItem)
                {
                    if (!_collectedHealthPositions.Contains(healthPos))
                    {
                        _collectedHealthPositions.Add(healthPos);
                        collectEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);
                        _health++;
                        _lastCollectedHealthTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
            }

            // Check for enemy collision with cooldown
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastEnemyCollisionTime > EnemyCollisionCooldown)
            {
                foreach (var enemy in enemies)
                {
                    if (_collisionDetector.CheckCollision(astronaut.Hitbox, enemy.Hitbox))
                    {
                        _health -= enemy.Level; // Reduce health based on enemy level
                        _lastEnemyCollisionTime = gameTime.TotalGameTime.TotalMilliseconds;
                        damageEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);
                        break; // Exit the loop after the first collision is detected
                    }
                }
            }

            // Check for collision with DeployableFinish layer (layer 6)
            var (isCollidingWithFinish, finishTileBounds, portal_pos) = _collisionDetector.CheckCollision(astronaut.Hitbox, _levelManager.Layers, 6);

            if (isCollidingWithFinish)
            {
                Console.WriteLine("Collision detected with DeployableFinish. Transitioning to next level.");
                string nextLevel = GetNextLevel(_lastPlayedLevel);
                if (nextLevel == "lvl1")
                {
                    _mainMenu.ShowVictoryMessage($"Congratulations! You have beaten the game with {_points} points!");
                    _currentState = GameState.Menu;
                    completeEffect.Play(volume: soundEffectVolume, pitch: 0f, pan: 0f);
                    _points = 0;
                    _health = 1;
                }
                _lastPlayedLevel = nextLevel;
                _levelManager.SetCurrentLevel(nextLevel);

                _collectedCoinPositions.Clear();
                _collectedHealthPositions.Clear();

                Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
                Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");
                InitializeGameObjects(idleTexture, runningTexture);

                // Clear existing enemies
                enemies.Clear();


                // Load new enemies for the current level
                foreach (var xx in levelEnemies)
                {
                    if (xx.Key.Split(':')[0] == _lastPlayedLevel)
                    {
                        foreach (var enemy in xx.Value)
                        {
                            Console.WriteLine($"Enemy Level: {enemy.Level}, Position: {enemy.Position}");

                            Enemy newEnemy = CreateEnemy(enemy.Level, enemy.Position);
                            if (newEnemy != null)
                            {
                                enemies.Add(newEnemy);
                                Console.WriteLine($"Added enemy of level {enemy.Level} at position {enemy.Position}");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to create enemy of level {enemy.Level} at position {enemy.Position}");
                            }
                        }
                    }
                }
                return;
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
                    _mainMenu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.Playing:
                    _spriteBatch.Begin(transformMatrix: _camera.Transform);
                    _spriteBatch.Draw(_levelManager.MapRenderTarget, Vector2.Zero, Color.White);
                    astronaut.Draw(_spriteBatch);

                    if (enemies != null)
                    {
                        foreach (var enemy in enemies)
                        {
                            enemy.Draw(_spriteBatch);
                            DrawingHelper.DrawRectangleBorder(_spriteBatch, enemy.Hitbox, Color.Red, 2, GraphicsDevice);
                        }
                    }

                    DrawingHelper.DrawRectangleBorder(_spriteBatch, astronaut.Hitbox, Color.Red, 2, GraphicsDevice);

                    foreach (var coinPos in _collectedCoinPositions)
                    {
                        Rectangle sourceRectangle = new Rectangle(64, 0, 64, 64);
                        Vector2 worldPos = coinPos * 64;
                        _spriteBatch.Draw(_collectableSpriteSheet, worldPos, sourceRectangle, Color.White);
                    }

                    foreach (var healthPos in _collectedHealthPositions)
                    {
                        Rectangle sourceRectangle = new Rectangle(0, 0, 64, 64);
                        Vector2 worldPos = healthPos * 64;
                        _spriteBatch.Draw(_collectableSpriteSheet, worldPos, sourceRectangle, Color.White);
                    }

                    _spriteBatch.End();

                    _uiBatch.Begin();

                    SpriteFont font = Content.Load<SpriteFont>(@"Fonts\SpaceFont");
                    _uiBatch.DrawString(font, $"Points: {_points}", new Vector2(10, 10), Color.White);
                    _uiBatch.DrawString(font, $"Health:", new Vector2(10, 70), Color.White);

                    Rectangle sourceRectangleHealth = new Rectangle(0, 0, 0, 0);
                    switch (_health)
                    {
                        case 1:
                            sourceRectangleHealth = new Rectangle(0, 0, 64, 64);
                            break;
                        case 2:
                            sourceRectangleHealth = new Rectangle(64, 0, 64, 64);
                            break;
                        case 3:
                            sourceRectangleHealth = new Rectangle(128, 0, 64, 64);
                            break;
                        default:
                            break;
                    }

                    if (sourceRectangleHealth.Width > 0 && sourceRectangleHealth.Height > 0)
                    {
                        _uiBatch.Draw(_healthSpriteSheet, new Vector2(210, 60), sourceRectangleHealth, Color.White);
                    }

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

            // Clear collected coin positions and health positions
            _collectedCoinPositions.Clear();
            _collectedHealthPositions.Clear();

            // Clear existing enemies
            enemies.Clear();


            // Load new enemies for the current level
            foreach (var xx in levelEnemies)
            {
                if (xx.Key.Split(':')[0] == _lastPlayedLevel)
                {
                    foreach (var enemy in xx.Value)
                    {
                        Console.WriteLine($"Enemy Level: {enemy.Level}, Position: {enemy.Position}");

                        Enemy newEnemy = CreateEnemy(enemy.Level, enemy.Position);
                        if (newEnemy != null)
                        {
                            enemies.Add(newEnemy);
                            Console.WriteLine($"Added enemy of level {enemy.Level} at position {enemy.Position}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to create enemy of level {enemy.Level} at position {enemy.Position}");
                        }
                    }
                }
            }

            // Reload astronaut's starting position
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
