using System;
using Gamelab.Items.Bullets;
using Gamelab.Services.Bullet;

namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class ScatterProjectile : AbstractComponent
{
    private readonly Random random = Random.Shared;

    public ScatterProjectile()
    {
        Type = EComponentType.Projectile;
        ComponentId = ComponentIds.ScatterProjectile;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Spread *= 3.0f;
        bulletEntity.Stats.Damage *= 0.2f;
        bulletEntity.Stats.Speed += (random.NextSingle() * 0.4f - 0.2f) * bulletEntity.Stats.Speed;

        if (bulletEntity.IsRootEntity)
        {
            for (int i = 0; i < 19; i++)
            {
                GamelabGame.Instance.Services.GetService<IBulletService>().EmitAdditionalBullet(
                    bulletEntity.Item,
                    bulletEntity.Stats.Position,
                    bulletEntity.Stats.Direction,
                    bulletEntity.Owner);
            }
        }
    }
}