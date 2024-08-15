using Code.Code;
using Code.Input;
using Code.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Astronaut astronaut;

        private List<TileMap> layers;
        private Dictionary<string, Texture2D> textures;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D idleTexture = Content.Load<Texture2D>("AstronautIdle(64x64)x9");
            Texture2D runningTexture = Content.Load<Texture2D>("AstronautRunning(64x64)x12");
            // Initialize game objects with textures and movement controller
            InitializeGameObjects(idleTexture, runningTexture);

            textures = new Dictionary<string, Texture2D>();

            layers = TileMap.LoadFromCsv("../../../Data/map.csv");

            foreach (var layer in layers)
            {
                if (!textures.ContainsKey(layer.TextureName))
                {
                    textures[layer.TextureName] = Content.Load<Texture2D>(layer.TextureName);
                }

                var texture = textures[layer.TextureName];
                layer.TextureStore = TileMap.GenerateTextureStore(texture, 64); // Adjust as needed
            }
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

        private void RenderLayers(List<TileMap> layers)
        {
            foreach (var layer in layers.OrderBy(l => l.ZIndex))
            {
                var texture = textures[layer.TextureName];

                foreach (var item in layer.TileMapData)
                {
                    int tileIndex = item.Value - 1;

                    if (tileIndex < 0 || tileIndex >= layer.TextureStore.Count)
                    {
                        // Log or debug to see the problematic tile index and texture store size
                        System.Diagnostics.Debug.WriteLine($"Invalid tile index: {tileIndex}. Total tiles: {layer.TextureStore.Count}");
                        continue; // Skip this tile if the index is invalid
                    }

                    Rectangle destination = new Rectangle((int)item.Key.X * 64, (int)item.Key.Y * 64, 64, 64);
                    Rectangle source = layer.TextureStore[tileIndex];

                    _spriteBatch.Draw(texture, destination, source, Color.White);
                }
            }
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            astronaut.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            RenderLayers(layers);
            astronaut.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
