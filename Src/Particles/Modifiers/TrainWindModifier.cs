using Gamelab.Map.Train.State;

namespace Gamelab.Particles.Modifiers;

public class TrainWindModifier() : IParticleModifier
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    public void Update(float dt, ref Particle particle)
    {
        if (gameplayContext == null) return;
        particle.Position.X -= gameplayContext.State.actualSpeed * dt;
    }
}