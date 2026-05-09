using System;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Bullets.Components.Casings;
using Gamelab.PhysicalEntities.Bullets.Components.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Components.Propellants;
using BasicCasing = Gamelab.PhysicalEntities.Bullets.Components.Casings.BasicCasing;
using ScatterCasing = Gamelab.PhysicalEntities.Bullets.Components.Casings.ScatterCasing;

namespace Gamelab.Items.Bullets;

public static class ComponentFactory
{
    public static AbstractComponent CreateDefinition(string componentId)
    {
        return componentId switch
        {
            // Projectiles
            ComponentIds.BasicProjectile => new BasicProjectile(),
            ComponentIds.FrangibleProjectile => new FrangibleProjectile(),
            ComponentIds.PiercingProjectile => new PiercingProjectile(),
            ComponentIds.MatryoshkaProjectile => new MatryoshkaProjectile(),
            // Casings
            ComponentIds.BasicCasing => new BasicCasing(),
            ComponentIds.ScatterCasing => new ScatterCasing(),
            ComponentIds.EnemyCasing => new EnemyCasing(),
            ComponentIds.BurstCasing => new BurstCasing(),
            ComponentIds.RapidFireCasing => new RapidFireCasing(),
            // Propellants
            ComponentIds.BasicPropellant => new BasicPropellant(),
            ComponentIds.HomingPropellant => new HomingPropellant(),
            ComponentIds.HeavyPropellant => new HeavyPropellant(),
            ComponentIds.BoomerangPropellant => new BoomerangPropellant(),

            _ => throw new ArgumentOutOfRangeException(nameof(componentId), componentId,
                $"Unknown component id '{componentId}'.")
        };
    }
}