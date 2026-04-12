using System;
using Gamelab.Services.Bullet;

namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class ScatterProjectile : AbstractComponent
{
    private readonly Random random = Random.Shared;
    
    public ScatterProjectile()
    {
        Type = EComponentType.Projectile;
    }
    
    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Spread *= 2.0f;
        bulletEntity.Stats.Damage *= 0.1f;
        bulletEntity.Stats.Speed += random.Next(-150, 151);
        
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