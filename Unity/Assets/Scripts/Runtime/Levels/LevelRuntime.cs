using System;
using Gamelab.Map.Train.State;
using UnityEngine;

namespace Gamelab.Levels
{
    /// <summary>
    /// Runs one level: completion watching and spawn hand-out from the train's distance.
    /// TrainStateRuntime ticks itself in its own Update, so this component never ticks the train
    /// (a second tick would double the distance). Tick() only reads the distance.
    /// The enemy manager of the enemy/level integration wave subscribes to SpawnDue.
    /// </summary>
    public sealed class LevelRuntime : MonoBehaviour
    {
        private LevelCompletionWatcher watcher;
        private SpawnSchedule schedule;
        private TrainStateRuntime train;
        private Func<bool> hasActiveThreats;

        public LevelDefinition Definition { get; private set; }
        public float DistanceTraveled => train != null ? train.State.DistanceTraveled : 0f;
        public SpawnSchedule Schedule => schedule;

        public event Action LevelCompleted;
        public event Action<SpawnEvent> SpawnDue;

        public void Initialize(LevelDefinition def, TrainStateRuntime trainState, Func<bool> threats)
        {
            Definition = def;
            train = trainState;
            hasActiveThreats = threats;
            schedule = new SpawnSchedule(def);
            watcher = new LevelCompletionWatcher(def);
            watcher.OnLevelCompleted += () => LevelCompleted?.Invoke();
        }

        public void Tick()
        {
            if (watcher == null) return;
            float distance = train.State.DistanceTraveled;
            foreach (SpawnEvent e in schedule.PopDue(distance)) SpawnDue?.Invoke(e);
            watcher.Update(distance, hasActiveThreats());
        }

        private void Update() => Tick();
    }
}
