using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;

namespace Gamelab.Levels;

public class ProceduralLevelGenerator(LevelGenerationConfig config)
{
    private readonly LevelGenerationConfig config = config ?? new LevelGenerationConfig();
    private readonly float threatScale = 1f;
    private readonly float spawnSpacingScale = 1f;

    public ProceduralLevelGenerator(LevelGenerationConfig config, float threatScale, float spawnSpacingScale) :
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
        int currentLevel = Math.Max(1, levelNumber);
        float currentLevelDistance = config.BaseDistance + (currentLevel - 1) * config.DistanceGrowthPerLevel;
        float safeZoneDistance = config.SafeZoneDistance;
        float floatBudget = config.BaseEnemyBudget * MathF.Pow(config.BudgetMultiplierPerLevel, currentLevel - 1) +
                            (currentLevel - 1) * config.BudgetGrowthPerLevel;

        floatBudget *= threatScale;
        int minCost = GetMinProceduralCost(currentLevel);
        int budget = Math.Max(minCost, (int)MathF.Round(floatBudget));
        int levelSeed = unchecked(config.RandomSeed + currentLevel * 7919);

        var definition = new LevelDefinition
        {
            LevelDistance = currentLevelDistance,
            LevelSeed = levelSeed
        };

        Random random = new Random(levelSeed);
        float levelEndBuffer = config.SafeZoneDistance;
        float maxSpawnDistance = MathF.Max(config.MinSpawnSpacing, currentLevelDistance - levelEndBuffer);

        List<float> distances = [];

        while (budget >= minCost)
        {
            float spawnDistance = random.NextSingle() * (maxSpawnDistance - safeZoneDistance) + safeZoneDistance;
            if (distances.Any(d => MathF.Abs(d - spawnDistance) < config.MinSpawnSpacing * spawnSpacingScale))
            {
                continue;
            }

            distances.Add(spawnDistance);

            EnemySpawnOption? selectedOption = SelectSpawnOption(random, currentLevel, budget);
            if (selectedOption == null)
            {
                break;
            }

            EnemySpawnOption option = selectedOption.Value;
            budget -= EnemyCatalog.GetCost(config, option.Type, option.AmmoDefinition.Id);

            definition.SpawnEvents.Add(new SpawnEvent
            {
                Distance = spawnDistance,
                Type = new EnemyDefinition(option.Type).Id,
                AmmoId = option.AmmoDefinition.Id,
                Side = random.NextSingle() < 0.5f ? "Top" : "Bottom"
            });
        }

        definition.SpawnEvents.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        return definition;
    }

    private EnemySpawnOption? SelectSpawnOption(Random random, int levelNumber, int budget)
    {
        IReadOnlyList<EnemySpawnOption> candidateOptions = EnemyCatalog.GetProceduralSpawnOptions(levelNumber);
        List<EnemySpawnOption> affordableOptions = [];
        float totalWeight = 0f;

        foreach (EnemySpawnOption option in candidateOptions)
        {
            int cost = EnemyCatalog.GetCost(config, option.Type, option.AmmoDefinition.Id);
            if (budget < cost)
            {
                continue;
            }

            float weight = EnemyCatalog.GetProceduralWeight(option, levelNumber);
            if (weight <= 0f)
            {
                continue;
            }

            affordableOptions.Add(option);
            totalWeight += weight;
        }

        if (affordableOptions.Count == 0 || totalWeight <= 0f)
        {
            return null;
        }

        float roll = random.NextSingle() * totalWeight;
        foreach (EnemySpawnOption option in affordableOptions)
        {
            roll -= EnemyCatalog.GetProceduralWeight(option, levelNumber);
            if (roll <= 0f)
            {
                return option;
            }
        }

        return affordableOptions[^1];
    }

    private int GetMinProceduralCost(int levelNumber)
    {
        int minCost = int.MaxValue;
        foreach (EnemySpawnOption option in EnemyCatalog.GetProceduralSpawnOptions(levelNumber))
        {
            minCost = Math.Min(minCost, EnemyCatalog.GetCost(config, option.Type, option.AmmoDefinition.Id));
        }

        return minCost == int.MaxValue ? 0 : minCost;
    }
}
