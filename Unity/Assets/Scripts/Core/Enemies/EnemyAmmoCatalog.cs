using System;
using System.Collections.Generic;
using Gamelab.Items.Bullets;

namespace Gamelab.Enemies.Core
{
    /// <summary>Port of the ammo half of Src EnemyCatalog. Only EnemyType.Rifle spawns procedurally, as in Src.</summary>
    public static class EnemyAmmoCatalog
    {
        private static readonly EnemyAmmoDefinition[] ProceduralAmmoDefinitions =
        {
            Make(EnemyAmmoIds.Basic, null, 0, 1, 1.6f),
            Make(EnemyAmmoIds.Heavy, ComponentIds.HeavyPropellant, 1, 3, 0.8f),
            Make(EnemyAmmoIds.Scatter, ComponentIds.ScatterCasing, 1, 3, 0.8f),
            Make(EnemyAmmoIds.Frangible, ComponentIds.FrangibleProjectile, 1, 4, 0.7f),
            Make(EnemyAmmoIds.Burst, ComponentIds.BurstCasing, 2, 5, 0.55f),
            Make(EnemyAmmoIds.Piercing, ComponentIds.PiercingProjectile, 2, 5, 0.5f),
            Make(EnemyAmmoIds.RapidFire, ComponentIds.RapidFireCasing, 3, 6, 0.35f),
            Make(EnemyAmmoIds.Homing, ComponentIds.HomingPropellant, 3, 7, 0.3f),
            Make(EnemyAmmoIds.Matryoshka, ComponentIds.MatryoshkaProjectile, 4, 8, 0.2f)
        };

        // Every Src recipe is the basic four plus at most one extra component.
        private static EnemyAmmoDefinition Make(string id, string extra, int surcharge, int minLevel, float weight)
        {
            var ids = new List<string>
            {
                ComponentIds.BasicProjectile, ComponentIds.BasicCasing,
                ComponentIds.BasicPropellant, ComponentIds.EnemyCasing
            };
            if (extra != null) ids.Add(extra);
            return new EnemyAmmoDefinition(id, ids, surcharge, minLevel, weight);
        }

        public static IReadOnlyList<EnemyAmmoDefinition> All => ProceduralAmmoDefinitions;

        public static EnemyAmmoDefinition GetAmmoDefinition(string ammoId)
        {
            string requestedId = string.IsNullOrWhiteSpace(ammoId) ? EnemyAmmoIds.Basic : ammoId;
            foreach (EnemyAmmoDefinition def in ProceduralAmmoDefinitions)
            {
                if (def.Id == requestedId) return def;
            }

            throw new ArgumentOutOfRangeException(nameof(ammoId), ammoId, "Unknown enemy ammo id.");
        }

        public static IReadOnlyList<EnemySpawnOption> GetProceduralSpawnOptions(int levelNumber)
        {
            int currentLevel = Math.Max(1, levelNumber);
            var options = new List<EnemySpawnOption>();
            foreach (EnemyAmmoDefinition def in ProceduralAmmoDefinitions)
            {
                if (!def.IsUnlockedAtLevel(currentLevel) || def.ProceduralWeight <= 0f) continue;
                options.Add(new EnemySpawnOption(EnemyType.Rifle, def));
            }

            return options;
        }

        public static float GetProceduralWeight(EnemySpawnOption option, int levelNumber)
        {
            if (option.Type != EnemyType.Rifle)
            {
                throw new ArgumentOutOfRangeException(nameof(option), option.Type, "Unknown enemy type.");
            }

            return option.AmmoDefinition.ProceduralWeight * (1f + Math.Max(1, levelNumber) * 0.05f);
        }

        /// <summary>Src takes a LevelGenerationConfig (not ported), so the base cost (config.BaseCost) is passed directly.</summary>
        public static int GetCost(int baseCost, EnemyType type, string ammoId = EnemyAmmoIds.Basic)
        {
            if (type != EnemyType.Rifle)
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.");
            }

            return baseCost + GetAmmoDefinition(ammoId).AmmoSurcharge;
        }
    }
}
