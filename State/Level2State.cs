using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.State
{
    public class Level2State : IEnemyState
    {
        public void Update(Enemy enemy, GameTime gameTime)
        {
            enemy.UpdateLevel2Behavior(gameTime);
        }
    }
}
