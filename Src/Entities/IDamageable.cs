using Gamelab.PhysicalEntities.Projectiles;

namespace Gamelab.Entities;

public interface IDamageable
{
    void TakeDamage(float damageAmount);
    void OnHit(AbstractProjectile projectile);
}