using System;

namespace Gamelab.Services.Sound;

public class SoundAnimation(Action<double> interpolate, double duration)
{
    public readonly Action<double> interpolate = interpolate;
    public readonly double duration = duration;
    public double elapsedTime = 0;
}