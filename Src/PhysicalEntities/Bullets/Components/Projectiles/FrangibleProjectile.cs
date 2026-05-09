using System;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Bullet;
using Gamelab.Utils;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class FrangibleProjectile : AbstractComponent
{
    private readonly Random random = Random.Shared;

    public FrangibleProjectile()
    {
        Type = EComponentType.Projectile;
        ComponentId = ComponentIds.FrangibleProjectile;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        if (!IsRootEffect)
        {
            bulletEntity.Stats.Spread *= 3.0f;
            bulletEntity.Stats.Damage *= 0.2f;
            bulletEntity.Stats.Speed += (random.NextSingle() * 0.4f - 0.2f) * bulletEntity.Stats.Speed;
        }
    }

    public override void OnHit(BulletEntity bulletEntity, IDamageable hitEntity)
    {
        if (!IsRootEffect) return;
        Vector2 hitPosition = bulletEntity.Position;
        Vector2 currentVelocity = bulletEntity.PhysicsBody.LinearVelocity;
        Vector2 currentDirection = currentVelocity.LengthSquared() > 0.01f
            ? Vector2.Normalize(currentVelocity)
            : bulletEntity.Stats.Direction;

        for (int i = 0; i < 3; i++)
        {
            bulletEntity.ChildTemplate.Stats.Position = bulletEntity.Position;
            bulletEntity.ChildTemplate.Stats.Direction = currentDirection;
            BulletEntity childBullet = GamelabGame.Instance.Services.GetService<IBulletService>().EmitAdditionalBullet(bulletEntity.ChildTemplate, this);
            childBullet?.AddHitCooldown(hitEntity, 1.0f);
        }

        IsRootEffect = false;
    }
}