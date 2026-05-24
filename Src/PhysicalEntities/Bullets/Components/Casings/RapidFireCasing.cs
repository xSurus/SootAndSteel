using System;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class RapidFireCasing : AbstractComponent
{
    private readonly ISoundService soundService;
    private Random random = Random.Shared;
    private bool playSoundOnSpawn = false;
    
    public RapidFireCasing()
    {
        Type = EComponentType.Casing;
        ComponentId = ComponentIds.RapidFireCasing;
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.CannonFire);
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Spread *= 5.0f;
        bulletEntity.Stats.Damage *= 0.25f;
    }

    public override void OnSpawn(BulletEntity bulletEntity)
    {
        if (!IsRootEffect)
        {
            if (playSoundOnSpawn) soundService.PlayOnce((bulletEntity.InitialShooter is CannonStation or CannonSlot) ? Sounds.CannonFire : Sounds.EnemyFire);
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            BulletEntity child = GamelabGame.Instance.Services.GetService<IBulletService>()
                .EmitAdditionalBullet(bulletEntity.ChildTemplate, this, i * 0.1f);
            ((RapidFireCasing)child.GetEffect(this)).playSoundOnSpawn = true;
        }

        bulletEntity.IsActive = false;
    }
}