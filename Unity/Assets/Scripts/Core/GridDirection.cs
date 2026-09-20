using System;
using System.Numerics;

namespace Gamelab.Utils
{
    // Ported from Src/Utils/WorldUtility.cs (GridDirection and its extension helpers).
    public enum GridDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class GridDirectionExtensions
    {
        public static GridDirection Opposite(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up: return GridDirection.Down;
                case GridDirection.Down: return GridDirection.Up;
                case GridDirection.Left: return GridDirection.Right;
                case GridDirection.Right: return GridDirection.Left;
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static GridDirection Clockwise(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up: return GridDirection.Right;
                case GridDirection.Down: return GridDirection.Left;
                case GridDirection.Left: return GridDirection.Up;
                case GridDirection.Right: return GridDirection.Down;
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        // Src frame is y-down, so Up is (0, -1).
        public static Vector2 ToVector2(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up: return new Vector2(0, -1);
                case GridDirection.Down: return new Vector2(0, 1);
                case GridDirection.Left: return new Vector2(-1, 0);
                case GridDirection.Right: return new Vector2(1, 0);
                default: return Vector2.Zero;
            }
        }
    }
}
