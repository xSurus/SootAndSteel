using System;

namespace Gamelab.Particles.Modifiers;

public sealed class BlizzardGustModifier(Func<float> getBlizzard01) : IParticleModifier
{
    public void Update(float dt, ref Particle particle)
    {
        float t = Math.Clamp(getBlizzard01(), 0f, 1f);
        if (t <= 0.0001f)
        {
            return;
        }

        float t2 = t * t;
        float headwind = 340f * t + 520f * t2;
        particle.Velocity.X -= headwind * dt;
        particle.Velocity.Y += (32f + 78f * t) * t * dt;
        float wobble = MathF.Sin(particle.Age * 14f + particle.Position.Y * 0.01f);
        particle.Velocity.X += wobble * (110f * t2) * dt;
    }
}
