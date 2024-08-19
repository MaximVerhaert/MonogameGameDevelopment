using System;
using System.Collections.Generic;
using System.Linq;
using Code;
using Code.Interfaces;
using Microsoft.Xna.Framework;

namespace Code
{
    public class CollisionDetector : ICollisionDetector
    {
        public (bool isColliding, Rectangle tileBounds, Vector2 tilePosition) CheckCollision(Rectangle hitbox, List<TileMap> layers, int layerIndex)
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
                        // Convert Vector2 to Point before returning
                        Vector2 tilePosition = item.Key;
                        return (true, tileBounds, tilePosition);
                    }
                }
            }
            return (false, Rectangle.Empty, Vector2.Zero);
        }

        // New method for rectangle collision detection
        public bool CheckCollision(Rectangle hitbox1, Rectangle hitbox2)
        {
            return hitbox1.Intersects(hitbox2);
        }
    }
}

