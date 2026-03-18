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

    public static TrainSpeedSetting Stopped { get; private set; } = new("Stopped", 0f, 1f);
    public static TrainSpeedSetting Default { get; private set; } = new("Default", 150f, 1f);
    public static TrainSpeedSetting Double { get; private set; } = new("Double", 300f, 1f);
    public static TrainSpeedSetting Quadruple { get; private set; } = new("Quadruple", 600f, 4f);

    public static List<TrainSpeedSetting> All { get; private set; } =
    [
        Stopped, Default, Double, Quadruple
    ];

    public static void Initialize(GameplayConfig config)
    {
        Stopped = new TrainSpeedSetting("Stopped", config.TrainSpeedStopped, config.TrainBurnMultiplierStopped);
        Default = new TrainSpeedSetting("Default", config.TrainSpeedDefault, config.TrainBurnMultiplierDefault);
        Double = new TrainSpeedSetting("Double", config.TrainSpeedDouble, config.TrainBurnMultiplierDouble);
        Quadruple = new TrainSpeedSetting("Quadruple", config.TrainSpeedQuadruple, config.TrainBurnMultiplierQuadruple);

        All = [Stopped, Default, Double, Quadruple];
    }
}