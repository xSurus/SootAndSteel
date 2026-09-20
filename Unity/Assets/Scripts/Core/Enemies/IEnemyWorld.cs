using System.Numerics;

namespace Gamelab.Enemies.Core
{
    // Seam for what Src reads from GameplayContext.Map: slot anchors and wall targeting
    // (Src EnemyTargetingHelper.GetTargetPoint, EnemyTrainSlot.GetAnchor). Pixels, y-down.
    public interface IEnemyWorld : IWorldBounds
    {
        Vector2 GetSlotAnchor(EnemyTrainSlot slot, float distanceFromTrainPx);
        // Src EnemyTargetingHelper.GetTargetPoint: position of the ShootHoleWall nearest to
        // enemyPositionPx among walls on the given side (Top: wall Y < map centre Y, Bottom:
        // wall Y > centre Y). With none, the map bounds centre. The frame is y-down, so Top
        // means the smaller Y.
        Vector2 GetTargetPoint(EnemySlotSide side, Vector2 enemyPositionPx);
    }
}
