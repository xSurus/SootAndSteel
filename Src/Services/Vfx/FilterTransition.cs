using System;
using Microsoft.Xna.Framework;

namespace Gamelab.Services.Vfx;

public sealed class FilterTransition
{
    private float opacity;
    private float startOpacity;
    private float targetOpacity;
    private float fadeDuration;
    private float fadeTimer;
    private float delayDuration;
    private float delayTimer;
    private bool fading;
    private bool smoothEasing;

    private Color filterColor = Color.White;
    
    public float Opacity => opacity;
    public Color FilterColor => filterColor;

    public bool IsDone => !fading && (delayTimer >= delayDuration && fadeTimer >= fadeDuration);

    public float EffectIntensity
    {
        get
        {
            if (IsDone) return targetOpacity;
            if (!fading) return opacity;

            float total = delayDuration + fadeDuration;
            float elapsed = delayTimer + fadeTimer;
            float progress = Math.Clamp(elapsed / total, 0f, 1f);

            if (targetOpacity >= startOpacity)
            {
                return progress;
            }
            else
            {
                return 1f - progress;
            }
        }
    }

    public void FadeIn(float durationSeconds, float delaySeconds = 0f, Color? color = null, bool smoothEasing = true) 
        => FadeTo(1f, durationSeconds, delaySeconds, color, smoothEasing);

    public void FadeOut(float durationSeconds, float delaySeconds = 0f, Color? color = null, bool smoothEasing = true) 
        => FadeTo(0f, durationSeconds, delaySeconds, color, smoothEasing);

    public void FadeTo(float target, float durationSeconds, float delaySeconds = 0f, Color? color = null, bool smoothEasing = true)
    {
        this.smoothEasing = smoothEasing;
        if (color.HasValue) filterColor = color.Value;
        startOpacity = opacity;
        targetOpacity = Math.Clamp(target, 0f, 1f);
        fadeDuration = Math.Max(0.001f, durationSeconds);
        delayDuration = Math.Max(0f, delaySeconds);
        fadeTimer = 0f;
        delayTimer = 0f;
        fading = true;
    }

    public void SnapTo(float target, Color? color = null)
    {
        if (color.HasValue) filterColor = color.Value;
        opacity = Math.Clamp(target, 0f, 1f);
        fading = false;
        delayTimer = delayDuration;
        fadeTimer = fadeDuration;
    }

    public void Update(float dt)
    {
        if (!fading)
        {
            return;
        }

        if (delayTimer < delayDuration)
        {
            delayTimer += dt;
            if (delayTimer < delayDuration) return;
        }

        fadeTimer += dt;
        float rawT = Math.Clamp(fadeTimer / fadeDuration, 0f, 1f);
        float t = smoothEasing ? rawT * rawT * (3f - 2f * rawT) : rawT;
        opacity = startOpacity + (targetOpacity - startOpacity) * t;

        if (fadeTimer >= fadeDuration)
        {
            opacity = targetOpacity;
            fading = false;
        }
    }
}
