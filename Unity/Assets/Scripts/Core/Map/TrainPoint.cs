using System;

namespace Gamelab.Map
{
    public readonly struct TrainPoint : IEquatable<TrainPoint>
    {
        public int X { get; }
        public int Y { get; }

        public TrainPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(TrainPoint other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is TrainPoint p && Equals(p);
        public override int GetHashCode() => unchecked(X * 397 ^ Y);
        public static bool operator ==(TrainPoint a, TrainPoint b) => a.Equals(b);
        public static bool operator !=(TrainPoint a, TrainPoint b) => !a.Equals(b);
        public override string ToString() => "(" + X + ", " + Y + ")";
    }
}
