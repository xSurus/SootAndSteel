using System.Numerics;

namespace Gamelab.Map
{
    // Mirrors MonoGame Rectangle for ints (pixels, y down). Right and Bottom are exclusive.
    public readonly struct RectPx
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public RectPx(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;

        // Integer division like MonoGame Rectangle.Center.
        public TrainPoint Center => new TrainPoint(X + Width / 2, Y + Height / 2);

        public Vector2 CenterVector => new Vector2(Center.X, Center.Y);

        public bool Contains(int x, int y) => X <= x && x < X + Width && Y <= y && y < Y + Height;
    }
}
