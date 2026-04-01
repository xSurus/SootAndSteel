namespace Gamelab.Particles.Modifiers;

public class FadeInModifier(float duration) : IParticleModifier
{
    private readonly float Duration = duration;

    public void Update(float dt, ref Particle particle)
    {
        if (Duration <= 0f) return;

        if (particle.Age < Duration)
        {
            float alphaRatio = particle.Age / Duration;
            particle.Color *= alphaRatio;
        }
    }
}