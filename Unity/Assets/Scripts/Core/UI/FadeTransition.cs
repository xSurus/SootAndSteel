using System;

namespace Gamelab.UI
{
    /// <summary>
    /// Opacity math of Src FilterTransition (delay, then smooth-step or linear fade). The colour parts
    /// and EffectIntensity are not ported, the view owns the colour.
    /// </summary>
    public sealed class FadeTransition
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

        public float Opacity => opacity;

        public bool IsDone => !fading && (delayTimer >= delayDuration && fadeTimer >= fadeDuration);

        public void FadeIn(float durationSeconds, float delaySeconds = 0f, bool smoothEasing = true)
            => FadeTo(1f, durationSeconds, delaySeconds, smoothEasing);

        public void FadeOut(float durationSeconds, float delaySeconds = 0f, bool smoothEasing = true)
            => FadeTo(0f, durationSeconds, delaySeconds, smoothEasing);

        public void FadeTo(float target, float durationSeconds, float delaySeconds = 0f, bool smoothEasing = true)
        {
            this.smoothEasing = smoothEasing;
            startOpacity = opacity;
            targetOpacity = Math.Clamp(target, 0f, 1f);
            fadeDuration = Math.Max(0.001f, durationSeconds);
            delayDuration = Math.Max(0f, delaySeconds);
            fadeTimer = 0f;
            delayTimer = 0f;
            fading = true;
        }

        public void SnapTo(float target)
        {
            opacity = Math.Clamp(target, 0f, 1f);
            fading = false;
            delayTimer = delayDuration;
            fadeTimer = fadeDuration;
        }

        public void Update(float dt)
        {
            if (!fading)
                return;

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
}
