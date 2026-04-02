namespace Gamelab.Particles.Modifiers;

public interface IParticleModifier
{
    void Update(float dt, ref Particle particle);
}