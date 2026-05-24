using System;
using FmodForFoxes.Studio;

namespace Gamelab.Services.Sound;

public interface ISoundService
{
    public void LoadSound(string id);
    public void UnloadSound(string id);
    public void PlayOnce(string id);
    public EventInstance GetSoundInstance(string id);
    public ParameterBinding RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter);
    public ParameterBinding RegisterGlobalParameter(string parameterName, Func<float> valueGetter);
    public void SetGlobalParameter(string parameterName, float value);
    public void ResetGlobalParameters();
    public SoundSettings Settings { get; }
    public void SetMasterVolume(float volume);
    public void SetMusicVolume(float volume);
    public void SetSfxVolume(float volume);
}
