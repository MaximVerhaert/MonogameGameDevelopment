using Code.Input;
using Code.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1;
using System.Collections.Generic;
using System.IO;

namespace Code
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch;

        private Texture2D playerSprite;
        private Astronaut astronaut;

        private Dictionary<Vector2, int> tilemap;
        private List<Rectangle> textureStore;
        private Texture2D textureSpaceship;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            tilemap = LoadMap("../../../Data/map.csv");
            textureStore = new()
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(64, 0, 64, 64),
                new Rectangle(128, 0, 64, 64),
                new Rectangle(192, 0, 64, 64),
                new Rectangle(256, 0, 64, 64),
                new Rectangle(320, 0, 64, 64),
                new Rectangle(384, 0, 64, 64),
                new Rectangle(448, 0, 64, 64),
                new Rectangle(512, 0, 64, 64),
                new Rectangle(576, 0, 64, 64),
                new Rectangle(640, 0, 64, 64)
            };
        }

        private Dictionary<Vector2, int> LoadMap(string filepath)
        {
            Dictionary<Vector2, int> result = new();
            StreamReader reader = new(filepath);
            int y = 0;
            string line;
            while((line=reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');
                for(int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value)) {
                        if(value > 0)
                        {
                            result[new Vector2(x, y)] = value;
                        }
                    }
                }
                y++;
            }
            return result;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);
            textureSpaceship = Content.Load<Texture2D>("Border(64x64)x11");

            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");

            InitializeGameObjects(idleTexture, runningTexture);
        }

        private void InitializeGameObjects(Texture2D idleTexture, Texture2D runningTexture)
        {
            IMovementController movementController = new MovementController(
                initialSpeed: new Vector2(1, 1),
                initialAcceleration: new Vector2(1f, 1f),
                maxAcceleration: 5f
            );

            astronaut = new Astronaut(idleTexture, runningTexture, new KeyBoardReader(), movementController);
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            astronaut.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

_spriteBatch.Begin();
            foreach (var item in tilemap)
            {
                Rectangle dest = new(
                    (int)item.Key.X * 64,
                    (int)item.Key.Y * 64,
                    64,
                    64
                );
                Rectangle src = textureStore[item.Value - 1];
                _spriteBatch.Draw(textureSpaceship, dest, src, Color.White);
            }
            astronaut.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
