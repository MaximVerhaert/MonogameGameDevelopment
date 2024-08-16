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
            public Dictionary<Vector2, (int TileIndex, int Rotation)> TileMapData { get; private set; } // Updated to store both TileIndex and Rotation
            public List<Rectangle> TextureStore { get; set; }

            private const int TileSize = 64; // Size of each tile in pixels

            public TileMap(string textureName, int zIndex, Dictionary<Vector2, (int, int)> tileMapData, List<Rectangle> textureStore)
            {
                TextureName = textureName;
                ZIndex = zIndex;
                TileMapData = tileMapData;
                TextureStore = textureStore;
            }

            public static Dictionary<string, List<TileMap>> LoadLevelsFromCsv(string filepath)
            {
                var levels = new Dictionary<string, List<TileMap>>();
                using (var reader = new StreamReader(filepath))
                {
                    List<TileMap> currentLevelLayers = null;
                    TileMap currentLayer = null;
                    string currentLevel = null;
                    string line;
                    int row = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        if (line.EndsWith(":"))
                        {
                            if (currentLevel != null && currentLevelLayers != null)
                            {
                                levels[currentLevel] = currentLevelLayers;
                            }

                            currentLevel = line.TrimEnd(':');
                            currentLevelLayers = new List<TileMap>();
                            currentLayer = null;
                            continue;
                        }

                        var parts = line.Split(',');

                        if (parts.Length == 3)
                        {
                            var layerName = parts[0].Trim();
                            var zIndexStr = parts[1].Trim();
                            var textureName = parts[2].Trim();

                            if (int.TryParse(zIndexStr, out var zIndex))
                            {
                                if (currentLayer != null)
                                {
                                    currentLevelLayers.Add(currentLayer);
                                }

                                currentLayer = new TileMap(textureName, zIndex, new Dictionary<Vector2, (int, int)>(), new List<Rectangle>());
                                row = 0;
                            }
                            else
                            {
                                Console.WriteLine($"Invalid zIndex format: {zIndexStr}");
                            }
                        }
                        else if (currentLayer != null)
                        {
                            var tiles = line.Split(',');

                            if (tiles.Length > 0)
                            {
                                for (int col = 0; col < tiles.Length; col++)
                                {
                                    var tileData = tiles[col].Trim('(', ')').Split(';');

                                    if (tileData.Length == 2 && int.TryParse(tileData[0], out var tileIndex) && int.TryParse(tileData[1], out var rotation))
                                    {
                                        currentLayer.TileMapData[new Vector2(col, row)] = (tileIndex, rotation);
                                    }
                                }
                                row++;
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

                    if (currentLevel != null && currentLevelLayers != null)
                    {
                        levels[currentLevel] = currentLevelLayers;
                    }
                }

                return levels;
            }


            public static List<Rectangle> GenerateTextureStore(Texture2D texture, int tileSize)
            {
                List<Rectangle> textureStore = new List<Rectangle>();
                int tilesPerRow = texture.Width / tileSize;
                int tilesPerColumn = texture.Height / tileSize;

                for (int y = 0; y < tilesPerColumn; y++)
                {
                    for (int x = 0; x < tilesPerRow; x++)
                    {
                        textureStore.Add(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize));
                    }
                }

                return textureStore;
            }
        }
    }
}
