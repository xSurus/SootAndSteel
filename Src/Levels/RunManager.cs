using System;
using Gamelab.Enemies;
using Gamelab.Map.Train.State;

namespace Gamelab.Levels;

public class RunManager
{
    private readonly IRunLevelProvider levelProvider;
    private LevelManager levelManager;

    public int CurrentLevelNumber { get; private set; }
    public LevelDefinition CurrentLevelDefinition { get; private set; }
    public RunPhase CurrentPhase { get; private set; } = RunPhase.LevelActive;

    public event Action<int, LevelDefinition> OnLevelStarted;
    public event Action<int> OnIntermissionStarted;
    public event Action OnRunEnded;

    public RunManager(int startingLevel, IRunLevelProvider levelProvider)
    {
        this.levelProvider = levelProvider ?? throw new ArgumentNullException(nameof(levelProvider));
        CurrentLevelNumber = Math.Max(1, startingLevel);
        LoadLevel(CurrentLevelNumber);
    }

    public void Update(GameplayContext gameplayContext, EnemyManager enemyManager)
    {
        if (CurrentPhase != RunPhase.LevelActive || CurrentLevelDefinition == null)
        {
            return;
        }

        if (levelManager.IsLevelComplete(gameplayContext, enemyManager))
        {
            CurrentPhase = RunPhase.Intermission;
            OnIntermissionStarted?.Invoke(CurrentLevelNumber);
        }
    }

    public bool TryAdvanceToNextLevel()
    {
        if (CurrentPhase != RunPhase.Intermission)
        {
            return false;
        }

        CurrentLevelNumber++;
        LoadLevel(CurrentLevelNumber);
        CurrentPhase = RunPhase.LevelActive;
        return true;
    }

    public void EndRun()
    {
        if (CurrentPhase == RunPhase.GameOver)
        {
            return;
        }

        CurrentPhase = RunPhase.GameOver;
        OnRunEnded?.Invoke();
    }

    private void LoadLevel(int levelNumber)
    {
        CurrentLevelDefinition = levelProvider.GetLevel(levelNumber);
        levelManager = new LevelManager(CurrentLevelDefinition);
        OnLevelStarted?.Invoke(levelNumber, CurrentLevelDefinition);
    }
}
