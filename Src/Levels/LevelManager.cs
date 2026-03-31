using Gamelab.Enemies;
using Gamelab.Map.Train.State;

namespace Gamelab.Levels;

public class LevelManager
{
    private readonly LevelDefinition levelDefinition;

    public LevelManager(LevelDefinition levelDefinition)
    {
        this.levelDefinition = levelDefinition;
    }

    public bool IsLevelComplete(GameplayContext gameplayContext, EnemyManager enemyManager)
    {
        return gameplayContext.State.DistanceTraveled >= levelDefinition.LevelDistance &&
               enemyManager.Enemies.Count == 0;
    }
}

