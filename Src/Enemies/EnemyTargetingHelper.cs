using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Workbenches;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public static class EnemyTargetingHelper
{
    public static Vector2 GetTargetPoint(GameplayContext gameplayContext, EnemySlotSide side, Vector2 enemyPosition)
    {
        ShootHoleWall wallTarget = GetNearestIntactWallOnSide(gameplayContext, side, enemyPosition);
        if (!HasBrokenWallOnSide(gameplayContext, side))
        {
            return wallTarget?.Position ?? gameplayContext.Map.GetBounds().Center.ToVector2();
        }

        IPhysicalEntity interiorTarget = GetHighestPriorityInteriorTarget(gameplayContext, enemyPosition);
        if (interiorTarget != null)
        {
            return interiorTarget.Position;
        }

        return wallTarget?.Position ?? gameplayContext.Map.GetBounds().Center.ToVector2();
    }

    private static bool HasBrokenWallOnSide(GameplayContext gameplayContext, EnemySlotSide side)
    {
        foreach (IPhysicalEntity entity in gameplayContext.Map.MapObjects)
        {
            if (entity is ShootHoleWall wall && wall.IsBroken && IsOnSide(gameplayContext, wall.Position, side))
            {
                return true;
            }
        }

        return false;
    }

    private static ShootHoleWall GetNearestIntactWallOnSide(GameplayContext gameplayContext, EnemySlotSide side, Vector2 enemyPosition)
    {
        ShootHoleWall bestWall = null;
        float bestDistanceSquared = float.MaxValue;

        foreach (IPhysicalEntity entity in gameplayContext.Map.MapObjects)
        {
            if (entity is not ShootHoleWall wall || wall.IsBroken || !IsOnSide(gameplayContext, wall.Position, side))
            {
                continue;
            }

            float distanceSquared = Vector2.DistanceSquared(enemyPosition, wall.Position);
            if (distanceSquared >= bestDistanceSquared)
            {
                continue;
            }

            bestDistanceSquared = distanceSquared;
            bestWall = wall;
        }

        return bestWall;
    }

    private static IPhysicalEntity GetHighestPriorityInteriorTarget(GameplayContext gameplayContext, Vector2 enemyPosition)
    {
        IPhysicalEntity target = GetNearestOfType<CannonStation>(gameplayContext, enemyPosition);
        if (target != null) return target;

        target = GetNearestOfType<SpeedLever>(gameplayContext, enemyPosition);
        if (target != null) return target;

        target = GetNearestOfType<Anvil>(gameplayContext, enemyPosition);
        if (target != null) return target;

        target = GetNearestPlayer(gameplayContext, enemyPosition);
        return target;
    }

    private static T GetNearestOfType<T>(GameplayContext gameplayContext, Vector2 enemyPosition) where T : class, IPhysicalEntity
    {
        T best = null;
        float bestDistanceSquared = float.MaxValue;

        foreach (IPhysicalEntity entity in gameplayContext.Map.MapObjects)
        {
            if (entity is not T candidate)
            {
                continue;
            }

            float distanceSquared = Vector2.DistanceSquared(enemyPosition, candidate.Position);
            if (distanceSquared >= bestDistanceSquared)
            {
                continue;
            }

            bestDistanceSquared = distanceSquared;
            best = candidate;
        }

        return best;
    }

    private static Player GetNearestPlayer(GameplayContext gameplayContext, Vector2 enemyPosition)
    {
        Player best = null;
        float bestDistanceSquared = float.MaxValue;

        foreach (var body in gameplayContext.PhysicsWorld.BodyList)
        {
            if (body.Tag is not Player player || player.IsStunned)
            {
                continue;
            }

            float distanceSquared = Vector2.DistanceSquared(enemyPosition, player.Position);
            if (distanceSquared >= bestDistanceSquared)
            {
                continue;
            }

            bestDistanceSquared = distanceSquared;
            best = player;
        }

        return best;
    }

    private static bool IsOnSide(GameplayContext gameplayContext, Vector2 position, EnemySlotSide side)
    {
        float centerY = gameplayContext.Map.GetBounds().Center.Y;
        return side switch
        {
            EnemySlotSide.Top => position.Y < centerY,
            EnemySlotSide.Bottom => position.Y > centerY,
            _ => false
        };
    }
}
