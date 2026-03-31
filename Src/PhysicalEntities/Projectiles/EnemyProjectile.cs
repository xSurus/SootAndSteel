using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Projectiles;

public class EnemyProjectile(
    World world,
    Vector2 position,
    Vector2 velocity,
    float damage,
    float maxLifetime,
    float size)
    : AbstractProjectile(world, position, velocity, damage, maxLifetime, size)
{
    protected override Color ProjectileColor => Color.Orange;

}