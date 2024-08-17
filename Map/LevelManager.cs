using Code.Code;
using Code.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Map
{
    public class LevelManager : ILevelManager
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private ContentManager _contentManager;
        private Dictionary<string, List<TileMap>> _levels;
        private Dictionary<string, Texture2D> _textures;
        private RenderTarget2D _mapRenderTarget;

        public List<TileMap> Layers { get; private set; }

        public LevelManager(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _contentManager = contentManager;
            _textures = new Dictionary<string, Texture2D>();
            _levels = new Dictionary<string, List<TileMap>>();
            Layers = new List<TileMap>();
        }

        public void LoadLevels(string path)
        {
            _levels = TileMap.LoadLevelsFromCsv(path);

            if (_levels == null || !_levels.Any())
            {
                throw new InvalidOperationException("No levels were loaded. Check the path and CSV format.");
            }
        }

        public void SetCurrentLevel(string levelName)
        {
            if (_levels.ContainsKey(levelName))
            {
                Layers = _levels[levelName];

                if (Layers == null || !Layers.Any())
                {
                    throw new InvalidOperationException($"Layers for level '{levelName}' are not loaded correctly.");
                }

                // Load and cache textures for the new level
                foreach (var layer in Layers)
                {
                    if (!_textures.ContainsKey(layer.TextureName))
                    {
                        _textures[layer.TextureName] = _contentManager.Load<Texture2D>(layer.TextureName);
                    }

                    var texture = _textures[layer.TextureName];
                    layer.TextureStore = TileMap.GenerateTextureStore(texture, 64); // Adjust as needed
                }

                // Setup render target and render layers
                var viewport = _graphicsDevice.Viewport;

                int maxX = (int)Layers.Max(l => l.TileMapData.Keys.Max(k => k.X));
                int maxY = (int)Layers.Max(l => l.TileMapData.Keys.Max(k => k.Y));

                // Ensure calculations are done with integers
                int mapWidth = (maxX + 1) * 64;  // Use (maxX + 1) to include the last tile
                int mapHeight = (maxY + 1) * 64; // Use (maxY + 1) to include the last tile

                _mapRenderTarget = new RenderTarget2D(_graphicsDevice, mapWidth, mapHeight);

                _graphicsDevice.SetRenderTarget(_mapRenderTarget);
                _graphicsDevice.Clear(Color.Transparent);

                _spriteBatch.Begin();
                RenderStaticLayers();
                _spriteBatch.End();

                _graphicsDevice.SetRenderTarget(null); // Reset to default
            }
            else
            {
                throw new InvalidOperationException($"Level '{levelName}' not found.");
            }
        }

        private void RenderStaticLayers()
        {
            var viewport = _graphicsDevice.Viewport;
            var visibleArea = new Rectangle(0, 0, viewport.Width, viewport.Height);

            foreach (var layer in Layers.OrderBy(l => l.ZIndex))
            {
                var texture = _textures[layer.TextureName];

                foreach (var item in layer.TileMapData)
                {
                    int tileIndex = item.Value.TileIndex - 1;
                    int rotation = item.Value.Rotation;

                    if (tileIndex < 0 || tileIndex >= layer.TextureStore.Count)
                    {
                        continue;
                    }

                    Rectangle destination = new Rectangle((int)item.Key.X * 64, (int)item.Key.Y * 64, 64, 64);
                    Rectangle source = layer.TextureStore[tileIndex];
                    float rotationRadians = MathHelper.ToRadians(rotation);
                    Vector2 origin = new Vector2(source.Width / 2f, source.Height / 2f);
                    Rectangle adjustedDestination = new Rectangle(
                        destination.X + (int)origin.X,
                        destination.Y + (int)origin.Y,
                        destination.Width,
                        destination.Height
                    );

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

        public RenderTarget2D MapRenderTarget => _mapRenderTarget;
    }
}
