using System;
using Gamelab.Items.Bullets;
using Gamelab.Services.Bullet;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class ScatterCasing : AbstractComponent
{
    private readonly Random random = Random.Shared;

    public ScatterCasing()
    {
        Type = EComponentType.Casing;
        ComponentId = ComponentIds.ScatterCasing;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Spread *= 3.0f;
        bulletEntity.Stats.Damage *= 0.2f;
        bulletEntity.Stats.Speed += (random.NextSingle() * 0.4f - 0.2f) * bulletEntity.Stats.Speed;
    }

    public override void OnSpawn(BulletEntity bulletEntity)
    {
        if (IsRootEffect)
        {
            for (int i = 0; i < 10; i++)
            {
                GamelabGame.Instance.Services.GetService<IBulletService>().EmitAdditionalBullet(bulletEntity.ChildTemplate, this);
            }

            bulletEntity.IsActive = false;
        }
    }
}