using Gamelab.Enemies;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;

namespace Gamelab.Tutorial;

public interface ITutorialDirector
{
    void Initialize(TrainMap trainMap, GameplayContext ctx);
    void Update(float dt, GameplayContext ctx, EnemyManager enemies);
    void OnTrainFrozen();
    void OnAllPlayersKnockedOut();
    void OnLevelCompleted();
    bool ConsumePendingHubOutroRequest();
}