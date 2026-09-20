using System;
using System.Collections.Generic;
using Gamelab.Enemies.Core;

namespace Gamelab.Levels
{
    /// <summary>Faithful port of Src ProceduralLevelGenerator. RNG call order must stay identical.</summary>
    public class ProceduralLevelGenerator
    {
        private readonly LevelGenerationConfig config;
        private readonly float threatScale = 1f;
        private readonly float spawnSpacingScale = 1f;

        public ProceduralLevelGenerator(LevelGenerationConfig config)
        {
            this.config = config ?? new LevelGenerationConfig();
        }

        public ProceduralLevelGenerator(LevelGenerationConfig config, float threatScale, float spawnSpacingScale)
            : this(config)
        {
            this.threatScale = Math.Max(1f, threatScale);
            this.spawnSpacingScale = Math.Min(1f, Math.Max(0.4f, spawnSpacingScale));
        }

        /// <summary>Builds a level definition: a distance target plus spawn events spread across it.</summary>
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

            var random = new Random(levelSeed);
            float levelEndBuffer = config.SafeZoneDistance;
            float maxSpawnDistance = MathF.Max(config.MinSpawnSpacing, currentLevelDistance - levelEndBuffer);
            float minGap = config.MinSpawnSpacing * spawnSpacingScale;

            var distances = new List<float>();

            // Same as Src: this rejection loop has no iteration cap. It never ends if the remaining budget
            // wants more events than fit in [safeZone, maxSpawnDistance] at minGap spacing (or if that range is
            // empty). Not reachable with default config; kept unguarded so RNG order and outputs match Src.
            while (budget >= minCost)
            {
                float spawnDistance = NextSingle(random) * (maxSpawnDistance - safeZoneDistance) + safeZoneDistance;
                if (IsTooClose(distances, spawnDistance, minGap))
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
                budget -= EnemyAmmoCatalog.GetCost(config.BaseCost, option.Type, option.AmmoDefinition.Id);

                definition.SpawnEvents.Add(new SpawnEvent
                {
                    Distance = spawnDistance,
                    Type = new EnemyDefinition(option.Type).Id,
                    AmmoId = option.AmmoDefinition.Id,
                    Side = NextSingle(random) < 0.5f ? "Top" : "Bottom"
                });
            }

            definition.SpawnEvents.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            return definition;
        }

        // Equals .NET's Random.NextSingle() for seeded instances.
        private static float NextSingle(Random random) => (float)random.NextDouble();

        private static bool IsTooClose(List<float> distances, float value, float minGap)
        {
            foreach (float d in distances)
            {
                if (MathF.Abs(d - value) < minGap) return true;
            }

            return false;
        }

        private EnemySpawnOption? SelectSpawnOption(Random random, int levelNumber, int budget)
        {
            IReadOnlyList<EnemySpawnOption> candidateOptions = EnemyAmmoCatalog.GetProceduralSpawnOptions(levelNumber);
            var affordableOptions = new List<EnemySpawnOption>();
            float totalWeight = 0f;

            foreach (EnemySpawnOption option in candidateOptions)
            {
                int cost = EnemyAmmoCatalog.GetCost(config.BaseCost, option.Type, option.AmmoDefinition.Id);
                if (budget < cost)
                {
                    continue;
                }

                float weight = EnemyAmmoCatalog.GetProceduralWeight(option, levelNumber);
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

            float roll = NextSingle(random) * totalWeight;
            foreach (EnemySpawnOption option in affordableOptions)
            {
                roll -= EnemyAmmoCatalog.GetProceduralWeight(option, levelNumber);
                if (roll <= 0f)
                {
                    return option;
                }
            }

            return affordableOptions[affordableOptions.Count - 1];
        }

        private int GetMinProceduralCost(int levelNumber)
        {
            int minCost = int.MaxValue;
            foreach (EnemySpawnOption option in EnemyAmmoCatalog.GetProceduralSpawnOptions(levelNumber))
            {
                minCost = Math.Min(minCost, EnemyAmmoCatalog.GetCost(config.BaseCost, option.Type, option.AmmoDefinition.Id));
            }

            return minCost == int.MaxValue ? 0 : minCost;
        }
    }
}
