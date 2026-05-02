using System;
using Gamelab.Config;
using Microsoft.Xna.Framework;

namespace Gamelab.Utils;

public enum GridDirection
{
    Up,
    Down,
    Left,
    Right
}

public static class WorldUtility
{
    public static float PixelsPerMeter { get; private set; }
    public static float TileSize { get; private set; }

    public static void Initialize()
    {
        GameplayConfig config = GamelabGame.Instance.GameplayConfig;
        PixelsPerMeter = config.PixelsPerMeter;
        TileSize = config.TrainTileSize;
    }

    public static float ToMeters(float pixels) => pixels / PixelsPerMeter;
    public static float ToMeters(int pixels) => pixels / PixelsPerMeter;
    public static float ToPixels(float meters) => meters * PixelsPerMeter;

    public static Vector2 ToMeters(Vector2 pixels) => pixels / PixelsPerMeter;
    public static Vector2 ToPixels(Vector2 meters) => meters * PixelsPerMeter;

    public static Point ToGrid(Vector2 pixels) => new Point(
        (int)Math.Floor(pixels.X / TileSize),
        (int)Math.Floor(pixels.Y / TileSize));

    public static GridDirection GetOppositeDirection(GridDirection direction) => direction switch
    {
        GridDirection.Up => GridDirection.Down,
        GridDirection.Down => GridDirection.Up,
        GridDirection.Left => GridDirection.Right,
        GridDirection.Right => GridDirection.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };

    public static GridDirection GetNextClockwiseDirection(GridDirection direction) => direction switch
    {
        GridDirection.Up => GridDirection.Right,
        GridDirection.Down => GridDirection.Left,
        GridDirection.Left => GridDirection.Up,
        GridDirection.Right => GridDirection.Down,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };

    public static Vector2 ToVector2(GridDirection direction) => direction switch
    {
        GridDirection.Up => new Vector2(0, -1),
        GridDirection.Down => new Vector2(0, 1),
        GridDirection.Left => new Vector2(-1, 0),
        GridDirection.Right => new Vector2(1, 0),
        _ => Vector2.Zero
    };
}

public static class PhysicsExtensions
{
    public static float ToMeters(this float pixels) => WorldUtility.ToMeters(pixels);
    public static float ToMeters(this int pixels) => WorldUtility.ToMeters(pixels);
    public static float ToPixels(this float meters) => WorldUtility.ToPixels(meters);

    public static Vector2 ToMeters(this Vector2 pixels) => WorldUtility.ToMeters(pixels);
    public static Vector2 ToPixels(this Vector2 meters) => WorldUtility.ToPixels(meters);
    public static Point ToGrid(this Vector2 pixels) => WorldUtility.ToGrid(pixels);
    public static GridDirection Opposite(this GridDirection direction) => WorldUtility.GetOppositeDirection(direction);

    public static GridDirection Clockwise(this GridDirection direction) =>
        WorldUtility.GetNextClockwiseDirection(direction);

    public static Vector2 ToVector2(this GridDirection direction) => WorldUtility.ToVector2(direction);
}