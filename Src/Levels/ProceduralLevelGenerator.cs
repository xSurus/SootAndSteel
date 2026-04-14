using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Enemies;

namespace Gamelab.Levels;

public class ProceduralLevelGenerator(RunDifficultyConfig config)
{
    private readonly RunDifficultyConfig config = config ?? new RunDifficultyConfig();
    private readonly float threatScale = 1f;
    private readonly float spawnSpacingScale = 1f;

    public ProceduralLevelGenerator(RunDifficultyConfig config, float threatScale, float spawnSpacingScale) :
        this(config)
    {
        this.threatScale = Math.Max(1f, threatScale);
        this.spawnSpacingScale = Math.Clamp(spawnSpacingScale, 0.4f, 1f);
    }

    /// <summary>
    /// Builds a level definition from the configured scaling model for a given level number.
    /// The result contains a distance target and spawn events distributed across that distance.
    /// </summary>
    public LevelDefinition Generate(int levelNumber)
    {
        int safeLevel = Math.Max(1, levelNumber);
        float levelDistance = config.BaseDistance + (safeLevel - 1) * config.DistanceGrowthPerLevel;
        float safeZoneDistance = config.SafeZoneDistance;
        float floatBudget = config.BaseEnemyBudget * MathF.Pow(config.BudgetMultiplierPerLevel, safeLevel - 1) +
                            (safeLevel - 1) * config.BudgetGrowthPerLevel;
        floatBudget *= threatScale;
        int minCost = GetMinProceduralCost(safeLevel);
        int budget = Math.Max(minCost, (int)MathF.Round(floatBudget));

        var definition = new LevelDefinition
        {
            LevelDistance = levelDistance
        };

        var random = new Random(unchecked(config.RandomSeed + safeLevel * 7919));
        float levelEndBuffer = MathF.Max(config.MinSpawnSpacing, 200f);
        float maxSpawnDistance = MathF.Max(config.MinSpawnSpacing, levelDistance - levelEndBuffer);
        List<float> distances = [];
        while (budget >= minCost)
        {
            float spawnDistance = random.NextSingle() * (maxSpawnDistance - safeZoneDistance) + safeZoneDistance;
            if (distances.Any(d => MathF.Abs(d - spawnDistance) < config.MinSpawnSpacing)) continue;
            distances.Add(spawnDistance);

            EnemyType? selectedType = SelectEnemyType(random, safeLevel, budget);
            if (selectedType == null)
            {
                break;
            }

            EnemyType enemyType = selectedType.Value;
            budget -= EnemyCatalog.GetCost(config, enemyType);

            definition.SpawnEvents.Add(new SpawnEvent
            {
                Distance = spawnDistance,
                Type = new EnemyDefinition(enemyType).Id,
                Side = random.NextSingle() < 0.5f ? "Top" : "Bottom"
            });
        }

        return definition;
    }

    private EnemyType? SelectEnemyType(Random random, int levelNumber, int budget)
    {
        IReadOnlyList<EnemyType> candidateTypes = EnemyCatalog.GetProceduralTypesForLevel(levelNumber);
        List<EnemyType> affordableTypes = [];
        float totalWeight = 0f;

        foreach (EnemyType type in candidateTypes)
        {
            int cost = EnemyCatalog.GetCost(config, type);
            if (budget < cost)
            {
                continue;
            }

            float weight = EnemyCatalog.GetProceduralWeight(type, levelNumber);
            if (weight <= 0f)
            {
                continue;
            }

            affordableTypes.Add(type);
            totalWeight += weight;
        }

        if (affordableTypes.Count == 0 || totalWeight <= 0f)
        {
            return null;
        }

        float roll = random.NextSingle() * totalWeight;
        foreach (EnemyType type in affordableTypes)
        {
            roll -= EnemyCatalog.GetProceduralWeight(type, levelNumber);
            if (roll <= 0f)
            {
                return type;
            }
        }

        return affordableTypes[^1];
    }

    private int GetMinProceduralCost(int levelNumber)
    {
        int minCost = int.MaxValue;
        foreach (EnemyType type in EnemyCatalog.GetProceduralTypesForLevel(levelNumber))
        {
            minCost = Math.Min(minCost, EnemyCatalog.GetCost(config, type));
        }

        return minCost == int.MaxValue ? 0 : minCost;
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}