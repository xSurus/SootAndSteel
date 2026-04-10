using Gamelab.Enemies;
using Gamelab.Map.Train.State;

namespace Gamelab.Levels;

public class LevelManager
{
    private readonly LevelDefinition levelDefinition;
    private readonly float levelStartDistance;

    public LevelManager(LevelDefinition levelDefinition, float levelStartDistance)
    {
        this.levelDefinition = levelDefinition;
        this.levelStartDistance = levelStartDistance;
    }

    public bool IsLevelComplete(GameplayContext gameplayContext, EnemyManager enemyManager)
    {
        float distanceInCurrentLevel = gameplayContext.State.DistanceTraveled - levelStartDistance;
        return distanceInCurrentLevel >= levelDefinition.LevelDistance &&
               enemyManager.Enemies.Count == 0;
    }
}

