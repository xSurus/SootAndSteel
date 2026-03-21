using System;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework.Audio;

namespace Gamelab.Services.Sound;

public class SoundHandle : IDisposable
{
    private readonly SoundEffectInstance[] pool;
    private int currentIndex;
    private bool isDisposed;
    private readonly Action unregister;

    internal SoundHandle(SoundEffectInstance[] pool, Action unregister)
    {
        this.pool = pool;
        this.unregister = unregister;
    }

    public SoundEffectInstance Play(float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
    {
        if (isDisposed) return null;
        
        SoundEffectInstance sfx = pool[currentIndex];
        sfx.Stop();
        sfx.Volume = Math.Clamp(volume, 0.0f, 1.0f);
        sfx.Pitch = Math.Clamp(pitch, -1.0f, 1.0f);
        sfx.Pan = Math.Clamp(pan, -1.0f, 1.0f);
        sfx.IsLooped = false;
        sfx.Play();
        
        currentIndex = (currentIndex + 1) % pool.Length;

        return sfx;
    }

    public SoundEffectInstance PlayLooping(float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
    {
        if (isDisposed) return null;
        
        SoundEffectInstance sfx = pool[currentIndex];
        sfx.Stop();
        sfx.Volume = Math.Clamp(volume, 0.0f, 1.0f);
        sfx.Pitch = Math.Clamp(pitch, -1.0f, 1.0f);
        sfx.Pan = Math.Clamp(pan, -1.0f, 1.0f);
        sfx.IsLooped = true;
        sfx.Play();
        currentIndex = (currentIndex + 1) % pool.Length;

        return sfx;
    }

    public void StopAll()
    {
        foreach (SoundEffectInstance sfx in pool)
            sfx.Stop();
    }
    
    public bool IsPlaying()
    {
        foreach (SoundEffectInstance sfx in pool)
        {
            if (sfx.State == SoundState.Playing)
                return true;
        }
        return false;
    }
    
    public void Dispose()
    {
        if (isDisposed) return;
        foreach (SoundEffectInstance sfx in pool)
            sfx.Dispose();
        isDisposed = true;
        unregister?.Invoke();
    }
}