using Code;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Interfaces
{
    public interface ILevelManager
    {
        void LoadLevels(string path);
        void SetCurrentLevel(string levelName);
        RenderTarget2D MapRenderTarget { get; }
        List<TileMap> Layers { get; }
    }

}
