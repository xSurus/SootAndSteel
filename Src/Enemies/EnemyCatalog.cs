using System;
using System.Collections.Generic;
using Gamelab.Levels;

namespace Gamelab.Enemies;

public static class EnemyCatalog
{
    private static readonly EnemyType[] EarlyProceduralTypes =
    [
        EnemyType.Mounter,
        EnemyType.Rifle,
        EnemyType.Molotov
    ];

    public static IReadOnlyList<EnemyType> GetProceduralTypesForLevel(int levelNumber)
    {
        return levelNumber switch
        {
            <= 0 => EarlyProceduralTypes,
            _ => EarlyProceduralTypes,
        };
    }

    public static int GetCost(RunDifficultyConfig config, EnemyType type)
    {
        return type switch
        {
            EnemyType.Mounter => config.MounterCost,
            EnemyType.Rifle => config.RifleCost,
            EnemyType.Shield => config.ShieldCost,
            EnemyType.Anchor => config.AnchorCost,
            EnemyType.Molotov => config.MolotovCost,
            EnemyType.TarThrower => config.TarThrowerCost,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.")
        };
    }
}
