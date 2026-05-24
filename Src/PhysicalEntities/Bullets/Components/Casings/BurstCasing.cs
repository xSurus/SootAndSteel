using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class BurstCasing : AbstractComponent
{
    private readonly ISoundService soundService;
    private readonly float burstInterval = 0.15f;
    private bool playSoundOnSpawn = false;
    
    public BurstCasing()
    {
        Type = EComponentType.Casing;
        ComponentId = ComponentIds.BurstCasing;
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.CannonFire);
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Damage *= 0.5f;
    }

    public override void OnSpawn(BulletEntity bulletEntity)
    {
        if (!IsRootEffect)
        {
            if(playSoundOnSpawn) soundService.PlayOnce((bulletEntity.InitialShooter is CannonStation or CannonSlot) ? Sounds.CannonFire : Sounds.EnemyFire);
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            BulletEntity child = GamelabGame.Instance.Services.GetService<IBulletService>().EmitAdditionalBullet(
                bulletEntity.ChildTemplate, this, i * burstInterval);
            ((BurstCasing)child.GetEffect(this)).playSoundOnSpawn = true;
            bulletEntity.IsActive = false;
        }
    }
}