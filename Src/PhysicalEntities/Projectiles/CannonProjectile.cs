using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Projectiles;

public class CannonProjectile : AbstractProjectile
{
    protected override Color ProjectileColor => Color.Cyan;

    public CannonProjectile(
        World world,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float maxLifetime,
        float size)
        : base(world, position, velocity, damage, maxLifetime, size)
    {
    }
}
