using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Bullets.Components.Propellants;

public class HomingPropellant : AbstractComponent
{
    private Random random = Random.Shared;
    private float initialLockOnDelay = 0.2f;
    private float lockOnInterval = 0.3f;
    private float timer;
    private Body lockedOnTarget;
    private Vector2 randomizedLockOnOffset = Vector2.Zero;
    private bool hasSpedUp;
    
    private readonly Color searching = new Color(255, 255, 255, 1);
    private readonly Color homing = new Color(255, 0, 0, 1);
    
    public HomingPropellant()
    {
        Type = EComponentType.Propellant;
        ComponentId = ComponentIds.HomingPropellant;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Speed *= 0.5f;
        bulletEntity.Stats.Spread *= 1.5f;
        initialLockOnDelay += random.NextSingle() / 2f;
        randomizedLockOnOffset = new Vector2(random.NextSingle() * 50, random.NextSingle() * 50)
            - new Vector2(25f, 25f);
        randomizedLockOnOffset = randomizedLockOnOffset.ToMeters();
    }

    public override void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
        timer += deltaTime;

        if (timer >= initialLockOnDelay + lockOnInterval)
        {
            lockedOnTarget = LockOnTarget(bulletEntity);
            timer -= lockOnInterval;
        }

        var body = bulletEntity.PhysicsBody;
        
        float maxRotation = 2.8f;
        float angle = (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X);
        float maxRotationThisFrame = maxRotation * deltaTime;
        float speed = body.LinearVelocity.Length();
        if (lockedOnTarget != null)
        {
            Vector2 desiredDirection = lockedOnTarget.Position + randomizedLockOnOffset - body.Position;
            float targetAngle = (float)Math.Atan2(desiredDirection.Y, desiredDirection.X);
            float angleDifference = MathHelper.WrapAngle(targetAngle - angle);
            float rotationAmount = Math.Clamp(angleDifference, -maxRotationThisFrame, maxRotationThisFrame);
            angle += rotationAmount;

            if (!hasSpedUp)
            {
                speed *= 1.7f;
                hasSpedUp = true;
            }
        } 

        body.LinearVelocity = new Vector2(
            (float)Math.Cos(angle) * speed,
            (float)Math.Sin(angle) * speed
        );
    }

    public override void OnDraw(BulletEntity bulletEntity, SpriteBatch spriteBatch)
    {
        Vector2 bottomCenter = bulletEntity.Position + new Vector2(0, bulletEntity.Stats.Size / 2f);
        float renderDepth = RenderUtility.CalculateDepth(bottomCenter.Y) - 0.001f;
        Vector2 direction = Vector2.Normalize(bulletEntity.PhysicsBody.LinearVelocity);
        Vector2 position = bulletEntity.PhysicsBody.Position.ToPixels();
        const float indicatorLength = 80f;
        const float indicatorThickness = 1f;
        
        spriteBatch.DrawLine(
            position,
            position + Vector2.Rotate(direction, -Single.Pi/4) * indicatorLength,
            lockedOnTarget != null ? homing : searching,
            indicatorThickness,
            renderDepth
        );
        spriteBatch.DrawLine(
            position,
            position + Vector2.Rotate(direction, Single.Pi/4) * indicatorLength,
            lockedOnTarget != null ? homing : searching,
            indicatorThickness,
            renderDepth
        );
    }

    private Body LockOnTarget(BulletEntity bulletEntity)
    {
        Vector2 pos = bulletEntity.PhysicsBody.Position;
        Vector2 velocity = bulletEntity.PhysicsBody.LinearVelocity;

        if (velocity.LengthSquared() < 0.01f) return null;
        Vector2 forward = Vector2.Normalize(velocity);

        List<Body> hostileTargets = BulletTargetingHelper.GetHostileTargets(bulletEntity)
            .Where(body => IsWithinHomingArc(bulletEntity, pos, forward, body))
            .ToList();

        if (hostileTargets.Count == 0) return null;
        return BulletTargetingHelper.GetClosestHomingTargetToLine(bulletEntity, hostileTargets, pos,
            pos + (forward * 1000f));
    }

    private static bool IsWithinHomingArc(BulletEntity bulletEntity, Vector2 position, Vector2 forward, Body targetBody)
    {
        Vector2 toTarget = targetBody.Position - position;
        if (toTarget.LengthSquared() < 0.0001f)
        {
            return true;
        }

        float dot = Vector2.Dot(forward, Vector2.Normalize(toTarget));
        float minDot = BulletTargetingHelper.GetHomingTargetDotThreshold(bulletEntity.Faction, targetBody.Tag);
        return dot >= minDot;
    }
}