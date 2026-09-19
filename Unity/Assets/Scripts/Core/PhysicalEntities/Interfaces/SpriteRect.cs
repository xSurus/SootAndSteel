namespace Gamelab.PhysicalEntities.Interfaces
{
    public readonly struct SpriteRect
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public SpriteRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
