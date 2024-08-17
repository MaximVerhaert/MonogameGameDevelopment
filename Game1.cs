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
        private MainMenu _mainMenu;
        private GameState _currentState;
        private string _lastPlayedLevel = "lvl1";
        private double _lastClickTime = 0;
        private const double ClickCooldown = 200; // milliseconds

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
        }

        private void InitializeGameObjects(Texture2D idleTexture, Texture2D runningTexture)
        {
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
            }

            astronaut.Update(gameTime);
            _camera.Update(astronaut.Position);
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
            _currentState = GameState.Playing;
        }

        private void OnExitRequested(object sender, EventArgs e)
        {
            Exit();
        }
    }
}
