namespace Gamelab.Levels;

/// <summary>
/// Tuning knobs for procedural level generation.
/// Level distance scales linearly: <c>BaseDistance + DistanceGrowthPerLevel * (level - 1)</c>.
/// Enemy budget scales exponentially with a linear component:
/// <c>(BaseEnemyBudget + BudgetGrowthPerLevel * (level - 1)) * BudgetMultiplierPerLevel ^ (level - 1)</c>.
/// Spawn composition shifts toward shooters as levels increase, capped by <see cref="MaxShooterChance"/>.
/// </summary>
public class RunDifficultyConfig
{
    /// <summary>Distance target for the first procedural level (in world units).</summary>
    public float BaseDistance { get; set; } = 300f;

    /// <summary>Extra distance added per level beyond the first.</summary>
    public float DistanceGrowthPerLevel { get; set; } = 300f;

    /// <summary>Enemy budget for level 1. Zero means no enemies spawn on the first procedural level.</summary>
    public float BaseEnemyBudget { get; set; } = 0f;

    /// <summary>Linear budget increase per level.</summary>
    public float BudgetGrowthPerLevel { get; set; } = 2.5f;

    /// <summary>Exponential multiplier applied per level, compounding the budget growth.</summary>
    public float BudgetMultiplierPerLevel { get; set; } = 1.12f;

    /// <summary>How many budget points a single Shooter costs.</summary>
    public int ShooterCost { get; set; } = 3;

    /// <summary>How many budget points a single Thief costs.</summary>
    public int ThiefCost { get; set; } = 2;

    /// <summary>Probability of spawning a Shooter (vs. Thief) on level 1.</summary>
    public float BaseShooterChance { get; set; } = 0.45f;

    /// <summary>Per-level increase in Shooter spawn probability.</summary>
    public float ShooterChanceIncreasePerLevel { get; set; } = 0.02f;

    /// <summary>Hard cap on Shooter spawn probability regardless of level number.</summary>
    public float MaxShooterChance { get; set; } = 0.85f;

    /// <summary>Minimum distance (world units) between consecutive spawn events.</summary>
    public float MinSpawnSpacing { get; set; } = 120f;

    /// <summary>Maximum distance (world units) between consecutive spawn events.</summary>
    public float MaxSpawnSpacing { get; set; } = 450f;

    /// <summary>RNG seed for deterministic level generation across runs.</summary>
    public int RandomSeed { get; set; } = 1337;
}
