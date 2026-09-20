using System;

namespace Gamelab.Items.Bullets
{
    // Component type and IsBasic per id, matching Src/PhysicalEntities/Bullets/Components/*.
    // Only BasicCasing, BasicProjectile and BasicPropellant are basic.
    public static class ComponentTraits
    {
        public static EComponentType TypeOf(string id)
        {
            switch (id)
            {
                case ComponentIds.BasicProjectile:
                case ComponentIds.FrangibleProjectile:
                case ComponentIds.PiercingProjectile:
                case ComponentIds.MatryoshkaProjectile:
                    return EComponentType.Projectile;
                case ComponentIds.BasicCasing:
                case ComponentIds.ScatterCasing:
                case ComponentIds.BurstCasing:
                case ComponentIds.EnemyCasing:
                case ComponentIds.RapidFireCasing:
                    return EComponentType.Casing;
                case ComponentIds.BasicPropellant:
                case ComponentIds.HomingPropellant:
                case ComponentIds.HeavyPropellant:
                case ComponentIds.BoomerangPropellant:
                    return EComponentType.Propellant;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown component id");
            }
        }

        public static bool IsBasic(string id)
        {
            TypeOf(id); // validates the id
            return id == ComponentIds.BasicProjectile
                || id == ComponentIds.BasicCasing
                || id == ComponentIds.BasicPropellant;
        }
    }
}
