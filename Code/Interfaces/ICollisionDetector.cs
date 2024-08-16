using Code.Code;
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
    public interface ICollisionDetector
    {
        bool CheckCollision(Rectangle hitbox, List<TileMap> layers);
    }
}
