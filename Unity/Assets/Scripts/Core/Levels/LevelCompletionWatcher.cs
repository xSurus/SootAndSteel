using System;

namespace Gamelab.Levels
{
    public class LevelCompletionWatcher
    {
        private bool completionFired;

        public LevelDefinition CurrentLevelDefinition { get; }

        public event Action OnLevelCompleted;

        public LevelCompletionWatcher(LevelDefinition levelDefinition)
        {
            CurrentLevelDefinition = levelDefinition;
        }

        public void Update(float distanceTraveled, bool hasActiveThreats)
        {
            if (completionFired || CurrentLevelDefinition == null) return;
            if (distanceTraveled < CurrentLevelDefinition.LevelDistance || hasActiveThreats) return;

            completionFired = true;
            OnLevelCompleted?.Invoke();
        }
    }
}
