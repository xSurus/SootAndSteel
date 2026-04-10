using System;
using FmodForFoxes.Studio;

namespace Gamelab.Services.Sound;

public interface ISoundService
{
    public void LoadSound(string id);
    public void UnloadSound(string id);
    public void PlayOnce(string id);
    public EventInstance GetSoundInstance(string id);
    public void RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter);
}