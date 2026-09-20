namespace Gamelab.Config
{
    // Train temperature and patch GameplayConfig values (speeds stay in TrainSpeedTuning).
    // Default holds the effective Src values. Note: gameplay.json has the keys
    // "temperatureDecreasePerSecondPerBreachedWall", "temperatureDecreasePerSecondEngineOff"
    // (5.0) and "temperatureIncreasePerSecond", but the GameplayConfig properties are named
    // Train*, so those keys never bind and the class defaults apply: 3 / 3 / 6. The rest
    // (acceleration 50, max temperature 100, snow and ice values) match json and defaults.
    public sealed class TrainStateTuning
    {
        public static readonly TrainStateTuning Default = new TrainStateTuning(
            50f, 100f, 3f, 3f, 6f,
            0.85f, 0.4f, 2f, 4f,
            0.5f, 0.3f, 3f, 6f);

        public float AccelerationRate { get; }
        public float MaxTemperature { get; }
        public float DecreasePerSecondPerBreachedWall { get; }
        public float DecreasePerSecondEngineOff { get; }
        public float IncreasePerSecond { get; }
        public float SnowStartThreshold { get; }
        public float SnowMaxCoverage { get; }
        public float SnowSpawnIntervalSeconds { get; }
        public float SnowMeltIntervalSeconds { get; }
        public float IceStartThreshold { get; }
        public float IceMaxCoverage { get; }
        public float IceSpawnIntervalSeconds { get; }
        public float IceMeltIntervalSeconds { get; }

        public TrainStateTuning(float accelerationRate, float maxTemperature,
            float decreasePerSecondPerBreachedWall, float decreasePerSecondEngineOff, float increasePerSecond,
            float snowStartThreshold, float snowMaxCoverage, float snowSpawnIntervalSeconds, float snowMeltIntervalSeconds,
            float iceStartThreshold, float iceMaxCoverage, float iceSpawnIntervalSeconds, float iceMeltIntervalSeconds)
        {
            AccelerationRate = accelerationRate;
            MaxTemperature = maxTemperature;
            DecreasePerSecondPerBreachedWall = decreasePerSecondPerBreachedWall;
            DecreasePerSecondEngineOff = decreasePerSecondEngineOff;
            IncreasePerSecond = increasePerSecond;
            SnowStartThreshold = snowStartThreshold;
            SnowMaxCoverage = snowMaxCoverage;
            SnowSpawnIntervalSeconds = snowSpawnIntervalSeconds;
            SnowMeltIntervalSeconds = snowMeltIntervalSeconds;
            IceStartThreshold = iceStartThreshold;
            IceMaxCoverage = iceMaxCoverage;
            IceSpawnIntervalSeconds = iceSpawnIntervalSeconds;
            IceMeltIntervalSeconds = iceMeltIntervalSeconds;
        }
    }
}
