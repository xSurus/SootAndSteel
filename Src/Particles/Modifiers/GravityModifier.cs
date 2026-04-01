using Microsoft.Xna.Framework;

namespace Gamelab.Particles.Modifiers;

public class GravityModifier(float strength) : IParticleModifier
{
    public readonly float Strength = strength;

    public void Update(float dt, ref Particle particle)
    {
        particle.Velocity += Vector2.UnitY * Strength * dt;
    }
}