using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Enemies.Core;
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
    private float initialLockOnDelay = 0.5f;
    private float lockOnInterval = 0.3f;
    private float timer;
    private Body lockedOnEnemy;
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
            lockedOnEnemy = LockOnEnemy(bulletEntity);
            timer -= lockOnInterval;
        }

        var body = bulletEntity.PhysicsBody;
        
        float maxRotation = 1.8f;
        float angle = (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X);
        float maxRotationThisFrame = maxRotation * deltaTime;
        float speed = body.LinearVelocity.Length();
        if (lockedOnEnemy != null)
        {
            Vector2 desiredDirection = lockedOnEnemy.Position + randomizedLockOnOffset - body.Position;
            float targetAngle = (float)Math.Atan2(desiredDirection.Y, desiredDirection.X);
            float angleDifference = MathHelper.WrapAngle(targetAngle - angle);
            float rotationAmount = Math.Clamp(angleDifference, -maxRotationThisFrame, maxRotationThisFrame);
            angle += rotationAmount;

            if (!hasSpedUp)
            {
                speed *= 2f;
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
        
        spriteBatch.DrawLine(
            position,
            position + Vector2.Rotate(direction, -Single.Pi/4) * 300,
            lockedOnEnemy != null ? homing : searching,
            2f,
            renderDepth
        );
        spriteBatch.DrawLine(
            position,
            position + Vector2.Rotate(direction, Single.Pi/4) * 300,
            lockedOnEnemy != null ? homing : searching,
            2f,
            renderDepth
        );
    }

    private Body LockOnEnemy(BulletEntity bulletEntity)
    {
        Vector2 pos = bulletEntity.PhysicsBody.Position;
        Vector2 velocity = bulletEntity.PhysicsBody.LinearVelocity;

        if (velocity.LengthSquared() < 0.01f) return null;
        Vector2 forward = Vector2.Normalize(velocity);

        List<Body> enemies = bulletEntity.World?.BodyList
            .Where(b => b.Tag is AbstractEnemy && b != bulletEntity.PhysicsBody)
            .Where(b =>
            {
                Vector2 toEnemy = b.Position - pos;
                float dot = Vector2.Dot(forward, Vector2.Normalize(toEnemy));
                return dot > 0.5f;
            })
            .ToList() ?? new List<Body>();

        return GetClosestEnemyToLine(pos, pos + (forward * 1000f), enemies);
    }

    private Body GetClosestEnemyToLine(Vector2 lineStart, Vector2 lineEnd, List<Body> enemies)
    {
        if (enemies.Count == 0) return null;
        return enemies
            .Select(enemy => new
            {
                Body = enemy,
                Dist = GetDistanceToLine(lineStart, lineEnd, enemy.Position)
            })
            .OrderBy(item => item.Dist)
            .FirstOrDefault()?.Body;
    }

    private float GetDistanceToLine(Vector2 a, Vector2 b, Vector2 p)
    {
        // Direction of the line
        Vector2 dir = b - a;
        float lengthSquared = dir.LengthSquared();

        if (lengthSquared == 0) return Vector2.Distance(p, a);

        // Calculate the projection to ensure the enemy is IN FRONT of the bullet
        float t = Vector2.Dot(p - a, dir) / lengthSquared;

        // If t < 0, the enemy is behind the bullet. We return a huge distance.
        if (t < 0) return float.MaxValue;

        // Closest point on the line segment
        Vector2 projection = a + t * dir;
        return Vector2.Distance(p, projection);
    }
}