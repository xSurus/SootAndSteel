using System.Collections.Generic;
using System.Linq;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Bullets;

public static class BulletTargetingHelper
{
    public static IEnumerable<Body> GetHostileTargets(BulletEntity bulletEntity, IDamageable excludedTarget = null)
    {
        if (bulletEntity.World == null)
        {
            return Enumerable.Empty<Body>();
        }

        return bulletEntity.World.BodyList
            .Where(body => body != bulletEntity.PhysicsBody)
            .Where(body => body.Enabled)
            .Where(body => body.Tag is IDamageable damageable)
            .Where(body => excludedTarget == null || !ReferenceEquals(body.Tag, excludedTarget))
            .Where(body => IsHostileTag(bulletEntity.Faction, body.Tag));
    }

    public static bool IsHostileTag(BulletFaction faction, object targetTag)
    {
        return faction switch
        {
            BulletFaction.Player => targetTag is AbstractEnemy,
            BulletFaction.Enemy => targetTag is IDamageable and not AbstractEnemy,
            _ => false
        };
    }

    public static Body GetClosestHostileTarget(BulletEntity bulletEntity, IDamageable excludedTarget = null)
    {
        return GetHostileTargets(bulletEntity, excludedTarget)
            .MinBy(body => (body.Position - bulletEntity.PhysicsBody.Position).LengthSquared());
    }

    public static int GetHomingTargetPriority(BulletFaction faction, object targetTag)
    {
        return faction switch
        {
            BulletFaction.Player when targetTag is AbstractEnemy => 0,
            BulletFaction.Enemy when targetTag is Player => 0,
            BulletFaction.Enemy when targetTag is ShootHoleWall => 1,
            BulletFaction.Enemy when targetTag is IDamageable and not AbstractEnemy => 2,
            _ => int.MaxValue
        };
    }

    public static float GetHomingTargetDotThreshold(BulletFaction faction, object targetTag)
    {
        return faction == BulletFaction.Enemy && targetTag is Player ? -0.15f : 0.5f;
    }

    public static Body GetClosestHomingTargetToLine(
        BulletEntity bulletEntity,
        IEnumerable<Body> candidates,
        Microsoft.Xna.Framework.Vector2 lineStart,
        Microsoft.Xna.Framework.Vector2 lineEnd)
    {
        return candidates
            .Select(body => new
            {
                Body = body,
                Priority = GetHomingTargetPriority(bulletEntity.Faction, body.Tag),
                Dist = GetDistanceToLine(lineStart, lineEnd, body.Position)
            })
            .OrderBy(item => item.Priority)
            .ThenBy(item => item.Dist)
            .FirstOrDefault()?.Body;
    }

    public static Body GetClosestHostileTargetToLine(
        BulletEntity bulletEntity,
        IEnumerable<Body> candidates,
        Microsoft.Xna.Framework.Vector2 lineStart,
        Microsoft.Xna.Framework.Vector2 lineEnd)
    {
        return candidates
            .Select(body => new
            {
                Body = body,
                Dist = GetDistanceToLine(lineStart, lineEnd, body.Position)
            })
            .OrderBy(item => item.Dist)
            .FirstOrDefault()?.Body;
    }

    private static float GetDistanceToLine(
        Microsoft.Xna.Framework.Vector2 a,
        Microsoft.Xna.Framework.Vector2 b,
        Microsoft.Xna.Framework.Vector2 p)
    {
        Microsoft.Xna.Framework.Vector2 dir = b - a;
        float lengthSquared = dir.LengthSquared();

        if (lengthSquared == 0)
        {
            return Microsoft.Xna.Framework.Vector2.Distance(p, a);
        }

        float t = Microsoft.Xna.Framework.Vector2.Dot(p - a, dir) / lengthSquared;
        if (t < 0)
        {
            return float.MaxValue;
        }

        Microsoft.Xna.Framework.Vector2 projection = a + t * dir;
        return Microsoft.Xna.Framework.Vector2.Distance(p, projection);
    }
}
