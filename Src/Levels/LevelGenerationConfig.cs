using System;

namespace Gamelab.Levels;

/// <summary>
/// Tuning knobs for procedural level generation.
/// Level distance scales linearly: <c>BaseDistance + DistanceGrowthPerLevel * (level - 1)</c>.
/// Enemy budget scales exponentially with a linear component:
/// <c>BaseEnemyBudget * BudgetMultiplierPerLevel ^ (level - 1) + BudgetGrowthPerLevel * (level - 1)</c>.
/// Procedural levels currently spawn rifle enemies only.
/// </summary>
public class LevelGenerationConfig
{
    /// <summary>Distance target for the first procedural level (in world units).</summary>
    public float BaseDistance { get; set; } = 15000f;

    /// <summary>Extra distance added per level beyond the first.</summary>
    public float DistanceGrowthPerLevel { get; set; } = 4000f;

    /// <summary>Enemy budget for level 1. Zero means no enemies spawn on the first procedural level.</summary>
    public float BaseEnemyBudget { get; set; } = 2.5f;

    /// <summary>Linear budget increase per level.</summary>
    public float BudgetGrowthPerLevel { get; set; } = 2.5f;

    /// <summary>Exponential multiplier applied per level, compounding the budget growth.</summary>
    public float BudgetMultiplierPerLevel { get; set; } = 1.12f;

    /// <summary>How many budget points a single Rifle costs.</summary>
    public int RifleCost { get; set; } = 3;

    /// <summary>Distance (world units) at the start of each level where no enemies spawn.</summary>
    public float SafeZoneDistance { get; set; } = 2000f;

    /// <summary>Minimum distance (world units) between consecutive spawn events.</summary>
    public float MinSpawnSpacing { get; set; } = 120f;

    /// <summary>Maximum distance (world units) between consecutive spawn events.</summary>
    public float MaxSpawnSpacing { get; set; } = 450f;

    /// <summary>RNG seed for deterministic level generation across runs.</summary>
    public int RandomSeed { get; set; } = Random.Shared.Next();
}