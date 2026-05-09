using System;
using Gamelab.Assets;
using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Bullets.Components.Propellants;

public class BasicPropellant : AbstractComponent
{
    private Random random = Random.Shared;

    public BasicPropellant()
    {
        Type = EComponentType.Propellant;
        IsBasic = true;
        ComponentId = ComponentIds.BasicPropellant;
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
        bulletEntity.PhysicsBody.Enabled = false;
    }

    public override void OnSpawn(BulletEntity bulletEntity)
    {
        bulletEntity.PhysicsBody.Enabled = true;
        IRandomService randomService = GamelabGame.Instance.Services.GetService<IRandomService>();
        float randomSpread = (float)randomService.SampleGaussian(0, bulletEntity.Stats.Spread / 3f);
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

    public override void OnDraw(BulletEntity bulletEntity, SpriteBatch spriteBatch)
    {
        Vector2 bottomCenter = bulletEntity.Position + new Vector2(0, bulletEntity.Stats.Size / 2f);
        float renderDepth = RenderUtility.CalculateDepth(bottomCenter.Y);
        Vector2 origin = new Vector2(0.5f, 1f);

        spriteBatch.Draw(
            texture: AssetManager.BlankTexture,
            position: bottomCenter,
            sourceRectangle: null,
            color: bulletEntity.Stats.Color,
            rotation: 0f,
            origin: origin,
            scale: new Vector2(bulletEntity.Stats.Size, bulletEntity.Stats.Size),
            effects: SpriteEffects.None,
            layerDepth: renderDepth
        );
    }
}