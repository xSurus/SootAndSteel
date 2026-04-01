using System;
using Gamelab.Particles;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

// <-- Add the namespace for your new service

namespace Gamelab.PhysicalEntities.Projectiles;

public class CannonProjectile : AbstractProjectile
{
    protected override Color ProjectileColor => Color.Cyan;
    private readonly ParticleEmitter trailEmitter;
    private readonly IVfxService vfxService = GamelabGame.Instance.Services.GetService<IVfxService>();

    public CannonProjectile(
        World world,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float maxLifetime,
        float size)
        : base(world, position, velocity, damage, maxLifetime, size)
    {
        trailEmitter = ParticleFactory.CreateCannonballTrail(new Random());
        vfxService?.AddContinuous(trailEmitter);
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        trailEmitter.Position = Position;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        trailEmitter.ShouldRemove = true;
    }
}