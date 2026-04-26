using System.Collections.Generic;
using Gamelab.Config;

namespace Gamelab.Map.Train.State;

public class TrainSpeedSetting
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

    public static TrainSpeedSetting Stopped { get; private set; }
    public static TrainSpeedSetting Slow { get; private set; }
    public static TrainSpeedSetting Default { get; private set; }
    public static TrainSpeedSetting Fast { get; private set; }

    public static List<TrainSpeedSetting> All { get; private set; }

    public static void Initialize(GameplayConfig config)
    {
        Stopped = new TrainSpeedSetting("Stopped", 0f, 0f);
        Slow = new TrainSpeedSetting("Slow", config.TrainSpeedSlow, config.TrainBurnMultiplierSlow);
        Default = new TrainSpeedSetting("Default", config.TrainSpeedDefault, config.TrainBurnMultiplierDefault);
        Fast = new TrainSpeedSetting("Fast", config.TrainSpeedFast, config.TrainBurnMultiplierFast);

        All = [Slow, Default, Fast];
    }
}