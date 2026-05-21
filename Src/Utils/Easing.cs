using System;

namespace Gamelab.Utils;

public static class Easing
{
    public static float SmoothStep(float t) => t * t * (3f - 2f * t);

    public static float SmoothStepClamped(float value, float rangeStart, float rangeEnd)
    {
        if (rangeEnd <= rangeStart)
            return value >= rangeStart ? 1f : 0f;

        return SmoothStep(Math.Clamp((value - rangeStart) / (rangeEnd - rangeStart), 0f, 1f));
    }
}
