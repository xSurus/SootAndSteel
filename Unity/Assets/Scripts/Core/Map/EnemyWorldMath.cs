using System.Collections.Generic;
using System.Numerics;
using Gamelab.Enemies.Core;

namespace Gamelab.Map
{
    // Port of Src EnemyTargetingHelper.
    public static class EnemyWorldMath
    {
        public static Vector2 GetTargetPoint(IReadOnlyList<Vector2> wallPositions, EnemySlotSide side,
            Vector2 enemyPx, RectPx trainBounds)
        {
            float centerY = trainBounds.Center.Y;
            bool found = false;
            Vector2 best = default;
            float bestSq = float.MaxValue;
            foreach (Vector2 wall in wallPositions)
            {
                bool onSide = side == EnemySlotSide.Top ? wall.Y < centerY : wall.Y > centerY;
                if (!onSide) continue;
                float sq = Vector2.DistanceSquared(enemyPx, wall);
                if (sq >= bestSq) continue;
                bestSq = sq;
                best = wall;
                found = true;
            }
            return found ? best : trainBounds.CenterVector;
        }
    }
}
