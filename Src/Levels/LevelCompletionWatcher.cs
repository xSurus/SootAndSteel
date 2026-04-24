using System;
using Gamelab.Enemies;
using Gamelab.Map.Train.State;

namespace Gamelab.Levels;

public class LevelCompletionWatcher
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private bool completionFired;

    public LevelDefinition CurrentLevelDefinition { get; }

    public event Action OnLevelCompleted;

    public LevelCompletionWatcher(LevelDefinition levelDefinition)
    {
        CurrentLevelDefinition = levelDefinition;
    }

    public void Update(EnemyManager enemyManager)
    {
        if (completionFired || CurrentLevelDefinition == null) return;

        bool isComplete = gameplayContext.State.DistanceTraveled >= CurrentLevelDefinition.LevelDistance &&
                          !enemyManager.HasActiveThreats;
        if (!isComplete) return;

        completionFired = true;
        OnLevelCompleted?.Invoke();
    }
}
