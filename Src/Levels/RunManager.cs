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
    private bool hasReachedLevelDestination;

    public event Action<int, LevelDefinition> OnLevelStarted;
    public event Action<int> OnIntermissionStarted;

    /// <summary>
    /// Fired when the train covers the level's target distance, before all enemies are eliminated.
    /// Use this to trigger the "train drives off screen" animation while combat may still be active.
    /// </summary>
    public event Action<int> OnLevelDestinationReached;
    public event Action OnRunEnded;

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

        if (!hasReachedLevelDestination && levelManager.HasReachedLevelDestination(gameplayContext))
        {
            hasReachedLevelDestination = true;
            OnLevelDestinationReached?.Invoke(CurrentLevelNumber);
        }

        if (levelManager.IsLevelComplete(gameplayContext, enemyManager))
        {
            CurrentPhase = RunPhase.Intermission;
            OnIntermissionStarted?.Invoke(CurrentLevelNumber);
        }
    }

    /// <summary>
    /// Starts the next level if the run is currently in intermission.
    /// The current total distance is used as the new level baseline.
    /// </summary>
    public bool TryAdvanceToNextLevel(float currentDistanceTraveled)
    {
        if (CurrentPhase != RunPhase.Intermission)
        {
            return false;
        }

        CurrentLevelNumber++;
        LoadLevel(CurrentLevelNumber, currentDistanceTraveled);
        CurrentPhase = RunPhase.LevelActive;
        return true;
    }

    /// <summary>
    /// Ends the run and emits the run-ended event once.
    /// </summary>
    public void EndRun()
    {
        if (CurrentPhase == RunPhase.GameOver)
        {
            return;
        }

        CurrentPhase = RunPhase.GameOver;
        OnRunEnded?.Invoke();
    }

    /// <summary>
    /// Loads a level definition from the provider and rebuilds level-completion tracking for that level baseline.
    /// </summary>
    private void LoadLevel(int levelNumber, float levelStartDistance)
    {
        CurrentLevelDefinition = levelProvider.GetLevel(levelNumber);
        levelManager = new LevelManager(CurrentLevelDefinition, levelStartDistance);
        hasReachedLevelDestination = false;
        OnLevelStarted?.Invoke(levelNumber, CurrentLevelDefinition);
    }
}
