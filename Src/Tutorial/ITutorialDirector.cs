using Gamelab.Enemies;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Tutorial;

public interface ITutorialDirector
{
    void Initialize(TrainMap trainMap, GameplayContext ctx);
    void Update(float dt, GameplayContext ctx, EnemyManager enemies);
    void DrawWorld(SpriteBatch spriteBatch);
    void OnTrainFrozen();
    void OnAllPlayersKnockedOut();
    void OnLevelCompleted();

    bool ConsumePendingHubOutroRequest();
}
