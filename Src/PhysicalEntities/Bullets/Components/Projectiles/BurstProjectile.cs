using Gamelab.Items.Bullets;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;

namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class BurstProjectileEmitter : IBulletEmitter
{
}

public class BurstProjectile : AbstractComponent
{
    private static readonly BurstProjectileEmitter BurstEmitter = new BurstProjectileEmitter();
    private ISoundService soundService;

    private int burstsRemaining = 2;
    private float burstTimer = 0f;
    private readonly float burstInterval = 0.15f;

    public BurstProjectile()
    {
        Type = EComponentType.Projectile;
        ComponentId = ComponentIds.BurstProjectile;
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.CannonFire);
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Damage *= 0.5f;
    }

    public override void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
        if (bulletEntity.DirectEmitter is BurstProjectileEmitter) return;
        if (burstsRemaining <= 0) return;
        burstTimer += deltaTime;
        if (burstTimer >= burstInterval)
        {
            burstTimer = 0f;
            burstsRemaining--;
            GamelabGame.Instance.Services.GetService<IBulletService>().EmitAdditionalBullet(
                bulletEntity.Item,
                bulletEntity.Stats.Position,
                bulletEntity.Stats.Direction,
                bulletEntity.InitialShooter,
                BurstEmitter
            );
            soundService.PlayOnce(Sounds.CannonFire);
        }
    }
}