using System;
using Microsoft.Xna.Framework.Audio;

namespace Gamelab.Services.Sound;

public interface ISoundService
{
    public SoundHandle RegisterSound(string soundName);
    public SoundHandle RegisterSound(string soundName, int maxConcurrent);
    public void FadeIn(SoundEffectInstance sound, double duration);
    public void FadeOut(SoundEffectInstance sound, double duration);
}