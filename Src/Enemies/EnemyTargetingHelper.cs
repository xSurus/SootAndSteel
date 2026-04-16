using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Structures;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public static class EnemyTargetingHelper
{
    public static Vector2 GetTargetPoint(GameplayContext gameplayContext, EnemySlotSide side, Vector2 enemyPosition)
    {
        ShootHoleWall localWall = GetNearestWallOnSide(gameplayContext, side, enemyPosition);
        return localWall?.Position ?? gameplayContext.Map.GetBounds().Center.ToVector2();
    }

    private static ShootHoleWall GetNearestWallOnSide(GameplayContext gameplayContext, EnemySlotSide side,
        Vector2 enemyPosition)
    {
        ShootHoleWall bestWall = null;
        float bestDistanceSquared = float.MaxValue;

        foreach (IPhysicalEntity entity in gameplayContext.Map.MapObjects)
        {
            if (entity is not ShootHoleWall wall || !IsOnSide(gameplayContext, wall.Position, side))
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