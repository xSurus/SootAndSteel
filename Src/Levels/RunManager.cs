using System;
using Gamelab.Enemies;
using Gamelab.Map.Train.State;
using Gamelab.Serialization;

namespace Gamelab.Levels;

public class RunManager
{
    private readonly IRunLevelProvider levelProvider;
    private readonly RunSession session;

    public LevelDefinition CurrentLevelDefinition { get; private set; }
    public RunPhase CurrentPhase { get; private set; } = RunPhase.LevelActive;
    public event Action<int> OnIntermissionStarted;

    public RunManager(RunSession session, IRunLevelProvider levelProvider)
    {
        this.session = session ?? throw new ArgumentNullException(nameof(session));
        this.levelProvider = levelProvider ?? throw new ArgumentNullException(nameof(levelProvider));

        LoadLevel(session.CurrentLevel);
    }

    public void Update(GameplayContext gameplayContext, EnemyManager enemyManager)
    {
        if (CurrentPhase != RunPhase.LevelActive || CurrentLevelDefinition == null) return;
        bool isLevelComplete = gameplayContext.State.DistanceTraveled >= CurrentLevelDefinition.LevelDistance &&
                               !enemyManager.HasActiveThreats;

        if (isLevelComplete)
        {
            CurrentPhase = RunPhase.Intermission;
            OnIntermissionStarted?.Invoke(session.CurrentLevel);
        }
    }

    private void LoadLevel(int levelNumber)
    {
        CurrentLevelDefinition = levelProvider.GetLevel(levelNumber);
    }
}