using System;
using System.Collections.Generic;
using Gamelab.Levels;

namespace Gamelab.Enemies;

public static class EnemyCatalog
{
    private static readonly EnemyType[] EarlyProceduralTypes =
    [
        EnemyType.Mounter,
        EnemyType.Rifle
    ];

    private static readonly EnemyType[] MidProceduralTypes =
    [
        EnemyType.Mounter,
        EnemyType.Rifle,
        EnemyType.Molotov,
        EnemyType.TarThrower
    ];

    private static readonly EnemyType[] LateProceduralTypes =
    [
        EnemyType.Mounter,
        EnemyType.Rifle,
        EnemyType.Shield,
        EnemyType.Molotov,
        EnemyType.TarThrower,
        EnemyType.Anchor
    ];

    public static IReadOnlyList<EnemyType> GetProceduralTypesForLevel(int levelNumber)
    {
        return levelNumber switch
        {
            <= 2 => EarlyProceduralTypes,
            <= 4 => MidProceduralTypes,
            _ => LateProceduralTypes,
        };
    }

    public static float GetProceduralWeight(EnemyType type, int levelNumber)
    {
        return type switch
        {
            EnemyType.Mounter => levelNumber <= 2 ? 1.25f : 0.9f,
            EnemyType.Rifle => 1.2f + levelNumber * 0.05f,
            EnemyType.Shield => levelNumber >= 5 ? 0.75f : 0f,
            EnemyType.Anchor => levelNumber >= 6 ? 0.5f : 0f,
            EnemyType.Molotov => levelNumber >= 3 ? 0.7f : 0f,
            EnemyType.TarThrower => levelNumber >= 3 ? 0.65f : 0f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.")
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
