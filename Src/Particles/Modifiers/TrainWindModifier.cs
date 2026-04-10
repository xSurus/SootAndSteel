using Gamelab.Map.Train.State;

namespace Gamelab.Particles.Modifiers;

public class TrainWindModifier() : IParticleModifier
{
    public void Update(float dt, ref Particle particle)
    {
        var context = GamelabGame.Instance.Services.GetService<GameplayContext>();
        if (context == null) return;
        particle.Position.X -= context.State.actualSpeed * dt;
    }
}