using System;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Bullet;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class ScatterCasingEmitter : IBulletEmitter
{
}

public class ScatterCasing : AbstractComponent
{
    private readonly Random random = Random.Shared;
    private static readonly ScatterCasingEmitter CasingEmitter = new ScatterCasingEmitter();

    public ScatterCasing()
    {
        Type = EComponentType.Casing;
        ComponentId = "ScatterCasing";
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        if (bulletEntity.DirectEmitter is ScatterCasingEmitter)
        {
            bulletEntity.Stats.Spread *= 3.0f;
            bulletEntity.Stats.Damage *= 0.2f;
            bulletEntity.Stats.Speed += (random.NextSingle() * 0.4f - 0.2f) * bulletEntity.Stats.Speed;
        }
    }

    public override void OnHit(BulletEntity bulletEntity, IDamageable hitEntity)
    {
        if (bulletEntity.DirectEmitter is ScatterCasingEmitter) return;
        Vector2 hitPosition = bulletEntity.Position;
        Vector2 currentVelocity = bulletEntity.PhysicsBody.LinearVelocity;
        Vector2 currentDirection = currentVelocity.LengthSquared() > 0.01f
            ? Vector2.Normalize(currentVelocity)
            : bulletEntity.Stats.Direction;

        for (int i = 0; i < 19; i++)
        {
            GamelabGame.Instance.Services.GetService<IBulletService>().EmitAdditionalBullet(
                bulletEntity.Item,
                hitPosition,
                currentDirection,
                bulletEntity.InitialShooter,
                CasingEmitter);
        }
    }
}