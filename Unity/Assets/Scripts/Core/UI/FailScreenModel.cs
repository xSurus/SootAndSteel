using System;
using Gamelab.Screens;
using Gamelab.Services.Sound;

namespace Gamelab.UI
{
    public enum FailScreenPhase
    {
        WaitingForReturn,
        Stamping,
    }

    /// <summary>
    /// Typewriter, confirm and return timing of Src FailScreen. The typewriter runs before the phase
    /// switch and takes the same dt, as in Src.
    /// </summary>
    public sealed class FailScreenModel
    {
        public const float StampRevealSeconds = 0.20f;
        public const float ReturnDelayAfterStampSeconds = 0.90f;
        public const float CharsPerSecond = 55f;
        private const float SoundInterval = 0.1f;

        private readonly string fullCauseText;
        private float typedCharacters;
        private float soundTimer;
        private float phaseTimer;
        private bool returnRequested;

        public event Action<string> Cue;
        public event Action ReturnRequested;

        public FailScreenPhase Phase { get; private set; } = FailScreenPhase.WaitingForReturn;
        public string CauseText { get; private set; } = string.Empty;
        public StampRevealTimer Stamp { get; }

        public FailScreenModel(FailureReason reason)
        {
            fullCauseText = FailureReasonText.Get(reason);
            Stamp = new StampRevealTimer(StampRevealSeconds, 1.4f, -8f);
            Stamp.Cue += s => Cue?.Invoke(s);
        }

        public void Update(float dt, bool confirm)
        {
            UpdateCauseTypewriter(dt);
            switch (Phase)
            {
                case FailScreenPhase.WaitingForReturn:
                    if (confirm)
                        BeginStampAndReturn();
                    break;
                case FailScreenPhase.Stamping:
                    Stamp.Update(dt);
                    phaseTimer += dt;
                    if (phaseTimer >= StampRevealSeconds + ReturnDelayAfterStampSeconds && !returnRequested)
                    {
                        returnRequested = true;
                        ReturnRequested?.Invoke();
                    }
                    break;
            }
        }

        private void BeginStampAndReturn()
        {
            Phase = FailScreenPhase.Stamping;
            phaseTimer = 0f;
            typedCharacters = fullCauseText.Length;
            CauseText = fullCauseText;
            Cue?.Invoke(Sounds.MenuSelect);
            Stamp.Trigger();
        }

        private void UpdateCauseTypewriter(float dt)
        {
            if (typedCharacters >= fullCauseText.Length)
                return;

            typedCharacters = Math.Min(fullCauseText.Length, typedCharacters + dt * CharsPerSecond);
            int count = Math.Clamp((int)typedCharacters, 0, fullCauseText.Length);
            CauseText = fullCauseText.Substring(0, count);

            soundTimer += dt;
            if (soundTimer >= SoundInterval)
            {
                Cue?.Invoke(Sounds.PickupItem);
                soundTimer -= SoundInterval;
            }
        }
    }
}
