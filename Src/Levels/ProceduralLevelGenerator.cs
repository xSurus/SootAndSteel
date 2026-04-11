using System;
using System.Collections.Generic;
using Gamelab.Enemies;

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
        int minCost = GetMinProceduralCost(safeLevel);
        int budget = Math.Max(minCost, (int)MathF.Round(floatBudget));

        var definition = new LevelDefinition
        {
            LevelDistance = levelDistance
        };

        var random = new Random(unchecked(config.RandomSeed + safeLevel * 7919));
        float cursorDistance = config.MinSpawnSpacing;
        float levelEndBuffer = MathF.Max(config.MinSpawnSpacing, 200f);
        float maxSpawnDistance = MathF.Max(config.MinSpawnSpacing, levelDistance - levelEndBuffer);

        while (budget >= minCost && cursorDistance <= maxSpawnDistance)
        {
            EnemyType? selectedType = SelectEnemyType(random, safeLevel, budget);
            if (selectedType == null)
            {
                break;
            }

            EnemyType enemyType = selectedType.Value;
            budget -= EnemyCatalog.GetCost(config, enemyType);

            definition.SpawnEvents.Add(new SpawnEvent
            {
                Distance = cursorDistance,
                Type = new EnemyDefinition(enemyType).Id,
                Side = random.NextSingle() < 0.5f ? "Top" : "Bottom"
            });

            float spacing = Lerp(config.MaxSpawnSpacing, config.MinSpawnSpacing, MathF.Min(1f, safeLevel / 20f));
            spacing *= 0.8f + random.NextSingle() * 0.4f;
            cursorDistance += MathF.Max(config.MinSpawnSpacing, spacing);
        }

        return definition;
    }

    private EnemyType? SelectEnemyType(Random random, int levelNumber, int budget)
    {
        IReadOnlyList<EnemyType> candidateTypes = EnemyCatalog.GetProceduralTypesForLevel(levelNumber);
        bool canSpawnRifle = budget >= EnemyCatalog.GetCost(config, EnemyType.Rifle) && HasType(candidateTypes, EnemyType.Rifle);
        bool canSpawnMounter = budget >= EnemyCatalog.GetCost(config, EnemyType.Mounter) && HasType(candidateTypes, EnemyType.Mounter);
        bool canSpawnMolotov = budget >= EnemyCatalog.GetCost(config, EnemyType.Molotov) && HasType(candidateTypes, EnemyType.Molotov);
        bool canSpawnTarThrower = budget >= EnemyCatalog.GetCost(config, EnemyType.TarThrower) && HasType(candidateTypes, EnemyType.TarThrower);

        if (!canSpawnRifle && !canSpawnMounter && !canSpawnMolotov && !canSpawnTarThrower)
        {
            return null;
        }

        if (canSpawnTarThrower && levelNumber >= 2 && random.NextSingle() < 0.15f)
        {
            return EnemyType.TarThrower;
        }

        if (canSpawnMolotov && levelNumber >= 2 && random.NextSingle() < 0.2f)
        {
            return EnemyType.Molotov;
        }

        if (canSpawnRifle && (!canSpawnMounter || IsRifleRoll(random, levelNumber)))
        {
            return EnemyType.Rifle;
        }

        if (canSpawnMounter)
        {
            return EnemyType.Mounter;
        }

        if (canSpawnMolotov)
        {
            return EnemyType.Molotov;
        }

        return EnemyType.TarThrower;
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

    private bool IsRifleRoll(Random random, int levelNumber)
    {
        float chance = config.BaseRifleChance + (levelNumber - 1) * config.RifleChanceIncreasePerLevel;
        chance = Math.Clamp(chance, 0f, config.MaxRifleChance);
        return random.NextSingle() < chance;
    }

    private static bool HasType(IReadOnlyList<EnemyType> types, EnemyType type)
    {
        foreach (EnemyType candidate in types)
        {
            if (candidate == type)
            {
                return true;
            }
        }

        return false;
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
