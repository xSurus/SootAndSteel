using System.Numerics;

namespace Gamelab
{
    /// <summary>
    /// Single pixel/meter conversion point. Src authored everything in pixels;
    /// the Unity physics world is in meters (Src GameplayConfig.PixelsPerMeter = 100).
    /// </summary>
    public static class WorldUnits
    {
        public const float PixelsPerMeter = 100f;

        public static float ToMeters(float pixels) => pixels / PixelsPerMeter;
        public static float ToPixels(float meters) => meters * PixelsPerMeter;
        public static Vector2 ToMeters(Vector2 pixels) => pixels / PixelsPerMeter;
        public static Vector2 ToPixels(Vector2 meters) => meters * PixelsPerMeter;
    }
}
