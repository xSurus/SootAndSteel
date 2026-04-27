using System;
using System.Collections.Generic;
using Gamelab.Levels;

namespace Gamelab.Enemies.Core;

public static class EnemyCatalog
{
    private static readonly EnemyType[] ProceduralTypes =
    [
        EnemyType.Rifle
    ];

    public static IReadOnlyList<EnemyType> GetProceduralTypesForLevel(int levelNumber)
    {
        return ProceduralTypes;
    }

    public static float GetProceduralWeight(EnemyType type, int levelNumber)
    {
        if (type != EnemyType.Rifle)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.");
        }

        return 1.2f + levelNumber * 0.05f;
    }

    public static int GetCost(LevelGenerationConfig config, EnemyType type)
    {
        if (type != EnemyType.Rifle)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.");
        }

        return config.RifleCost;
    }
}
