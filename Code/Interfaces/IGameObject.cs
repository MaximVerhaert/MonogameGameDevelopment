﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Interfaces
{
    internal interface IGameObject
    {
        void Update(GameTime gametime);
        void Draw(SpriteBatch _spriteBatch);
    }
}
