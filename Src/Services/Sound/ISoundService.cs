using System;

namespace Gamelab.Services.Sound;

public interface ISoundService
{
    public void LoadSound(string id);
    public void UnloadSound(string id);
    public void PlayOnce(string id);
    public object GetSoundInstance(string id);
    public void StartSound(object soundInstance);
    public void StopSound(object soundInstance);
    public void DisposeSound(object soundInstance);
    public void RegisterParameter(object soundInstance, string parameterName, Func<float> valueGetter);
}