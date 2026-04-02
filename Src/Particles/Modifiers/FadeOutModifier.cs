namespace Gamelab.Particles.Modifiers;

public class FadeOutModifier(float duration) : IParticleModifier
{
    private readonly float Duration = duration;

    public void Update(float dt, ref Particle particle)
    {
        if (Duration <= 0f) return;

        float timeLeft = particle.MaxAge - particle.Age;
        if (timeLeft < Duration)
        {
            float alphaRatio = timeLeft / Duration;
            particle.Color *= alphaRatio;
        }
    }
}