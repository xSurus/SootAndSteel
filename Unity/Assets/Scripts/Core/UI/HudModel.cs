using System;
using System.Collections.Generic;
using Gamelab.Levels;

namespace Gamelab.UI
{
    /// <summary>
    /// State of Src GameplayHud.Update: distance ratio, smoothed needle and temperature ratio.
    /// Update runs once per frame and uses no clock. The 0.15 smoothing is per call, as in Src.
    /// </summary>
    public class HudModel
    {
        private readonly List<float> dotRatios = new List<float>();
        private LevelDefinition level;
        private float levelDistance = 1f;
        private float maxSpeed = 1f;

        public HudModel(LevelDefinition level)
        {
            SetLevel(level);
        }

        public float DistanceRatio { get; private set; }
        public float SmoothedSpeedRatio { get; private set; }

        // Deliberate deviation: Src starts at 0, which would draw full frost until the first update.
        public float TemperatureRatio { get; private set; } = 1f;

        public float NeedleDegrees => HudMath.NeedleDegrees(SmoothedSpeedRatio * maxSpeed);
        public float FrostOpacity1 => HudMath.FrostOpacity(1, TemperatureRatio);
        public float FrostOpacity2 => HudMath.FrostOpacity(2, TemperatureRatio);
        public float FrostOpacity3 => HudMath.FrostOpacity(3, TemperatureRatio);
        public IReadOnlyList<float> DotRatios => dotRatios;

        /// <summary>Increments on every SetLevel so a view knows to rebuild its dots.</summary>
        public int LevelVersion { get; private set; }

        public void SetLevel(LevelDefinition def)
        {
            level = def;
            levelDistance = Math.Max(def?.LevelDistance ?? 1f, 1f);
            dotRatios.Clear();
            if (def != null)
            {
                foreach (var spawn in def.SpawnEvents)
                    dotRatios.Add(HudMath.DotRatio(spawn.Distance, levelDistance));
            }
            LevelVersion++;
        }

        public void Update(float distance, float actualSpeed, float temperature, float maxTemperature, float maxSpeed)
        {
            DistanceRatio = HudMath.DistanceRatio(distance, levelDistance);

            this.maxSpeed = Math.Max(maxSpeed, 1f);
            float speedRatio = HudMath.SpeedRatio(actualSpeed, maxSpeed);
            SmoothedSpeedRatio = HudMath.Lerp(SmoothedSpeedRatio, speedRatio, HudMath.NeedleSmoothing);

            TemperatureRatio = HudMath.Clamp(temperature / Math.Max(maxTemperature, 1f), 0f, 1f);
        }
    }
}
