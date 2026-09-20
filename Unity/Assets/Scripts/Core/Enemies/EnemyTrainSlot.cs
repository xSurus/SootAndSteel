using System.Numerics;

namespace Gamelab.Enemies.Core
{
    public enum EnemySlotSide
    {
        Top,
        Bottom
    }

    public readonly struct EnemyTrainSlot
    {
        public EnemySlotSide Side { get; }
        public float PositionRatio { get; }

        public EnemyTrainSlot(EnemySlotSide side, float positionRatio)
        {
            Side = side;
            PositionRatio = positionRatio;
        }

        // Src GetAnchor. Bounds.Bottom is exclusive like MonoGame Rectangle.
        public Vector2 GetAnchor(Gamelab.Map.RectPx bounds, float distanceFromTrainPx, float topClearancePx)
        {
            float x = bounds.Left + bounds.Width * PositionRatio;
            if (Side == EnemySlotSide.Top)
            {
                return new Vector2(x, bounds.Top - distanceFromTrainPx - topClearancePx);
            }
            return new Vector2(x, bounds.Bottom + distanceFromTrainPx);
        }
    }
}
