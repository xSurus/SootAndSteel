using System;

namespace Gamelab.Levels
{
    /// <summary>
    /// Tuning knobs for procedural level generation.
    /// Distance: <c>BaseDistance + DistanceGrowthPerLevel * (level - 1)</c>.
    /// Budget: <c>BaseEnemyBudget * BudgetMultiplierPerLevel ^ (level - 1) + BudgetGrowthPerLevel * (level - 1)</c>.
    /// </summary>
    public class LevelGenerationConfig
    {
        // Src used Random.Shared (.NET 6+). One static instance stands in for it. Not thread safe, fine on the main thread.
        private static readonly Random SeedSource = new Random();

        /// <summary>Distance target for the first procedural level (world units).</summary>
        public float BaseDistance { get; set; } = 15000f;

        /// <summary>Extra distance added per level beyond the first.</summary>
        public float DistanceGrowthPerLevel { get; set; } = 4000f;

        /// <summary>Enemy budget for level 1. Zero means no enemies on the first level.</summary>
        public float BaseEnemyBudget { get; set; } = 2.5f;

        /// <summary>Linear budget increase per level.</summary>
        public float BudgetGrowthPerLevel { get; set; } = 2.5f;

        /// <summary>Exponential multiplier applied per level.</summary>
        public float BudgetMultiplierPerLevel { get; set; } = 1.12f;

        /// <summary>Budget cost of a baseline enemy before ammo surcharges.</summary>
        public int BaseCost { get; set; } = 3;

        /// <summary>Distance at the start (and end) of each level where no enemies spawn.</summary>
        public float SafeZoneDistance { get; set; } = 2000f;

        /// <summary>Minimum distance between consecutive spawn events.</summary>
        public float MinSpawnSpacing { get; set; } = 120f;

        /// <summary>Maximum distance between consecutive spawn events.</summary>
        public float MaxSpawnSpacing { get; set; } = 450f;

        /// <summary>RNG seed for deterministic generation.</summary>
        public int RandomSeed { get; set; } = SeedSource.Next();
    }
}
