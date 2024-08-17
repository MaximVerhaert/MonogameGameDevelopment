using Code.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Code.Map
{
    public class LevelLocationManager : ILevelLocationManager
    {
        public Dictionary<string, Vector2> ReadStartingPositions(string filePath)
        {
            var startingPositions = new Dictionary<string, Vector2>();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    string currentLevel = null;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("lvl"))
                        {
                            currentLevel = line.Trim();
                        }
                        else if (line.StartsWith("StartingPosition") && currentLevel != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 3)
                            {
                                var x = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                                var y = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                                startingPositions[currentLevel] = new Vector2(x, y);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file: {ex.Message}");
            }

            return startingPositions;
        }
    }
}
