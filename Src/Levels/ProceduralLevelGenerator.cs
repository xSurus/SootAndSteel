using System;

namespace Gamelab.Levels;

public class ProceduralLevelGenerator(RunDifficultyConfig config)
{
    private readonly RunDifficultyConfig config = config ?? new RunDifficultyConfig();

    /// <summary>
    /// Builds a level definition from the configured scaling model for a given level number.
    /// The result contains a distance target and spawn events distributed across that distance.
    /// </summary>
    public LevelDefinition Generate(int levelNumber)
    {
        int safeLevel = Math.Max(1, levelNumber);
        float levelDistance = config.BaseDistance + (safeLevel - 1) * config.DistanceGrowthPerLevel;
        float floatBudget = config.BaseEnemyBudget * MathF.Pow(config.BudgetMultiplierPerLevel, safeLevel - 1) +
                            (safeLevel - 1) * config.BudgetGrowthPerLevel;
        int budget = Math.Max(config.ThiefCost, (int)MathF.Round(floatBudget));

        var definition = new LevelDefinition
        {
            LevelDistance = levelDistance
        };

        var random = new Random(unchecked(config.RandomSeed + safeLevel * 7919));
        float cursorDistance = config.MinSpawnSpacing;
        float levelEndBuffer = MathF.Max(config.MinSpawnSpacing, 200f);
        float maxSpawnDistance = MathF.Max(config.MinSpawnSpacing, levelDistance - levelEndBuffer);
        int minCost = Math.Min(config.ShooterCost, config.ThiefCost);

        while (budget >= minCost && cursorDistance <= maxSpawnDistance)
        {
            bool canSpawnShooter = budget >= config.ShooterCost;
            bool canSpawnThief = budget >= config.ThiefCost;
            bool spawnShooter = canSpawnShooter && (!canSpawnThief || IsShooterRoll(random, safeLevel));

            int cost = spawnShooter ? config.ShooterCost : config.ThiefCost;
            budget -= cost;

            definition.SpawnEvents.Add(new SpawnEvent
            {
                Distance = cursorDistance,
                Type = spawnShooter ? "Shooter" : "Thief",
                Side = random.NextSingle() < 0.5f ? "Top" : "Bottom"
            });

            float spacing = Lerp(config.MaxSpawnSpacing, config.MinSpawnSpacing, MathF.Min(1f, safeLevel / 20f));
            spacing *= 0.8f + random.NextSingle() * 0.4f;
            cursorDistance += MathF.Max(config.MinSpawnSpacing, spacing);
        }

        return definition;
    }

    private bool IsShooterRoll(Random random, int levelNumber)
    {
        float chance = config.BaseShooterChance + (levelNumber - 1) * config.ShooterChanceIncreasePerLevel;
        chance = Math.Clamp(chance, 0f, config.MaxShooterChance);
        return random.NextSingle() < chance;
    }
    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
