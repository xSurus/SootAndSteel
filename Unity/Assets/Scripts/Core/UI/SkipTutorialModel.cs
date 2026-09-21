using System;

namespace Gamelab.UI
{
    /// <summary>
    /// Progress rules of Src TutorialDirector.UpdateSkipProgress. Progress fills at 2.0 per second
    /// while skip is held, drains at 5.0 per second otherwise, and is capped at 0..1.5. The bar
    /// width is Progress / 1.5 (ProgressRatio), so a full hold takes 0.75 s.
    /// SkipRequested mirrors Src pendingHubOutroRequest: it is set whenever the unclamped progress
    /// reaches the cap and stays true until the caller consumes it, even if progress drains later.
    /// Src's ConsumePendingHubOutroRequest also clears tutorial guidance, marks the tutorial completed
    /// and saves the run (TutorialDirector.cs:184-186); all of that stays with the caller.
    /// </summary>
    public class SkipTutorialModel
    {
        public const string Text = "Hold to skip tutorial";
        public const float HoldSeconds = 1.5f;
        public const float FillRate = 2.0f;
        public const float DrainRate = 5.0f;

        public float Progress { get; private set; }
        public float ProgressRatio => Progress / HoldSeconds;
        public bool SkipRequested { get; private set; }

        public void Update(float dt, bool skipHeld)
        {
            Progress += (skipHeld ? FillRate : -DrainRate) * dt;
            if (Progress >= HoldSeconds) SkipRequested = true;
            Progress = Math.Min(Math.Max(Progress, 0f), HoldSeconds);
        }

        /// <summary>Returns true once per request and clears it (Src ConsumePendingHubOutroRequest).</summary>
        public bool ConsumeRequest()
        {
            bool was = SkipRequested;
            SkipRequested = false;
            return was;
        }
    }
}
