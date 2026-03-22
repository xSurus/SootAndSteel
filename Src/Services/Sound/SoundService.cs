using System;
using System.Collections.Generic;
using Gamelab.Systems;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Gamelab.Services.Sound;

public class SoundService : ISoundService, IDisposable, IGameSystem
{
    private Logger logger = new Logger("SoundService");
    
    private readonly Dictionary<string, SoundEffect> sfxLibrary = new Dictionary<string, SoundEffect>();
    private readonly Dictionary<string, int> sfxReferences = new Dictionary<string, int>();
    private readonly List<SoundAnimation> sfxAnimations = new List<SoundAnimation>();

    public void Initialize(GamelabGame game)
    {
    }

    public SoundHandle RegisterSound(string soundName)
    {
        return RegisterSound(soundName, 1);
    }
    
    public SoundHandle RegisterSound(string soundName, int maxConcurrent)
    {
        if (!sfxLibrary.ContainsKey(soundName))
            PreloadSound(soundName);
        sfxReferences[soundName] += 1;
        
        SoundEffectInstance[] pool = new SoundEffectInstance[maxConcurrent];
        for (int i = 0; i < maxConcurrent; i++)
        {
            pool[i] = sfxLibrary[soundName].CreateInstance();
        }
        return new SoundHandle(pool, () => UnregisterSound(soundName));
    }

    public void FadeIn(SoundEffectInstance sound, double duration)
    {
        sfxAnimations.Add(new SoundAnimation(
            interpolate: (elapsedTime) =>
            {
                if (sound == null) return;
                sound.Volume = Math.Clamp((float)(elapsedTime / duration), 0.0f, 1.0f);
            },
            duration: duration
        ));
    }

    public void FadeOut(SoundEffectInstance sound, double duration)
    {
        sfxAnimations.Add(new SoundAnimation(
            interpolate: (elapsedTime) =>
            {
                if (sound == null) return;
                sound.Volume = Math.Clamp(1.0f - (float)(elapsedTime / duration), 0.0f, 1.0f);
            },
            duration: duration
        ));
    }

    private void UnregisterSound(string soundName)
    {
        if (!sfxLibrary.ContainsKey(soundName)) return;
        
        sfxReferences[soundName] -= 1;

        if (sfxReferences[soundName] > 0) return;
        
        logger.Debug("Unloading sound: " + soundName);
        sfxLibrary[soundName].Dispose();
        sfxLibrary.Remove(soundName);
        sfxReferences.Remove(soundName);
    }
    
    private void PreloadSound(string soundName)
    {
        sfxLibrary[soundName] = GamelabGame.Instance.Content.Load<SoundEffect>("sfx/" + soundName);
        sfxReferences[soundName] = 0;
    }
    
    public void Dispose()
    {
        foreach (var sfx in sfxLibrary.Values)
        {
            sfx.Dispose();
        }
        sfxLibrary.Clear();
        sfxReferences.Clear();
    }

    public void Update(GameTime gameTime)
    {
        List<SoundAnimation> elapsedAnimations = new List<SoundAnimation>();
        double dt = gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var anim in sfxAnimations)
        {
            anim.elapsedTime += dt;
            anim.interpolate?.Invoke(anim.elapsedTime);
            if (anim.elapsedTime >= anim.duration)
            {
                elapsedAnimations.Add(anim);
            }
        }
        
        foreach (var anim in elapsedAnimations)
            sfxAnimations.Remove(anim);
    }

    public void Draw()
    {
    }

    public void Shutdown()
    {
        logger.Info("Shutting down");
        Dispose();
    }
}