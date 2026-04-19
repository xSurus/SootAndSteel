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
            ComponentIds.HomingCasing => new HomingCasing(),

            _ => throw new ArgumentOutOfRangeException(nameof(componentId), componentId,
                $"Unknown component id '{componentId}'.")
        };
    }
}