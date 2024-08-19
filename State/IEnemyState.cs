using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.State
{
    public interface IEnemyState
    {
        void Update(Enemy enemy, GameTime gameTime);
    }
}
