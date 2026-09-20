namespace Gamelab.Config
{
    // Train speed GameplayConfig values. The class defaults in Src/Config/GameplayConfig.cs
    // have all burn multipliers at 1, but Src/Content/Data/gameplay.json (loaded over the
    // defaults) sets trainBurnMultiplierSlow 0.5 and trainBurnMultiplierFast 4.0. Default here
    // holds the effective values: speeds 150/300/600 px/s, burn 0.5/1/4.
    public sealed class TrainSpeedTuning
    {
        public static readonly TrainSpeedTuning Default = new TrainSpeedTuning(150f, 300f, 600f, 0.5f, 1f, 4f);

        public float SpeedSlow { get; }
        public float SpeedDefault { get; }
        public float SpeedFast { get; }
        public float BurnMultiplierSlow { get; }
        public float BurnMultiplierDefault { get; }
        public float BurnMultiplierFast { get; }

        public TrainSpeedTuning(float speedSlow, float speedDefault, float speedFast,
            float burnMultiplierSlow, float burnMultiplierDefault, float burnMultiplierFast)
        {
            SpeedSlow = speedSlow;
            SpeedDefault = speedDefault;
            SpeedFast = speedFast;
            BurnMultiplierSlow = burnMultiplierSlow;
            BurnMultiplierDefault = burnMultiplierDefault;
            BurnMultiplierFast = burnMultiplierFast;
        }
    }
}
