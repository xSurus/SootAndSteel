using Microsoft.Xna.Framework;

namespace Gamelab.Particles.Modifiers;

public class DirectionalForceModifier(Vector2 direction, float strength) : IParticleModifier
{
    public readonly float Strength = strength;
    public readonly Vector2 Direction = direction;

    public void Update(float dt, ref Particle particle)
    {
        particle.Velocity += Direction * Strength * dt;
    }
}