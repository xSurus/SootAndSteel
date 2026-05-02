using Microsoft.Xna.Framework;

namespace Gamelab.Particles.Modifiers;

public class DirectionalForceModifier(Vector2 direction, float strength) : IParticleModifier
{
    public Vector2 Direction { get; } = direction;
    public float Strength { get; set; } = strength;

    public void Update(float dt, ref Particle particle)
    {
        particle.Velocity += Direction * Strength * dt;
    }
}