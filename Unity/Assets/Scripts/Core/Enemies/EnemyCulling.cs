namespace Gamelab.Enemies.Core
{
    public static class EnemyCulling
    {
        // Src AbstractEnemy.IsOffScreenLeft: Position.X < -Size (origin 0).
        public static bool IsOffScreenLeft(float xPx, float sizePx, IWorldBounds b) => xPx < b.MinX - sizePx;

        // Src Enemy.cs flee cull: X > ScreenWidth + 600 || X < -Size.
        public static bool IsFleeCulled(float xPx, float sizePx, IWorldBounds b) =>
            xPx > b.MaxX + 600f || xPx < b.MinX - sizePx;
    }
}
