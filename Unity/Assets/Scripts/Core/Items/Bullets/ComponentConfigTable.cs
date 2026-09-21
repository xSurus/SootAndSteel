using System.Collections.Generic;

namespace Gamelab.Items.Bullets
{
    // Generated from Src/Content/Data/ComponentConfig.json, file order.
    public static class ComponentConfigTable
    {
        public static ComponentRegistry Default()
        {
            return new ComponentRegistry(new[]
            {
                Entry("BasicProjectile", "Basic Projectile", "A simple projectile that moves in a straight line and can damage targets on contact.", false, 18),
                Entry("BasicPropellant", "Basic Propellant", "A basic gunpowder propellant that provides a moderate amount of thrust to projectiles.", false, 0),
                Entry("BasicCasing", "Basic Casing", "A standard casing that holds the propellant and projectile together.", false, 0),
                Entry("ScatterCasing", "Scatter Casing", "A casing that holds many small pellets, increasing the area of effect, but reducing damage per fragment.", true, 10),
                Entry("EnemyCasing", "Enemy Casing", "[Internal Only] A casing fired by enemies, which may have different properties such as speed, damage, or special effects.", false, 0),
                Entry("HomingPropellant", "Homing Propellant", "A propellant that allows the projectile to home in on targets, increasing accuracy but potentially reducing speed.", true, 10),
                Entry("FrangibleProjectile", "Frangible Projectile", "A projectile that scatters into multiple shards after hitting an enemy, increasing spread but decreasing damage per shard.", true, 10),
                Entry("HeavyPropellant", "Heavy Propellant", "A propellant that increases projectile damage and size while decreasing its speed", true, 10),
                Entry("PiercingProjectile", "Piercing Projectile", "A projectile that makes the bullet pierce through multiple enemies at full damage!", true, 10),
                Entry("BurstCasing", "Burst Casing", "A casing that shoots three times in a row. How is that even possible?", true, 10),
                Entry("BoomerangPropellant", "BOOMerang", "A propellant that makes the projectile return to where it came from. Maybe not the best idea", true, 10),
                Entry("RapidFireCasing", "Rapid Fire Casing", "A casing of 10 small pellets, fired over time and dealing reduced damage on hit. Great to overwhelm a large amount of enemies.", true, 10),
                Entry("MatryoshkaProjectile", "Matryoshka Projectile", "A Russian classic. This projectile releases smaller versions of itself on hit, hurdling towards the next enemy.", true, 10),
            });
        }

        private static KeyValuePair<string, ComponentConfig> Entry(string id, string name, string description, bool appearsInShop, int shopPrice)
        {
            return new KeyValuePair<string, ComponentConfig>(id, new ComponentConfig
            {
                Name = name,
                Description = description,
                AppearsInShop = appearsInShop,
                ShopPrice = shopPrice,
            });
        }
    }
}
