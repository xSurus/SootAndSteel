using System;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Bullets.Components.Casings;
using Gamelab.PhysicalEntities.Bullets.Components.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Components.Propellants;

namespace Gamelab.Items.Bullets;

public static class ComponentFactory
{
    public static AbstractComponent CreateDefinition(string componentId)
    {
        return componentId switch
        {
            ComponentIds.BasicCasing => new BasicCasing(),
            ComponentIds.BasicProjectile => new BasicProjectile(),
            ComponentIds.BasicPropellant => new BasicPropellant(),
            ComponentIds.ScatterProjectile => new ScatterProjectile(),
            ComponentIds.EnemyProjectile => new EnemyProjectile(),
            ComponentIds.HomingPropellant => new HomingPropellant(),
            ComponentIds.ScatterCasing => new ScatterCasing(),
            ComponentIds.HeavyPropellant => new HeavyPropellant(),
            ComponentIds.PiercingCasing => new PiercingCasing(),
            ComponentIds.BurstProjectile => new BurstProjectile(),
            ComponentIds.BoomerangPropellant => new BoomerangPropellant(),

            _ => throw new ArgumentOutOfRangeException(nameof(componentId), componentId,
                $"Unknown component id '{componentId}'.")
        };
    }
}