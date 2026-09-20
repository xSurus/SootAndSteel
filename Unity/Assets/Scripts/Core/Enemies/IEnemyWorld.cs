using System.Numerics;

namespace Gamelab.Enemies.Core
{
    // Seam for what Src reads from GameplayContext.Map: slot anchors and wall targeting
    // (Src EnemyTargetingHelper.GetTargetPoint, EnemyTrainSlot.GetAnchor). Pixels, y-down.
    public interface IEnemyWorld : IWorldBounds
    {
        Vector2 GetSlotAnchor(EnemyTrainSlot slot, float distanceFromTrainPx);
        Vector2 GetTargetPoint(EnemySlotSide side, Vector2 enemyPositionPx);
    }
}
