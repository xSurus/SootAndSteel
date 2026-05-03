using System;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Particles;
using Gamelab.Particles.Modifiers;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class PostLevelStatsScreen : GamelabGameScreen
{
    private enum Phase
    {
        IntroReveal,
        WaitInput,
        FadeOutToHub,
    }

    private readonly float actualTime;
    private readonly float referenceTime;

    public PostLevelStatsScreen(GamelabGame game) : this(game, 1f, 1f) { }

    public PostLevelStatsScreen(GamelabGame game, float actualTime, float referenceTime) : base(game)
    {
        this.actualTime = actualTime;
        this.referenceTime = referenceTime;
    }

    private const float DimFadeSeconds = 0.6f;
    private const float CountUpSeconds = 1.1f;
    private const float CreditTickInterval = 0.10f;

    private Desktop desktop;
    private Label titleLabel;
    private Label deliveryLabel;
    private Label timeLineLabel;
    private Label dividerLabel;
    private Label totalLabel;
    private Label promptLabel;

    private readonly WhiteFilterTransition whiteToHub = new();
    private Phase phase;

    private ParticleEmitter snowstormEmitter;
    private SnowstormTransition.Baseline snowstormBaseline;

    private ISoundService soundService;
    private float introDim;
    private float revealClock;
    private bool playedTitleCue;
    private bool playedDeliveryCue;
    private bool playedTimeCue;
    private bool playedDividerCue;
    private int lastSoundAtTotal = -1;
    private float creditTickCooldown;

    private int deliveryReward;
    private int timeAdjustment;
    private int targetTotalCredits;
    private bool hasTimeLine;
    private Color timeLineColor;
    private string timeLineText;

    private int baseReward;
    private int expectedBonus;
    private int actualBonus;

    private bool countingTotal;
    private float countUpElapsed;
    private int displayedTotalCredits;
    private float titleColorBlendT;
    private float promptPulseTime;
    private float totalLineCelebrateT;

    public override void LoadContent()
    {
        base.LoadContent();

        soundService = Services.GetService<ISoundService>();

        baseReward = Game.GameplayConfig.LevelBaseReward;
        expectedBonus = Game.GameplayConfig.LevelReferenceBonus;
        float rawBonus = expectedBonus * MathF.Sqrt(referenceTime / Math.Max(actualTime, 0.1f));
        actualBonus = (int)MathF.Round(MathF.Max(0f, rawBonus));

        deliveryReward = baseReward + expectedBonus;
        timeAdjustment = actualBonus - expectedBonus;
        targetTotalCredits = deliveryReward + timeAdjustment;
        float timeDelta = actualTime - referenceTime;
        hasTimeLine = timeAdjustment != 0;
        bool faster = timeDelta < 0;
        timeLineText = faster
            ? $"{"Time Bonus:",-18}+{timeAdjustment}"
            : $"{"Time Penalty:",-18}{timeAdjustment}";
        timeLineColor = faster ? new Color(80, 200, 80) : new Color(220, 150, 50);

        var rootPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var stack = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 18,
        };

        titleLabel = new Label
        {
            Text = $"Stage Complete: {StageNaming.GetStageTitle(Game.CurrentRun.CurrentLevel)}",
            Font = Game.fontSystem.GetFont(72),
            TextColor = new Color(255, 210, 100),
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false,
        };

        deliveryLabel = new Label
        {
            Text = $"Delivery Reward:   +{deliveryReward}",
            Font = Game.fontSystem.GetFont(52),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false,
        };

        timeLineLabel = new Label
        {
            Text = timeLineText,
            Font = Game.fontSystem.GetFont(52),
            TextColor = timeLineColor,
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false,
        };

        dividerLabel = new Label
        {
            Text = "——————————————————",
            Font = Game.fontSystem.GetFont(40),
            TextColor = new Color(120, 120, 120),
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false,
        };

        totalLabel = new Label
        {
            Text = "Total:            +0",
            Font = Game.fontSystem.GetFont(60),
            TextColor = new Color(230, 200, 120),
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false,
        };

        promptLabel = new Label
        {
            Text = "Press Start / Continue",
            Font = Game.fontSystem.GetFont(40),
            TextColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false,
            Margin = new Thickness(0, 28, 0, 0),
        };

        stack.Widgets.Add(titleLabel);
        stack.Widgets.Add(deliveryLabel);
        if (hasTimeLine)
        {
            stack.Widgets.Add(timeLineLabel);
        }

        stack.Widgets.Add(dividerLabel);
        stack.Widgets.Add(totalLabel);
        stack.Widgets.Add(promptLabel);

        rootPanel.Widgets.Add(stack);
        desktop = new Desktop { Root = rootPanel };

        var vfx = Services.GetService<IVfxService>();
        snowstormEmitter = ParticleFactory.CreateSnowstorm();
        snowstormEmitter.Modifiers.Add(new BlizzardGustModifier(() =>
            phase == Phase.FadeOutToHub ? whiteToHub.Opacity : 0f));
        snowstormBaseline = SnowstormTransition.Capture(snowstormEmitter);
        vfx.AddContinuous(snowstormEmitter);
        phase = Phase.IntroReveal;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        whiteToHub.Update(dt);
        float blizzardT = phase == Phase.FadeOutToHub ? whiteToHub.Opacity : 0f;
        if (blizzardT > 0.0001f)
            SnowstormTransition.ApplyBlizzardIntensity(snowstormEmitter, snowstormBaseline, blizzardT);

        switch (phase)
        {
            case Phase.IntroReveal:
                UpdateIntroReveal(dt);
                break;
            case Phase.WaitInput:
                UpdateWaitInput(dt);
                if (Game.playerManager.Configs.Any(c =>
                        c.Input.IsPickupJustPressed() || c.Input.IsStartJustPressed()))
                {
                    soundService.PlayOnce(Sounds.MenuSelect);
                    Game.CurrentRun.AddCredits(baseReward + actualBonus);
                    whiteToHub.FadeIn(0.8f);
                    phase = Phase.FadeOutToHub;
                }

                break;
            case Phase.FadeOutToHub:
                if (whiteToHub.IsDone)
                {
                    Game.SwitchToScreen(new HubScreen(Game));
                }

                break;
        }
    }

    private void UpdateWaitInput(float dt)
    {
        promptPulseTime += dt;
        float w = 0.55f + 0.45f * MathF.Sin(promptPulseTime * 4f);
        promptLabel.TextColor = Color.LightBlue * w;
        titleColorBlendT = Math.Min(1f, titleColorBlendT + dt * 2.5f);
        titleLabel.TextColor = LerpColor(new Color(255, 210, 100), Color.White, SmoothStep(titleColorBlendT));

        if (totalLineCelebrateT > 0f)
        {
            totalLineCelebrateT -= dt;
            float punch = SmoothStep(Math.Clamp(totalLineCelebrateT / 0.22f, 0f, 1f));
            int size = (int)Math.Round(60 + 10 * punch);
            totalLabel.Font = Game.fontSystem.GetFont(size);
            totalLabel.TextColor = LerpColor(Color.White, new Color(230, 200, 120), 1f - punch);
        }
    }

    private void UpdateIntroReveal(float dt)
    {
        revealClock += dt;
        introDim = SmoothStep(Math.Clamp(revealClock / DimFadeSeconds, 0f, 1f));

        float t = revealClock;
        float tTitle = DimFadeSeconds;
        float tDelivery = tTitle + 0.45f;
        float tTime = tDelivery + 0.45f;
        float tDivider = hasTimeLine ? tTime + 0.38f : tDelivery + 0.45f;
        float tTotal = tDivider + 0.28f;

        if (t >= tTitle && !playedTitleCue)
        {
            playedTitleCue = true;
            titleLabel.Visible = true;
            soundService.PlayOnce(Sounds.Craft);
        }

        if (t >= tDelivery && !playedDeliveryCue)
        {
            playedDeliveryCue = true;
            deliveryLabel.Visible = true;
            soundService.PlayOnce(Sounds.PickupItem);
        }

        if (hasTimeLine && t >= tTime && !playedTimeCue)
        {
            playedTimeCue = true;
            timeLineLabel.Visible = true;
            soundService.PlayOnce(Sounds.PickupItem);
        }

        if (t >= tDivider && !playedDividerCue)
        {
            playedDividerCue = true;
            dividerLabel.Visible = true;
            soundService.PlayOnce(Sounds.PickupItem);
        }

        if (t >= tTotal && !countingTotal)
        {
            countingTotal = true;
            countUpElapsed = 0f;
            displayedTotalCredits = 0;
            totalLabel.Visible = true;
            lastSoundAtTotal = -1;
            creditTickCooldown = 0f;
        }

        if (countingTotal)
        {
            creditTickCooldown -= dt;
            countUpElapsed += dt;
            float u = SmoothStep(Math.Clamp(countUpElapsed / CountUpSeconds, 0f, 1f));
            int newShown = (int)Math.Round(targetTotalCredits * u);

            if (newShown > lastSoundAtTotal && creditTickCooldown <= 0f)
            {
                lastSoundAtTotal = newShown;
                creditTickCooldown = CreditTickInterval;
                soundService.PlayOnce(Sounds.PickupItem);
            }

            displayedTotalCredits = newShown;
            totalLabel.Text = $"Total:            +{displayedTotalCredits}";

            if (countUpElapsed >= CountUpSeconds)
            {
                displayedTotalCredits = targetTotalCredits;
                totalLabel.Text = $"Total:            +{displayedTotalCredits}";
                soundService.PlayOnce(Sounds.Craft);
                countingTotal = false;
                totalLineCelebrateT = 0.22f;
                totalLabel.Font = Game.fontSystem.GetFont(60);
                phase = Phase.WaitInput;
                promptLabel.Visible = true;
                promptPulseTime = 0f;
                titleColorBlendT = titleLabel.Visible ? 0.35f : 1f;
            }
        }

        if (playedTitleCue)
        {
            titleColorBlendT = Math.Min(1f, titleColorBlendT + dt * 2.2f);
            titleLabel.TextColor = LerpColor(new Color(255, 210, 100), Color.White, SmoothStep(titleColorBlendT));
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        Services.GetService<IVfxService>().Render(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        if (introDim > 0.001f)
        {
            spriteBatch.Draw(
                AssetManager.BlankTexture,
                new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.Black * (introDim * (100f / 255f)));
        }

        desktop.Render();
        float w = whiteToHub.Opacity;
        if (w > 0.001f)
        {
            spriteBatch.Draw(
                AssetManager.BlankTexture,
                new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * w);
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        Services.GetService<IVfxService>().ClearAll();
        base.UnloadContent();
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Color(
            (byte)Math.Round(a.R + (b.R - a.R) * t),
            (byte)Math.Round(a.G + (b.G - a.G) * t),
            (byte)Math.Round(a.B + (b.B - a.B) * t),
            (byte)Math.Round(a.A + (b.A - a.A) * t));
    }

    private static float SmoothStep(float t) => t * t * (3f - 2f * t);
}
