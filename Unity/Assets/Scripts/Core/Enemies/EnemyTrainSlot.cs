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

        // GetAnchor(float distanceFromTrain) is not ported yet: it needs the level/map
        // bounds (Src/Map/), which is Wave B (B1)'s subsystem and doesn't exist yet.
        // Add it back here once B1's map bounds API exists.
    }
}
