using System;
using FMOD.Studio;

namespace Gamelab.Services.Sound
{
    public interface ISoundService
    {
        void LoadSound(string id);
        void UnloadSound(string id);
        void PlayOnce(string id);
        EventInstance GetSoundInstance(string id);
        ParameterBinding RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter);
        ParameterBinding RegisterGlobalParameter(string parameterName, Func<float> valueGetter);
        void SetGlobalParameter(string parameterName, float value);
        void ResetGlobalParameters();
        SoundSettings Settings { get; }
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
    }
}
