using System;
using Gamelab.Assets;
using Gamelab.Components;
using Gamelab.Particles;
using Gamelab.Particles.Modifiers;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;

namespace Gamelab.Screens;

public class PostLevelStatsScreen(GamelabGame game, float actualTime, float referenceTime) : GamelabGameScreen(game)
{
    public PostLevelStatsScreen(GamelabGame game) : this(game, 1f, 1f) { }

    private enum Phase
    {
        IntroReveal,
        WaitInput,
        ContinueStamp,
        FadeOutToHub,
    }

    private static class Timing
    {
        public const float DimFade = 0.6f;
        public const float LineGap = 0.45f;
        public const float TimeLineGap = 0.38f;
        public const float TotalGap = 0.28f;
        public const float CountUp = 1.1f;
        public const float CreditTick = 0.10f;
        public const float StampReveal = 0.25f;
        public const float ContinueAfterStamp = 0.90f;
        public const float FadeOutToHub = 0.8f;
        public const float DimOpacity = 100f / 255f;
    }

    private PostStatsScreenOverlay overlay;
    private PostStatsDisplay statsDisplay;
    private StatsListItem deliveryLine;
    private StatsListItem timeLine;
    private StampRevealAnimator paidStampAnimator;

    private readonly WhiteFilterTransition fadeToHub = new();
    private readonly LevelRewardBreakdown rewards =
        LevelRewardBreakdown.FromCompletion(actualTime, referenceTime, game.GameplayConfig);

    private Phase phase;
    private ISoundService soundService;

    private ParticleEmitter snowstormEmitter;
    private SnowstormTransition.Baseline snowstormBaseline;

    private float introDim;
    private float revealClock;
    private float continueStampTimer;
    private bool didGrantCredits;

    private bool playedTitleCue;
    private bool playedDeliveryCue;
    private bool playedTimeCue;
    private bool playedDividerCue;

    private bool countingTotal;
    private float countUpElapsed;
    private int displayedTotalCredits;
    private int lastSoundAtTotal = -1;
    private float creditTickCooldown;

    public override void LoadContent()
    {
        base.LoadContent();
        GumService.Default.Root.Children.Clear();

        soundService = Services.GetService<ISoundService>();
        SetupOverlay();
        SetupSnowstorm();
        phase = Phase.IntroReveal;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        fadeToHub.Update(dt);
        float blizzardT = phase == Phase.FadeOutToHub ? fadeToHub.Opacity : 0f;
        SnowstormTransition.ApplyBlizzardIntensity(snowstormEmitter, snowstormBaseline, blizzardT);
        paidStampAnimator?.Update(dt);

        switch (phase)
        {
            case Phase.IntroReveal:
                UpdateIntroReveal(dt);
                break;
            case Phase.WaitInput:
                if (Game.playerManager.Configs.AnyPressedMenuConfirm())
                    BeginContinueSequence();
                break;
            case Phase.ContinueStamp:
                continueStampTimer += dt;
                if (continueStampTimer >= Timing.StampReveal + Timing.ContinueAfterStamp)
                {
                    fadeToHub.FadeIn(Timing.FadeOutToHub);
                    phase = Phase.FadeOutToHub;
                }
                break;
            case Phase.FadeOutToHub:
                if (fadeToHub.IsDone)
                    Game.SwitchToScreen(new HubScreen(Game));
                break;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        DrawWorldLayer();
        DrawIntroDim();
        GumService.Default.Draw();
        DrawFadeOverlay();

        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        Services.GetService<IVfxService>().ClearAll();
        if (overlay?.Visual != null && GumService.Default.Root.Children.Contains(overlay.Visual))
            GumService.Default.Root.Children.Remove(overlay.Visual);
        base.UnloadContent();
    }

    private void SetupOverlay()
    {
        overlay = new PostStatsScreenOverlay();
        overlay.AddToRoot();

        statsDisplay = overlay.PostStatsDisplayInstance;
        deliveryLine = statsDisplay.StatsListItemInstance;
        timeLine = statsDisplay.StatsListItemInstance1;

        statsDisplay.ButtonWithIconInstance.ButtonText = "Continue";
        XboxButtonGlyphs.ApplyFaceButton(statsDisplay.ButtonWithIconInstance, XboxButtonAtlas.Face.A);
        statsDisplay.FinishStageText.Text = StageNaming.GetStageTitle(Game.CurrentRun.CurrentLevel);

        ConfigureStatsLine(
            deliveryLine,
            index: "01",
            label: "Delivery Reward",
            description: "Coal shipment delivered",
            amount: LevelRewardBreakdown.FormatSignedAmount(rewards.DeliveryReward));

        ConfigureStatsLine(
            timeLine,
            index: "02",
            label: rewards.TimeLineLabel,
            description: rewards.TimeLineDescription,
            amount: LevelRewardBreakdown.FormatSignedAmount(rewards.TimeAdjustment));

        statsDisplay.SummaryDescription.Text = "Account Credited";
        statsDisplay.SummaryText = "+0";
        statsDisplay.SummaryMoney.Visible = false;

        paidStampAnimator = statsDisplay.Paid_Stamp != null
            ? new StampRevealAnimator(statsDisplay.Paid_Stamp, Timing.StampReveal)
            : null;
    }

    private void SetupSnowstorm()
    {
        var vfx = Services.GetService<IVfxService>();
        snowstormEmitter = ParticleFactory.CreateSnowstorm();
        snowstormEmitter.Modifiers.Add(new BlizzardGustModifier(() =>
            phase == Phase.FadeOutToHub ? fadeToHub.Opacity : 0f));
        snowstormBaseline = SnowstormTransition.Capture(snowstormEmitter);
        vfx.AddContinuous(snowstormEmitter);
    }

    private void BeginContinueSequence()
    {
        soundService.PlayOnce(Sounds.MenuSelect);

        if (!didGrantCredits)
        {
            Game.CurrentRun.AddCredits(rewards.TotalCredits);
            didGrantCredits = true;
        }

        paidStampAnimator?.Trigger();
        continueStampTimer = 0f;
        phase = Phase.ContinueStamp;
    }

    private void UpdateIntroReveal(float dt)
    {
        revealClock += dt;
        introDim = Easing.SmoothStepClamped(revealClock, 0f, Timing.DimFade);

        float tTitle = Timing.DimFade;
        float tDelivery = tTitle + Timing.LineGap;
        float tTime = tDelivery + Timing.LineGap;
        float tDivider = tTime + Timing.TimeLineGap;
        float tTotal = tDivider + Timing.TotalGap;

        TryPlayCue(revealClock, tTitle, ref playedTitleCue, Sounds.Craft);
        TryRevealLine(revealClock, tDelivery, ref playedDeliveryCue, deliveryLine, Sounds.PickupItem);
        TryRevealLine(revealClock, tTime, ref playedTimeCue, timeLine, Sounds.PickupItem);
        TryPlayCue(revealClock, tDivider, ref playedDividerCue, Sounds.PickupItem);

        if (revealClock >= tTotal && !countingTotal)
            BeginTotalCountUp();

        if (countingTotal)
            UpdateTotalCountUp(dt);
    }

    private void BeginTotalCountUp()
    {
        countingTotal = true;
        countUpElapsed = 0f;
        displayedTotalCredits = 0;
        statsDisplay.SummaryMoney.Visible = true;
        lastSoundAtTotal = -1;
        creditTickCooldown = 0f;
    }

    private void UpdateTotalCountUp(float dt)
    {
        creditTickCooldown -= dt;
        countUpElapsed += dt;

        float progress = Easing.SmoothStepClamped(countUpElapsed, 0f, Timing.CountUp);
        int newShown = (int)Math.Round(rewards.TotalCredits * progress);

        if (newShown > lastSoundAtTotal && creditTickCooldown <= 0f)
        {
            lastSoundAtTotal = newShown;
            creditTickCooldown = Timing.CreditTick;
            soundService.PlayOnce(Sounds.PickupItem);
        }

        displayedTotalCredits = newShown;
        statsDisplay.SummaryText = $"+{displayedTotalCredits}";

        if (countUpElapsed < Timing.CountUp)
            return;

        displayedTotalCredits = rewards.TotalCredits;
        statsDisplay.SummaryText = $"+{displayedTotalCredits}";
        soundService.PlayOnce(Sounds.Craft);
        countingTotal = false;
        phase = Phase.WaitInput;
    }

    private void TryPlayCue(float clock, float at, ref bool played, string sound)
    {
        if (played || clock < at)
            return;

        played = true;
        soundService.PlayOnce(sound);
    }

    private void TryRevealLine(float clock, float at, ref bool played, StatsListItem line, string sound)
    {
        if (played || clock < at)
            return;

        played = true;
        line.Visual.Visible = true;
        soundService.PlayOnce(sound);
    }

    private static void ConfigureStatsLine(
        StatsListItem line,
        string index,
        string label,
        string description,
        string amount)
    {
        line.IndexText = index;
        line.ItemLableText = label;
        line.ItemDescriptionText = description;
        line.AmountText = amount;
        line.Visual.Visible = false;
    }

    private void DrawWorldLayer()
    {
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        Services.GetService<IVfxService>().Render(spriteBatch);
        spriteBatch.End();
    }

    private void DrawIntroDim()
    {
        if (introDim <= 0.001f)
            return;

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        spriteBatch.Draw(
            AssetManager.BlankTexture,
            new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
            Color.Black * (introDim * Timing.DimOpacity));
        spriteBatch.End();
    }

    private void DrawFadeOverlay()
    {
        float opacity = fadeToHub.Opacity;
        if (opacity <= 0.001f)
            return;

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        spriteBatch.Draw(
            AssetManager.BlankTexture,
            new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
            Color.White * opacity);
        spriteBatch.End();
    }
}
