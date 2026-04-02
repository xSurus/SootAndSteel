using System;

namespace Gamelab.Services.Vfx;

public sealed class WhiteFilterTransition
{
    private float opacity;
    private float startOpacity;
    private float targetOpacity;
    private float fadeDuration;
    private float fadeTimer;
    private bool fading;

    public float Opacity => opacity;

    public bool IsDone => !fading && Math.Abs(opacity - targetOpacity) <= 0.0001f;

    public void SetImmediate(float newOpacity)
    {
        fading = false;
        opacity = Math.Clamp(newOpacity, 0f, 1f);
        startOpacity = opacity;
        targetOpacity = opacity;
        fadeDuration = 0f;
        fadeTimer = 0f;
    }

    public void FadeIn(float durationSeconds) => FadeTo(1f, durationSeconds);

    public void FadeOut(float durationSeconds) => FadeTo(0f, durationSeconds);

    public void FadeTo(float target, float durationSeconds)
    {
        startOpacity = opacity;
        targetOpacity = Math.Clamp(target, 0f, 1f);
        fadeDuration = Math.Max(0.0001f, durationSeconds);
        fadeTimer = 0f;
        fading = true;
    }

    public void Update(float dt)
    {
        if (!fading)
        {
            return;
        }

        fadeTimer += dt;
        float t = Math.Clamp(fadeTimer / fadeDuration, 0f, 1f);
        opacity = startOpacity + (targetOpacity - startOpacity) * t;

        if (t >= 1f)
        {
            fading = false;
        }
    }
}
