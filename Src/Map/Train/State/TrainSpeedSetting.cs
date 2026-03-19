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
    public static TrainSpeedSetting Default { get; private set; }
    public static TrainSpeedSetting Double { get; private set; }
    public static TrainSpeedSetting Quadruple { get; private set; }

    public static List<TrainSpeedSetting> All { get; private set; }

    public static void Initialize(GameplayConfig config)
    {
        Stopped = new TrainSpeedSetting("Stopped", config.TrainSpeedStopped, config.TrainBurnMultiplierStopped);
        Default = new TrainSpeedSetting("Default", config.TrainSpeedDefault, config.TrainBurnMultiplierDefault);
        Double = new TrainSpeedSetting("Double", config.TrainSpeedDouble, config.TrainBurnMultiplierDouble);
        Quadruple = new TrainSpeedSetting("Quadruple", config.TrainSpeedQuadruple, config.TrainBurnMultiplierQuadruple);

        All = [Stopped, Default, Double, Quadruple];
    }
}