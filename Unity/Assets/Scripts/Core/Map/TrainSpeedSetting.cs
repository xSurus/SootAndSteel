using System.Collections.Generic;
using Gamelab.Config;

namespace Gamelab.Map.Train.State
{
    // Port of Src/Map/Train/State/TrainSpeedSetting.cs. Src keeps static singletons filled by
    // Initialize(GameplayConfig). Static mutable state is awkward in tests, so this builds a
    // set instead: CreateSet(tuning). Reference equality still identifies a setting, as in Src,
    // so all users must share one set (wave B's TrainState owns it).
    public sealed class TrainSpeedSetting
    {
        public string Name { get; }
        public float TargetSpeed { get; }
        public float BurnMultiplier { get; }

        private TrainSpeedSetting(string name, float targetSpeed, float burnMultiplier)
        {
            Name = name;
            TargetSpeed = targetSpeed;
            BurnMultiplier = burnMultiplier;
        }

        public sealed class Set
        {
            public TrainSpeedSetting Stopped { get; }
            public TrainSpeedSetting Slow { get; }
            public TrainSpeedSetting Default { get; }
            public TrainSpeedSetting Fast { get; }
            public IReadOnlyList<TrainSpeedSetting> All { get; }

            internal Set(TrainSpeedTuning t)
            {
                Stopped = new TrainSpeedSetting("Stopped", 0f, 0f);
                Slow = new TrainSpeedSetting("Slow", t.SpeedSlow, t.BurnMultiplierSlow);
                Default = new TrainSpeedSetting("Default", t.SpeedDefault, t.BurnMultiplierDefault);
                Fast = new TrainSpeedSetting("Fast", t.SpeedFast, t.BurnMultiplierFast);
                All = new[] { Slow, Default, Fast };
            }
        }

        public static Set CreateSet(TrainSpeedTuning tuning) => new Set(tuning);
    }
}
