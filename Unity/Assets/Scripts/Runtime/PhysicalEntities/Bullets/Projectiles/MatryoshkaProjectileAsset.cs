using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Projectiles
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Projectiles/Matryoshka Projectile", fileName = "MatryoshkaProjectile")]
    public class MatryoshkaProjectileAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Projectile;
        public override string ComponentId => ComponentIds.MatryoshkaProjectile;

        public class MatryoshkaState
        {
            public int Level = 3;
            public Rigidbody2D Target;
        }

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Spread *= 0.5f;
            bullet.Stats.Speed *= 1.2f;
        }

        public override void OnSpawn(BulletRuntime bullet)
        {
            var state = bullet.GetState<MatryoshkaState>(this);
            if (state.Level > 0) bullet.Stats.Pierce = 0f;
            bullet.Stats.Damage *= Mathf.Lerp(0.3f, 2.0f, state.Level / 3f);
            bullet.Stats.Size *= Mathf.Lerp(0.5f, 1.3f, state.Level / 3f);

            if (state.Target != null && state.Target.simulated)
            {
                Vector2 toTarget = state.Target.position - bullet.PhysicsBody.position;
                if (toTarget.sqrMagnitude > 0f)
                {
                    float speed = bullet.PhysicsBody.linearVelocity.magnitude;
                    bullet.PhysicsBody.linearVelocity = toTarget.normalized * speed;
                }
            }
        }

        public override void OnHit(BulletRuntime bullet, IDamageable hitEntity)
        {
            int level = bullet.GetState<MatryoshkaState>(this).Level;
            if (level <= 0) return;

            Rigidbody2D target = BulletTargeting.GetClosestHostileTarget(bullet, hitEntity);
            BulletRuntime child = bullet.SpawnChild(this, bullet.PhysicsBody.position, bullet.AimDirection, 0f, c =>
            {
                var s = c.GetState<MatryoshkaState>(this);
                s.Level = level - 1;
                s.Target = target;
            });
            child.AddHitCooldown(hitEntity, 1f);
        }
    }
}
