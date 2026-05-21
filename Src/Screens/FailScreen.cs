using System;
using System.Text;
using Gamelab.Components;
using Gamelab.Particles;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using MonoGameGum;

namespace Gamelab.Screens;


public enum FailureReason
{
    
    AllPlayersKnockedOut,

    
    TrainFrozenHullBreached,

   
    TrainFrozenFurnaceOut,

    
    TrainFrozenBreachesAndFurnaceOut,

    
    TrainFrozenOther,
}

public readonly record struct PostDeathStatsSnapshot(
    int EnemiesDefeated,
    float DistanceTravelledMeters,
    int StagesDefeated,
    int UpgradesBought
);

public class FailScreen(GamelabGame game, FailureReason reason, PostDeathStatsSnapshot? stats = null) : GamelabGameScreen(game)
{
    public FailScreen(GamelabGame game) : this(game, FailureReason.TrainFrozenOther)
    {
    }

    private enum Phase
    {
        WaitingForReturn,
        Stamping,
    }

    private const float StampRevealSeconds = 0.20f;
    private const float ReturnDelayAfterStampSeconds = 0.90f;
    private const float CauseTypewriterCharsPerSecond = 55f;

    private readonly PostDeathStatsSnapshot stats = stats ?? default;
    private PostDeathOverlay overlay;
    private StampRevealAnimator fileClosedStampAnimator;
    private Phase phase = Phase.WaitingForReturn;
    private float phaseTimer;
    private ISoundService soundService;
    private string fullCauseText = string.Empty;
    private float typedCharacters;

    private static string GetReasonText(FailureReason r) => r switch
    {
        FailureReason.AllPlayersKnockedOut =>
            "ALL HANDS INCAPACITATED. CREW UNABLE TO KEEP THE ENGINE FIRED, " +
            "AND NO ONE REMAINED TO STOKE THE FURNACE OR SECURE THE CARGO.",
        FailureReason.TrainFrozenHullBreached =>
            "HULL BREACHES SPREAD FROST THROUGH THE CARS. COLD AIR POURED IN " +
            "FASTER THAN THE CREW COULD PATCH, AND THE ENGINE LOCKED SOLID.",
        FailureReason.TrainFrozenFurnaceOut =>
            "FURNACE FIRE EXTINGUISHED. WITH NO HEAT RETURNED TO THE BOILER, " +
            "PRESSURE COLLAPSED AND THE TRAIN WENT COLD BEFORE ARRIVAL.",
        FailureReason.TrainFrozenBreachesAndFurnaceOut =>
            "MULTIPLE BREACHES AND A DEAD FURNACE. WITH HEAT LOST INSIDE AND " +
            "OUT, THE TRAIN FROZE IN TRANSIT LONG BEFORE REACHING STATION.",
        FailureReason.TrainFrozenOther =>
            "CRITICAL THERMAL FAILURE. CORE SYSTEMS LOST TEMPERATURE CONTROL, " +
            "AND THE TRAIN FROZE BEFORE DELIVERY COULD BE COMPLETED.",
        _ => string.Empty
    };

    public override void LoadContent()
    {
        base.LoadContent();
        GumService.Default.Root.Children.Clear();
        soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.MenuSelect);
        Services.GetService<IVfxService>().AddContinuous(ParticleFactory.CreateSnowstorm());

        overlay = new PostDeathOverlay();
        overlay.AddToRoot();
        overlay.ButtonWithIconInstance.ButtonText = "Return";
        XboxButtonGlyphs.ApplyFaceButton(overlay.ButtonWithIconInstance, XboxButtonAtlas.Face.A);

        overlay.EnemiesDefeatedStatStatText = stats.EnemiesDefeated.ToString();
        overlay.DistanceTravelledStatStatText = $"{(int)Math.Round(stats.DistanceTravelledMeters)} m";
        overlay.StagesDefeatedStatStatText = Math.Max(0, stats.StagesDefeated).ToString();
        overlay.UpgradesBoughtStatStatText = Math.Max(0, stats.UpgradesBought).ToString();

        fullCauseText = WrapReportText(GetReasonText(reason), 62);
        typedCharacters = 0f;
        overlay.CauseOfFailureDescriptionText = string.Empty;
        overlay.IncidentTitleDetail.Text = BuildIncidentLine();

        if (overlay.FileClosedStamp != null)
        {
            fileClosedStampAnimator = new StampRevealAnimator(
                overlay.FileClosedStamp,
                StampRevealSeconds,
                popScale: 1.4f,
                popRotationOffset: -8f);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateCauseTypewriter(dt);
        switch (phase)
        {
            case Phase.WaitingForReturn:
                if (Game.playerManager.Configs.AnyPressedMenuConfirm())
                    BeginStampAndReturn();
                break;
            case Phase.Stamping:
                fileClosedStampAnimator?.Update(dt);
                phaseTimer += dt;
                if (phaseTimer >= StampRevealSeconds + ReturnDelayAfterStampSeconds)
                    Game.SwitchToScreen(new global::Gamelab.MainMenuScreen(Game));
                break;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 0, 0));

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        Services.GetService<IVfxService>().Render(spriteBatch);
        spriteBatch.End();

        GumService.Default.Draw();
        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        Services.GetService<IVfxService>().ClearAll();
        if (overlay?.Visual != null && GumService.Default.Root.Children.Contains(overlay.Visual))
            GumService.Default.Root.Children.Remove(overlay.Visual);
        base.UnloadContent();
    }

    private void BeginStampAndReturn()
    {
        phase = Phase.Stamping;
        phaseTimer = 0f;
        typedCharacters = fullCauseText.Length;
        overlay.CauseOfFailureDescriptionText = fullCauseText;
        soundService.PlayOnce(Sounds.MenuSelect);
        fileClosedStampAnimator?.Trigger();
    }

    private string BuildIncidentLine()
    {
        string levelTag = $"N. {Math.Max(1, Game.CurrentRun.CurrentLevel):00} / F";
        string dateTag = $"FILED {DateTime.Now:dd MMM yyyy}".ToUpperInvariant();
        string timeTag = DateTime.Now.ToString("HH:mm");
        return $"{levelTag} · {dateTag} · {timeTag}";
    }

    private static string WrapReportText(string text, int maxLineLength)
    {
        if (string.IsNullOrWhiteSpace(text) || maxLineLength <= 0)
            return string.Empty;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var builder = new StringBuilder(text.Length + 32);
        int currentLength = 0;
        foreach (string word in words)
        {
            int extra = currentLength == 0 ? word.Length : word.Length + 1;
            if (currentLength > 0 && currentLength + extra > maxLineLength)
            {
                builder.Append('\n');
                builder.Append(word);
                currentLength = word.Length;
            }
            else
            {
                if (currentLength > 0)
                {
                    builder.Append(' ');
                    currentLength++;
                }
                builder.Append(word);
                currentLength += word.Length;
            }
        }

        return builder.ToString();
    }

    private void UpdateCauseTypewriter(float dt)
    {
        if (overlay == null || typedCharacters >= fullCauseText.Length)
            return;

        typedCharacters = Math.Min(fullCauseText.Length, typedCharacters + dt * CauseTypewriterCharsPerSecond);
        int count = Math.Clamp((int)typedCharacters, 0, fullCauseText.Length);
        overlay.CauseOfFailureDescriptionText = fullCauseText[..count];
    }

}

