using System;
using Gamelab.Services.Random;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Bullets.Components.Propellants;

public class BasicPropellant : AbstractComponent
{
    private Random random = Random.Shared;
    
    public BasicPropellant()
    {
        Type = EComponentType.Propellant;
        IsBasic = true;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.PhysicsBody = bulletEntity.World.CreateCircle(
            (bulletEntity.Stats.Size / 2f).ToMeters(), 
            1f, 
            bulletEntity.Stats.Position.ToMeters(), 
            BodyType.Dynamic);
        bulletEntity.PhysicsBody.IgnoreGravity = true;
        bulletEntity.PhysicsBody.IsBullet = true;
        bulletEntity.PhysicsBody.FixedRotation = true;
        
        IRandomService randomService = GamelabGame.Instance.Services.GetService<IRandomService>();
        float randomSpread = (float) randomService.SampleGaussian(0, bulletEntity.Stats.Spread / 3f);
        Vector2 directionWithSpread = Vector2.Rotate(bulletEntity.Stats.Direction, randomSpread);
        bulletEntity.PhysicsBody.LinearVelocity = directionWithSpread * bulletEntity.Stats.Speed.ToMeters();
    }

    public override void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
        if (!bulletEntity.IsActive) return;
        bulletEntity.Age += deltaTime;
        if (bulletEntity.Age > bulletEntity.Stats.Lifetime)
        {
            bulletEntity.IsActive = false;
        }
    }
}