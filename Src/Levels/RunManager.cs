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
    public event Action<int> OnIntermissionStarted;

    /// <summary>
    /// Creates a run manager, clamps the initial level to at least 1, and loads that first level.
    /// </summary>
    public RunManager(int startingLevel, IRunLevelProvider levelProvider)
    {
        this.levelProvider = levelProvider ?? throw new ArgumentNullException(nameof(levelProvider));
        CurrentLevelNumber = Math.Max(1, startingLevel);
        LoadLevel(CurrentLevelNumber, 0f);
    }

    /// <summary>
    /// Advances run state while a level is active and switches to intermission when completion conditions are met.
    /// </summary>
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

    /// <summary>
    /// Loads a level definition from the provider and rebuilds level-completion tracking for that level baseline.
    /// </summary>
    private void LoadLevel(int levelNumber, float levelStartDistance)
    {
        CurrentLevelDefinition = levelProvider.GetLevel(levelNumber);
        levelManager = new LevelManager(CurrentLevelDefinition, levelStartDistance);
    }
}
