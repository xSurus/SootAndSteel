using System;
using FMOD.Studio;
using FMODUnity;

namespace Gamelab.Services.Sound
{
    public class ParameterBinding
    {
        private readonly Action<float> setter;
        private readonly Func<float> valueGetter;

        public bool active = true;

        public ParameterBinding(Action<float> setter, Func<float> valueGetter)
        {
            this.setter = setter;
            this.valueGetter = valueGetter;
        }

        public void Update() => setter(valueGetter());

        public static ParameterBinding Local(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
            => new ParameterBinding(val => eventInstance.setParameterByName(parameterName, val), valueGetter);

        public static ParameterBinding Global(string parameterName, Func<float> valueGetter)
            => new ParameterBinding(val => RuntimeManager.StudioSystem.setParameterByName(parameterName, val), valueGetter);

        public void Set(float value) => setter(value);

        public float Get(float value) => valueGetter();

        public void Deactivate() => active = false;
    }
}
