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

        public Dictionary<string, List<(int Level, Vector2 Position)>> ReadEnemies(string filePath)
        {
            var levelEnemies = new Dictionary<string, List<(int Level, Vector2 Position)>>();

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
                            if (!levelEnemies.ContainsKey(currentLevel))
                            {
                                levelEnemies[currentLevel] = new List<(int Level, Vector2 Position)>();
                            }
                        }
                        else if (line.StartsWith("Enemy") && currentLevel != null)
                        {
                            var parts = line.Split(':');
                            if (parts.Length == 2)
                            {
                                var enemiesData = parts[1].Trim();
                                var enemies = enemiesData.Split(',');

                                foreach (var enemy in enemies)
                                {
                                    var enemyParts = enemy.Trim('(', ')').Split(';');
                                    if (enemyParts.Length == 3 &&
                                        int.TryParse(enemyParts[0], out int level) &&
                                        float.TryParse(enemyParts[1], out float x) &&
                                        float.TryParse(enemyParts[2], out float y))
                                    {
                                        levelEnemies[currentLevel].Add((level, new Vector2(x, y)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file: {ex.Message}");
            }

            return levelEnemies;
        }

    }
}
