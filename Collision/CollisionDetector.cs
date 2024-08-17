using System;
using System.Collections.Generic;
using System.Linq;
using Code.Code;
using Code.Interfaces;
using Microsoft.Xna.Framework;

namespace Code
{
    public class CollisionDetector : ICollisionDetector
    {
        public (bool isColliding, Rectangle tileBounds) CheckCollision(Rectangle hitbox, List<TileMap> layers, int layerIndex)
        {
            if (layers == null)
            {
                throw new ArgumentNullException(nameof(layers), "Layers cannot be null");
            }

            foreach (var layer in layers.Where(l => l.ZIndex == layerIndex))
            {
                foreach (var item in layer.TileMapData)
                {
                    int tileIndex = item.Value.TileIndex - 1;

                    if (tileIndex < 0 || tileIndex >= layer.TextureStore.Count)
                        continue;

                    Rectangle tileBounds = new Rectangle((int)item.Key.X * 64, (int)item.Key.Y * 64, 64, 64);
                    if (hitbox.Intersects(tileBounds))
                    {
                        return (true, tileBounds);
                    }
                }
            }
            return (false, Rectangle.Empty);
        }

    }
}

