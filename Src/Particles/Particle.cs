using Microsoft.Xna.Framework;

namespace Gamelab.Particles;

public struct Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Age;
    public float MaxAge;
    public Color Color;
    public Color InitialColor;
    public float Size;
    public float Rotation;
    public float RotationSpeed;
}