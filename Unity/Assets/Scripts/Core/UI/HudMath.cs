using System;

namespace Gamelab.UI
{
    /// <summary>
    /// Pure formulas of Src GameplayHud. Constants and expressions mirror the Src file.
    /// </summary>
    public static class HudMath
    {
        // Constants from Src GameplayHud (speedometer calibration, smoothing, track padding).
        public const float MinDisplaySpeed = 0f;
        public const float MaxDisplaySpeed = 600f;
        public const float AngleAtMinDisplaySpeed = 115.7099f;
        public const float AngleAtMaxDisplaySpeed = -119.0546f;
        public const float NeedleSmoothing = 0.15f;
        public const float MinimumTrackWidthPixels = 1f;
        public const float TrainMarkerPaddingPixels = 24f;

        /// <summary>MathHelper.Lerp(a, b, t) = a + (b - a) * t.</summary>
        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

        public static float DistanceRatio(float distance, float levelDistance)
        {
            float level = Math.Max(levelDistance, 1f);
            return Math.Clamp(Math.Max(0f, distance) / level, 0f, 1f);
        }

        /// <summary>Src TryGetTrackTravelRange: 0 when the track is too narrow to lay out.</summary>
        public static float TrackTravelRange(float trackWidth)
        {
            if (trackWidth <= MinimumTrackWidthPixels)
                return 0f;
            return Math.Max(0f, trackWidth - (TrainMarkerPaddingPixels * 2f));
        }

        public static float SpeedRatio(float actualSpeed, float maxSpeed)
            => Math.Clamp(actualSpeed / Math.Max(maxSpeed, 1f), 0f, 1f);

        /// <summary>Src GetNeedleContainerRotation, result in 0..360.</summary>
        public static float NeedleDegrees(float speed)
        {
            float clamped = Math.Clamp(speed, MinDisplaySpeed, MaxDisplaySpeed);
            float t = (clamped - MinDisplaySpeed) / (MaxDisplaySpeed - MinDisplaySpeed);
            float angle = Lerp(AngleAtMinDisplaySpeed, AngleAtMaxDisplaySpeed, t);
            float normalized = angle % 360f;
            if (normalized < 0f)
                normalized += 360f;
            return normalized;
        }

        /// <summary>Src Draw: layer 1 above 0.3, layer 2 above 0.7, layer 3 above 0.9 of frost = 1 - ratio.</summary>
        public static float FrostOpacity(int layer, float temperatureRatio)
        {
            float frost = 1f - temperatureRatio;
            switch (layer)
            {
                case 1:
                    return frost > 0.3f ? Math.Clamp((frost - 0.3f) * (1f / 0.7f), 0f, 1f) : 0f;
                // frost > 0.7 and > 0.9 compare as doubles on purpose, as in Src GameplayHud. Do not change to float.
                case 2:
                    return frost > 0.7 ? Math.Clamp((frost - 0.7f) * (1f / 0.3f), 0f, 1f) : 0f;
                case 3:
                    return frost > 0.9 ? Math.Clamp((frost - 0.9f) * (1f / 0.1f), 0f, 1f) : 0f;
                default:
                    return 0f;
            }
        }

        public static float DotRatio(float spawnDistance, float levelDistance)
            => Math.Clamp(spawnDistance / Math.Max(levelDistance, 1f), 0f, 1f);
    }
}
