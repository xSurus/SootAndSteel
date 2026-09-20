using Gamelab.Enemies.Core;

namespace Gamelab.Map
{
    // Src culls enemies against origin 0 and ScreenWidth, so MinX is 0 and MaxX the screen width.
    public sealed class MapBounds : IWorldBounds
    {
        public float MinX => 0f;
        public float MaxX { get; }

        public MapBounds(float screenWidth = TrainTuning.ScreenWidth)
        {
            MaxX = screenWidth;
        }
    }
}
