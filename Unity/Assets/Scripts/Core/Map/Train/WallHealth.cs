using System;

namespace Gamelab.Map.Train
{
    // Health part of Src ShootHoleWall. Defaults are GameplayConfig.WallMaxHealth 50 and
    // WallHealthRestoredPerSecond 20 (gameplay.json holds the same values).
    public sealed class WallHealth
    {
        public const float DefaultMax = 50f;
        public const float DefaultRepairPerSecond = 20f;

        private readonly float repairPerSecond;

        public float Max { get; }
        public float Current { get; private set; }
        public bool IsBroken => Current <= 0f;
        // Stays true from the first break until the wall is back at full health (Src isBreached).
        public bool IsBreached { get; private set; }
        public float DamagePercent => (1f - Current / Max) * 100f;

        public event Action Breached;
        public event Action Repaired;

        public WallHealth(float max = DefaultMax, float repairPerSecond = DefaultRepairPerSecond)
        {
            Max = max;
            this.repairPerSecond = repairPerSecond;
            Current = max;
        }

        public void TakeDamage(float amount)
        {
            if (IsBroken) return;
            Current = Math.Max(0f, Current - amount);
            if (IsBroken && !IsBreached)
            {
                IsBreached = true;
                Breached?.Invoke();
            }
        }

        public void Repair(float dt)
        {
            Current = Math.Min(Max, Current + repairPerSecond * dt);
            if (Current < Max) return;
            bool wasBreached = IsBreached;
            IsBreached = false;
            if (wasBreached) Repaired?.Invoke();
        }
    }
}
