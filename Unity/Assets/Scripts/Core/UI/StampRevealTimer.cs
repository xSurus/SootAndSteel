using System;
using Gamelab.Services.Sound;
using Gamelab.Utils;

namespace Gamelab.UI
{
    /// <summary>
    /// Timing of Src StampRevealAnimator without the sprite. The view multiplies its base size by
    /// ScaleFactor and adds RotationOffset to its base rotation. Cue fires once with Sounds.Stamp.
    /// </summary>
    public sealed class StampRevealTimer
    {
        private readonly float durationSeconds;
        private readonly float popScale;
        private readonly float popRotationOffset;
        private float timer;
        private bool soundPlayed;

        public event Action<string> Cue;

        public bool Active { get; private set; }
        public bool Visible { get; private set; }
        public float ScaleFactor { get; private set; } = 1f;
        public float RotationOffset { get; private set; }

        public StampRevealTimer(float durationSeconds = 0.25f, float popScale = 1.35f, float popRotationOffset = -10f)
        {
            this.durationSeconds = durationSeconds;
            this.popScale = popScale;
            this.popRotationOffset = popRotationOffset;
        }

        public void Trigger()
        {
            Active = true;
            timer = 0f;
            Visible = true;
            ScaleFactor = popScale;
            RotationOffset = popRotationOffset;
        }

        public void Update(float dt)
        {
            if (!Active)
                return;

            timer += dt;
            float t = Math.Clamp(timer / durationSeconds, 0f, 1f);
            float eased = Easing.SmoothStep(t);

            if (eased > 0.5 && !soundPlayed)
            {
                soundPlayed = true;
                Cue?.Invoke(Sounds.Stamp);
            }

            ScaleFactor = popScale + (1f - popScale) * eased;
            RotationOffset = popRotationOffset + (0f - popRotationOffset) * eased;

            if (t >= 1f)
                Active = false;
        }
    }
}
