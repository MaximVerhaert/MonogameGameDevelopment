using Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code.Interfaces
{
    // Modify ICollisionDetector interface to return a tuple
    public interface ICollisionDetector
    {
        (bool isColliding, Rectangle tileBounds, Vector2 tilePosition) CheckCollision(Rectangle hitbox, List<TileMap> layers, int layerIndex);
        // Overloaded method for checking collisions between two rectangles (e.g., player and enemy)
        bool CheckCollision(Rectangle hitbox1, Rectangle hitbox2);
    }

}
