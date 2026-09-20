using System.Collections.Generic;

namespace Gamelab.Levels
{
    /// <summary>
    /// Port of the spawn consumption in Src/Enemies/EnemyManager.Update: events are handed out in list order
    /// once distanceTraveled reaches their Distance, each exactly once. Src also reads the next event's
    /// distance for its intensity value. That belongs to the enemy manager, which can use NextEvent.
    /// </summary>
    public sealed class SpawnSchedule
    {
        private readonly IReadOnlyList<SpawnEvent> events;
        private int nextIndex;

        public SpawnSchedule(LevelDefinition definition)
        {
            events = definition?.SpawnEvents ?? new List<SpawnEvent>();
        }

        public bool IsExhausted => nextIndex >= events.Count;

        /// <summary>The next event not yet handed out, or null when exhausted.</summary>
        public SpawnEvent NextEvent => IsExhausted ? null : events[nextIndex];

        public IReadOnlyList<SpawnEvent> PopDue(float distanceTraveled)
        {
            var due = new List<SpawnEvent>();
            while (nextIndex < events.Count && distanceTraveled >= events[nextIndex].Distance)
                due.Add(events[nextIndex++]);
            return due;
        }
    }
}
