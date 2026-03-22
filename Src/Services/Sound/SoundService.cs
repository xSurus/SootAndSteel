using System;
using System.Collections.Generic;
using Gamelab.Systems;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Gamelab.Services.Sound;

public class SoundService : ISoundService, IDisposable, IGameSystem
{
    private readonly Logger logger = new Logger("SoundService");
    
    private readonly Dictionary<string, SoundEffect> sfxLibrary = new Dictionary<string, SoundEffect>();
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
        
        SoundEffectInstance[] pool = new SoundEffectInstance[maxConcurrent];
        for (int i = 0; i < maxConcurrent; i++)
        {
            pool[i] = sfxLibrary[soundName].CreateInstance();
        }
        return new SoundHandle(pool);
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
    
    private void PreloadSound(string soundName)
    {
        sfxLibrary[soundName] = GamelabGame.Instance.Content.Load<SoundEffect>("sfx/" + soundName);
    }
    
    public void Dispose()
    {
        foreach (var sfx in sfxLibrary.Values)
        {
            sfx.Dispose();
        }
        sfxLibrary.Clear();
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