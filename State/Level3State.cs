using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.State
{
    public class Level3State : IEnemyState
    {
        public void Update(Enemy enemy, GameTime gameTime)
        {
            enemy.UpdateLevel3Behavior(gameTime);
        }
    }
}
