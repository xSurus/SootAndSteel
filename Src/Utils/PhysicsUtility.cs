using Microsoft.Xna.Framework;

namespace Gamelab.Utils;

public static class PhysicsUtility
{
    public static float PixelsPerMeter { get; private set; }

    public static void Initialize(float pixelsPerMeter)
    {
        PixelsPerMeter = pixelsPerMeter;
    }

    public static float ToMeters(float pixels) => pixels / PixelsPerMeter;
    public static float ToMeters(int pixels) => pixels / PixelsPerMeter;
    public static float ToPixels(float meters) => meters * PixelsPerMeter;

    public static Vector2 ToMeters(Vector2 pixels) => pixels / PixelsPerMeter;
    public static Vector2 ToPixels(Vector2 meters) => meters * PixelsPerMeter;
}

public static class PhysicsExtensions
{
    public static float ToMeters(this float pixels) => PhysicsUtility.ToMeters(pixels);
    public static float ToMeters(this int pixels) => PhysicsUtility.ToMeters(pixels);
    public static float ToPixels(this float meters) => PhysicsUtility.ToPixels(meters);

    public static Vector2 ToMeters(this Vector2 pixels) => PhysicsUtility.ToMeters(pixels);
    public static Vector2 ToPixels(this Vector2 meters) => PhysicsUtility.ToPixels(meters);
}