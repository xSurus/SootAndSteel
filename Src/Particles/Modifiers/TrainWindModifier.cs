using Gamelab.Map.Train.State;

namespace Gamelab.Particles.Modifiers;

public class TrainWindModifier() : IParticleModifier
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    public void Update(float dt, ref Particle particle)
    {
        float trainSpeed = gameplayContext.State.actualSpeed;
        particle.Position.X -= trainSpeed * dt;
    }
}