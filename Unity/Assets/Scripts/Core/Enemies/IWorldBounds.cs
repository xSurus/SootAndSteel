namespace Gamelab.Enemies.Core
{
    // World horizontal extent in pixels. Src's origin is 0 and its right edge is the screen width;
    // wave B supplies the real map/camera bounds.
    public interface IWorldBounds
    {
        float MinX { get; }
        float MaxX { get; }
    }
}
