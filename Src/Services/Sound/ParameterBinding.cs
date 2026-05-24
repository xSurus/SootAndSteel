using System;
using FmodForFoxes.Studio;

namespace Gamelab.Services.Sound;

public class ParameterBinding(Action<float> setter, Func<float> valueGetter)
{
    public bool active = true;

    public void Update() => setter(valueGetter());

    public static ParameterBinding Local(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
        => new(val => eventInstance.SetParameterValue(parameterName, val), valueGetter);

    public static ParameterBinding Global(string parameterName, Func<float> valueGetter)
        => new(val => StudioSystem.SetParameterValue(parameterName, val), valueGetter);

    public void Set(float value)
    {
        setter(value);
    }

    public float Get(float value)
    {
        return valueGetter();
    }

    public void Deactivate()
    {
        active = false;
    }
}