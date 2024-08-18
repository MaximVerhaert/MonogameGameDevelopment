using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Code.Interfaces
{
    public interface ILevelLocationManager
    {
        Dictionary<string, Vector2> ReadStartingPositions(string filePath);
        // Other methods for reading level data can be added here
        Dictionary<string, List<(int Level, Vector2 Position)>> ReadEnemies(string filePath);

    }
}
