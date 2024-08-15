using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Linq;
using System;

namespace Code
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Collections.Generic;
    using System.IO;

    namespace Code
    {
        public class TileMap
        {
            public int ZIndex { get; private set; }
            public string TextureName { get; private set; }
            public Dictionary<Vector2, int> TileMapData { get; private set; }
            public List<Rectangle> TextureStore { get;  set; }

            private const int TileSize = 64; // Size of each tile in pixels

            public TileMap(string textureName, int zIndex, Dictionary<Vector2, int> tileMapData, List<Rectangle> textureStore)
            {
                TextureName = textureName;
                ZIndex = zIndex;
                TileMapData = tileMapData;
                TextureStore = textureStore;
            }

            public static List<TileMap> LoadFromCsv(string filepath)
            {
                var layers = new List<TileMap>();
                using (var reader = new StreamReader(filepath))
                {
                    TileMap currentLayer = null;
                    string line;
                    int row = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        var parts = line.Split(',');

                        // Check if the line contains the correct number of parts
                        if (parts.Length == 3)
                        {
                            // Layer metadata
                            var layerName = parts[0].Trim();
                            var zIndexStr = parts[1].Trim();
                            var textureName = parts[2].Trim();

                            // Parse zIndex and create new layer if valid
                            if (int.TryParse(zIndexStr, out var zIndex))
                            {
                                // Add the previous layer to the list
                                if (currentLayer != null)
                                {
                                    layers.Add(currentLayer);
                                }

                                // Create a new TileMap instance for the layer
                                currentLayer = new TileMap(textureName, zIndex, new Dictionary<Vector2, int>(), new List<Rectangle>());
                                row = 0; // Reset row for new layer
                            }
                            else
                            {
                                Console.WriteLine($"Invalid zIndex format: {zIndexStr}");
                            }
                        }
                        else if (currentLayer != null)
                        {
                            // Handle tile map data
                            var tiles = line.Split(',');

                            // Check if tiles contain valid data
                            if (tiles.Length > 0)
                            {
                                for (int col = 0; col < tiles.Length; col++)
                                {
                                    if (int.TryParse(tiles[col], out var tileIndex))
                                    {
                                        currentLayer.TileMapData[new Vector2(col, row)] = tileIndex;
                                    }
                                }
                                row++; // Move to the next row
                            }
                            else
                            {
                                Console.WriteLine($"Empty or invalid tile map data line: {line}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Unexpected line format: {line}");
                        }
                    }

                    // Add the last layer if it exists
                    if (currentLayer != null)
                    {
                        layers.Add(currentLayer);
                    }
                }

                return layers;
            }



            public static List<Rectangle> GenerateTextureStore(Texture2D texture, int tileCount)
            {
                var textureStore = new List<Rectangle>();
                int textureWidth = texture.Width;
                int textureHeight = texture.Height;

                int tilesX = textureWidth / TileSize;
                int tilesY = textureHeight / TileSize;

                for (int y = 0; y < tilesY; y++)
                {
                    for (int x = 0; x < tilesX; x++)
                    {
                        if (textureStore.Count >= tileCount)
                        {
                            return textureStore;
                        }
                        textureStore.Add(new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize));
                    }
                }

                return textureStore;
            }
        }
    }


}
