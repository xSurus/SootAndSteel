using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.Levels;

namespace Gamelab.Enemies.Core;

public static class EnemyCatalog
{
    private static readonly EnemyAmmoDefinition[] ProceduralAmmoDefinitions =
    [
        new(
            EnemyAmmoIds.Basic,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing],
            ammoSurcharge: 0,
            minLevel: 1,
            proceduralWeight: 1.6f),
        new(
            EnemyAmmoIds.Heavy,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.HeavyPropellant],
            ammoSurcharge: 1,
            minLevel: 3,
            proceduralWeight: 0.8f),
        new(
            EnemyAmmoIds.Scatter,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.ScatterCasing],
            ammoSurcharge: 1,
            minLevel: 3,
            proceduralWeight: 0.8f),
        new(
            EnemyAmmoIds.Frangible,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.FrangibleProjectile],
            ammoSurcharge: 1,
            minLevel: 4,
            proceduralWeight: 0.7f),
        new(
            EnemyAmmoIds.Burst,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.BurstCasing],
            ammoSurcharge: 2,
            minLevel: 5,
            proceduralWeight: 0.55f),
        new(
            EnemyAmmoIds.Piercing,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.PiercingProjectile],
            ammoSurcharge: 2,
            minLevel: 5,
            proceduralWeight: 0.5f),
        new(
            EnemyAmmoIds.RapidFire,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.RapidFireCasing],
            ammoSurcharge: 3,
            minLevel: 6,
            proceduralWeight: 0.35f),
        new(
            EnemyAmmoIds.Homing,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.HomingPropellant],
            ammoSurcharge: 3,
            minLevel: 7,
            proceduralWeight: 0.3f),
        new(
            EnemyAmmoIds.Matryoshka,
            [ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.EnemyCasing, ComponentIds.MatryoshkaProjectile],
            ammoSurcharge: 4,
            minLevel: 8,
            proceduralWeight: 0.2f)
    ];

    public static EnemyAmmoDefinition GetAmmoDefinition(string ammoId)
    {
        string requestedId = string.IsNullOrWhiteSpace(ammoId) ? EnemyAmmoIds.Basic : ammoId;
        EnemyAmmoDefinition definition = ProceduralAmmoDefinitions.FirstOrDefault(def => def.Id == requestedId);
        if (definition == null)
        {
            throw new ArgumentOutOfRangeException(nameof(ammoId), ammoId, "Unknown enemy ammo id.");
        }

        return definition;
    }

    public static IReadOnlyList<EnemySpawnOption> GetProceduralSpawnOptions(int levelNumber)
    {
        int currentLevel = Math.Max(1, levelNumber);
        List<EnemySpawnOption> options = [];

        foreach (EnemyAmmoDefinition ammoDefinition in ProceduralAmmoDefinitions)
        {
            if (!ammoDefinition.IsUnlockedAtLevel(currentLevel) || ammoDefinition.ProceduralWeight <= 0f)
            {
                continue;
            }

            options.Add(new EnemySpawnOption(EnemyType.Rifle, ammoDefinition));
        }

        return options;
    }

    public static float GetProceduralWeight(EnemySpawnOption option, int levelNumber)
    {
        if (option.Type != EnemyType.Rifle)
        {
            throw new ArgumentOutOfRangeException(nameof(option), option.Type, "Unknown enemy type.");
        }

        int currentLevel = Math.Max(1, levelNumber);
        float levelWeightBonus = 1f + currentLevel * 0.05f;
        return option.AmmoDefinition.ProceduralWeight * levelWeightBonus;
    }

    public static int GetCost(LevelGenerationConfig config, EnemyType type, string ammoId = EnemyAmmoIds.Basic)
    {
        if (type != EnemyType.Rifle)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.");
        }

        EnemyAmmoDefinition ammoDefinition = GetAmmoDefinition(ammoId);
        return config.BaseCost + ammoDefinition.AmmoSurcharge;
    }
}
