using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Projectiles
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Projectiles/Frangible Projectile", fileName = "FrangibleProjectile")]
    public class FrangibleProjectileAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Projectile;
        public override string ComponentId => ComponentIds.FrangibleProjectile;

        private static readonly IRandomService RandomService = new RandomService();

        public override void OnCreate(BulletRuntime bullet)
        {
            if (bullet.IsRoot(this)) return;
            bullet.Stats.Spread *= 3.0f;
            bullet.Stats.Damage *= 0.2f;
            bullet.Stats.Speed += (RandomService.NextSingle() * 0.4f - 0.2f) * bullet.Stats.Speed;
        }

        public override void OnHit(BulletRuntime bullet, IDamageable hitEntity)
        {
            if (!bullet.IsRoot(this)) return;
            Vector2 velocity = bullet.PhysicsBody.linearVelocity;
            Vector2 direction = velocity.sqrMagnitude > 0.01f ? velocity.normalized : bullet.AimDirection;
            for (int i = 0; i < 3; i++)
            {
                bullet.SpawnChild(this, bullet.transform.position, direction)
                    .AddHitCooldown(hitEntity, 1.0f);
            }

            bullet.MarkNonRoot(this);
        }
    }
}
