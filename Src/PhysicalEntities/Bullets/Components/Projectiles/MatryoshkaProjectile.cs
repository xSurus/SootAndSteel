using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Bullet;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class MatryoshkaProjectile : AbstractComponent
{
    public int Level = 3;
    public Body? Target = null;
    
    public MatryoshkaProjectile()
    {
        Type = EComponentType.Projectile;
        ComponentId = ComponentIds.MatryoshkaProjectile;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Spread *= 0.5f;
        bulletEntity.Stats.Speed *= 1.2f;
    }

    public override void OnSpawn(BulletEntity bulletEntity)
    {
        if (Level > 0) bulletEntity.Stats.Pierce = 0f;
        bulletEntity.Stats.Damage *= MathHelper.Lerp(0.3f, 2.0f, Level / 3f);
        bulletEntity.Stats.Size *= MathHelper.Lerp(0.5f, 1.3f, Level / 3f);

        if (Target != null && Target.Enabled)
        {
            Vector2 direction = new Vector2(
                Target.Position.X - bulletEntity.Position.ToMeters().X,
                Target.Position.Y - bulletEntity.Position.ToMeters().Y
            );
            direction.Normalize();
            float speed = bulletEntity.PhysicsBody.LinearVelocity.Length();
            bulletEntity.PhysicsBody.LinearVelocity = direction * speed;
        }
    }

    public override void OnHit(BulletEntity bulletEntity, IDamageable hitEntity)
    {
        if (Level > 0)
        {
            BulletEntity child = new BulletEntity(bulletEntity.ChildTemplate);
            child.Stats.Position = bulletEntity.Position;
            
            BulletEntity childBullet = GamelabGame.Instance.Services.GetService<IBulletService>()
                .EmitAdditionalBullet(child, this);
            
            ((MatryoshkaProjectile)childBullet.GetEffect(this)).Level = Level - 1;
            ((MatryoshkaProjectile)childBullet.GetEffect(this)).Target =
                BulletTargetingHelper.GetClosestHostileTarget(bulletEntity, hitEntity);
            
            childBullet?.AddHitCooldown(hitEntity, 1.0f);
        }
    }

}