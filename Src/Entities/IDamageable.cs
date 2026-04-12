using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Projectiles;

namespace Gamelab.Entities;

public interface IDamageable
{
    void TakeDamage(float damageAmount);
    bool OnHit(BulletEntity bulletEntity);
}