using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Enemies;
using Gamelab.Items.Bullets;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class HomingCasing : AbstractComponent
{
    public HomingCasing()
    {
        Type = EComponentType.Casing;
        ComponentId = ComponentIds.HomingCasing;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Speed *= 0.5f;
        bulletEntity.Stats.Spread *= 1.5f;
    }

    public override void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
        var body = bulletEntity.PhysicsBody;
        var target = LockOnEnemy(bulletEntity);

        if (target == null) return;

        float maxRotation = 1.5f;

        Vector2 desiredDirection = target.Position - body.Position;
        float targetAngle = (float)Math.Atan2(desiredDirection.Y, desiredDirection.X);
        float currentAngle = (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X);
        float angleDifference = MathHelper.WrapAngle(targetAngle - currentAngle);
        float maxRotationThisFrame = maxRotation * deltaTime;
        float rotationAmount = Math.Clamp(angleDifference, -maxRotationThisFrame, maxRotationThisFrame);
        float newAngle = currentAngle + rotationAmount;
        float currentSpeed = body.LinearVelocity.Length();
        body.LinearVelocity = new Vector2(
            (float)Math.Cos(newAngle) * currentSpeed,
            (float)Math.Sin(newAngle) * currentSpeed
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