using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Projectiles;

namespace Gamelab.PhysicalEntities;

public interface IDamageable
{
    void TakeDamage(float damageAmount);
    bool OnHit(BulletEntity bulletEntity);
}