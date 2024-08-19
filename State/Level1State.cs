using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.State
{
    public class Level1State : IEnemyState
    {
        public void Update(Enemy enemy, GameTime gameTime)
        {
            enemy.UpdateLevel1Behavior(gameTime);
        }
    }
}
