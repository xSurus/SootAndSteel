using System.Collections.Generic;
using Gamelab.Items.Bullets;
using Gamelab.Particles;
using Gamelab.Services.Vfx;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class BasicCasing : AbstractComponent
{
    private readonly IVfxService vfxService = GamelabGame.Instance.Services.GetService<IVfxService>();
    private Dictionary<BulletEntity, ParticleEmitter> trailEmitter = new();

    public BasicCasing()
    {
        Type = EComponentType.Casing;
        IsBasic = true;
        ComponentId = ComponentIds.BasicCasing;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        trailEmitter[bulletEntity] = ParticleFactory.CreateCannonballTrail();
        vfxService?.AddContinuous(trailEmitter[bulletEntity]);
    }

    public override void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
        trailEmitter[bulletEntity].Position = bulletEntity.Position;
    }

    public override void OnCleanup(BulletEntity bulletEntity)
    {
        trailEmitter[bulletEntity].ShouldRemove = true;
        trailEmitter.Remove(bulletEntity);
    }
}