using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Code
{
    public class TileMap
    {
        private Dictionary<Vector2, int> tilemap;
        private List<Rectangle> textureStore;
        private Texture2D textureSpaceship;

        public TileMap(Texture2D textureSpaceship, string mapFilePath)
        {
            this.textureSpaceship = textureSpaceship;
            tilemap = LoadMap(mapFilePath);
            InitializeTextureStore();
        }

        private Dictionary<Vector2, int> LoadMap(string filepath)
        {
            Dictionary<Vector2, int> result = new();
            StreamReader reader = new(filepath);
            int y = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');
                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value))
                    {
                        if (value > 0)
                        {
                            result[new Vector2(x, y)] = value;
                        }
                    }
                }
                y++;
            }
            return result;
        }

        private void InitializeTextureStore()
        {
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

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var item in tilemap)
            {
                Rectangle dest = new(
                    (int)item.Key.X * 64,
                    (int)item.Key.Y * 64,
                    64,
                    64
                );
                Rectangle src = textureStore[item.Value - 1];
                spriteBatch.Draw(textureSpaceship, dest, src, Color.White);
            }
        }
    }
}
