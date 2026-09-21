using System;
using Gamelab.Screens;
using Gamelab.Services.Sound;
using Gamelab.Utils;

namespace Gamelab.UI
{
    public enum PostLevelStatsPhase
    {
        IntroReveal,
        ContinueStamp,
        FadeOutToHub,
    }

    /// <summary>
    /// Phase logic of Src PostLevelStatsScreen. Update order is the Src order: fade and stamp first,
    /// then the phase switch, with confirm read after the intro reveal step. Sounds come out through Cue.
    /// </summary>
    public sealed class PostLevelStatsModel
    {
        public const float DimFade = 0.6f;
        public const float LineGap = 0.45f;
        public const float TimeLineGap = 0.38f;
        public const float TotalGap = 0.28f;
        public const float CountUp = 1.1f;
        public const float CreditTick = 0.10f;
        public const float StampReveal = 0.25f;
        public const float ContinueAfterStamp = 0.90f;

        private readonly LevelRewardBreakdown rewards;
        private readonly Action<int> grantCredits;
        private readonly FadeTransition fade = new FadeTransition();

        private float revealClock;
        private float continueStampTimer;
        private bool didGrantCredits;
        private bool playedTitleCue, playedDeliveryCue, playedTimeCue, playedDividerCue;
        private bool startedTotalCountUp;
        private bool countingTotal;
        private float countUpElapsed;
        private int lastSoundAtTotal = -1;
        private float creditTickCooldown;
        private bool continueRequested;

        public event Action<string> Cue;
        public event Action ContinueRequested;

        public PostLevelStatsPhase Phase { get; private set; }
        public bool DeliveryVisible { get; private set; }
        public bool TimeVisible { get; private set; }
        public bool SummaryVisible { get; private set; }
        public int DisplayedTotal { get; private set; }
        public string SummaryText => "+" + DisplayedTotal;
        public float FadeOpacity => fade.Opacity;
        public StampRevealTimer Stamp { get; }

        public PostLevelStatsModel(LevelRewardBreakdown rewards, Action<int> grantCredits)
        {
            this.rewards = rewards;
            this.grantCredits = grantCredits;
            Stamp = new StampRevealTimer(StampReveal);
            Stamp.Cue += PlayCue;
            fade.SnapTo(1f);
            fade.FadeOut(1f);
        }

        public void Update(float dt, bool confirm)
        {
            fade.Update(dt);
            Stamp.Update(dt);

            switch (Phase)
            {
                case PostLevelStatsPhase.IntroReveal:
                    UpdateIntroReveal(dt);
                    if (confirm)
                    {
                        countUpElapsed = CountUp;
                        RevealAll();
                        BeginContinueSequence();
                    }
                    break;
                case PostLevelStatsPhase.ContinueStamp:
                    continueStampTimer += dt;
                    if (continueStampTimer >= StampReveal + ContinueAfterStamp)
                    {
                        fade.FadeIn(1f);
                        Phase = PostLevelStatsPhase.FadeOutToHub;
                    }
                    break;
                case PostLevelStatsPhase.FadeOutToHub:
                    if (fade.IsDone && !continueRequested)
                    {
                        continueRequested = true;
                        ContinueRequested?.Invoke();
                    }
                    break;
            }
        }

        private void PlayCue(string sound) => Cue?.Invoke(sound);

        private void BeginContinueSequence()
        {
            if (!didGrantCredits)
            {
                grantCredits?.Invoke(rewards.TotalCredits);
                didGrantCredits = true;
            }

            Stamp.Trigger();
            continueStampTimer = 0f;
            Phase = PostLevelStatsPhase.ContinueStamp;
        }

        private void UpdateIntroReveal(float dt)
        {
            revealClock += dt;

            float tTitle = DimFade;
            float tDelivery = tTitle + LineGap;
            float tTime = tDelivery + LineGap;
            float tDivider = tTime + TimeLineGap;
            float tTotal = tDivider + TotalGap;

            if (!playedTitleCue && revealClock >= tTitle)
            {
                playedTitleCue = true;
                PlayCue(Sounds.Craft);
            }
            if (!playedDeliveryCue && revealClock >= tDelivery)
            {
                playedDeliveryCue = true;
                DeliveryVisible = true;
                PlayCue(Sounds.PickupItem);
            }
            if (!playedTimeCue && revealClock >= tTime)
            {
                playedTimeCue = true;
                TimeVisible = true;
                PlayCue(Sounds.PickupItem);
            }
            if (!playedDividerCue && revealClock >= tDivider)
            {
                playedDividerCue = true;
                PlayCue(Sounds.PickupItem);
            }

            if (revealClock >= tTotal && !startedTotalCountUp)
            {
                startedTotalCountUp = true;
                countingTotal = true;
                countUpElapsed = 0f;
                DisplayedTotal = 0;
                SummaryVisible = true;
                lastSoundAtTotal = -1;
                creditTickCooldown = 0f;
            }

            if (countingTotal)
                UpdateTotalCountUp(dt);
        }

        private void UpdateTotalCountUp(float dt)
        {
            creditTickCooldown -= dt;
            countUpElapsed += dt;

            float progress = Easing.SmoothStepClamped(countUpElapsed, 0f, CountUp);
            int newShown = (int)Math.Round(rewards.TotalCredits * progress);

            if (newShown > lastSoundAtTotal && creditTickCooldown <= 0f)
            {
                lastSoundAtTotal = newShown;
                creditTickCooldown = CreditTick;
                PlayCue(Sounds.PickupItem);
            }

            DisplayedTotal = newShown;

            if (countUpElapsed < CountUp)
                return;

            DisplayedTotal = rewards.TotalCredits;
            PlayCue(Sounds.Craft);
            countingTotal = false;
        }

        private void RevealAll()
        {
            float tTotal = DimFade + LineGap + LineGap + TimeLineGap + TotalGap;
            revealClock = tTotal + CountUp;

            DeliveryVisible = true;
            TimeVisible = true;
            SummaryVisible = true;
            DisplayedTotal = rewards.TotalCredits;

            countingTotal = false;
            startedTotalCountUp = true;
            countUpElapsed = CountUp;

            playedTitleCue = playedDeliveryCue = playedTimeCue = playedDividerCue = true;
        }
    }
}
