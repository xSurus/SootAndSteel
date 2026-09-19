using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float damageAmount);
        bool OnHit(BulletRuntime bullet);
    }
}
