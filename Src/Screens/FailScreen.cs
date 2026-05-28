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
        soundService?.ResetGlobalParameters();
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
    private float soundInterval = 0.1f;
    private float soundTimer;

    private static string GetReasonText(FailureReason r) => r switch
    {
        FailureReason.AllPlayersKnockedOut =>
            "ALL WORKERS INCAPACITATED. ALL CARGO WAS STOLEN. TRAIN AND CREW WAS LEFT TO THE WEATHER",
        FailureReason.TrainFrozenHullBreached =>
            "TRAIN HULL WAS DESTROYED BY HORSE RIDERS, THE COLD CREPT IN"+
            "AND TOOK OUT THE CREW. ALL CARGO LOST",
        FailureReason.TrainFrozenFurnaceOut =>
            "OVEN COULD NOT BE KEPT BURNING, CREW GOT TAKEN OUT BY THE COLD SIBERIAN WINTER, " +
            "TRAIN WAS LEFT TO THE WEATHER.",
        FailureReason.TrainFrozenBreachesAndFurnaceOut =>
            "TRAIN WAS BREACHED, FURNANCE WAS FOUND OUT," +
            "CREW WAS WIPED OUT BY THE COLD. WILD ANIMALS TRACKS FOUND ON THE TRACKS, NO CREW FOUND ",
        FailureReason.TrainFrozenOther =>
            "NO INFORMATION AVAILABLE; NO CREW FOUND",
        _ => string.Empty
    };

    public override void LoadContent()
    {
        base.LoadContent();
        GumService.Default.Root.Children.Clear();
        soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.MenuSelect);
        soundService.LoadSound(Sounds.PickupItem);
        Services.GetService<IVfxService>().AddContinuous(ParticleFactory.CreateSnowstorm());

        overlay = new PostDeathOverlay();
        overlay.AddToRoot();
        overlay.ButtonWithIconInstance.ButtonText = "Return";
        XboxButtonGlyphs.ApplyFaceButton(overlay.ButtonWithIconInstance, XboxButtonAtlas.Face.A);

        overlay.EnemiesDefeatedStatText.SetValue(stats.EnemiesDefeated.ToString());
        overlay.DistanceTravelledStatText.SetValue($"{(int)Math.Round(stats.DistanceTravelledMeters)} m");
        overlay.StagesDefeatedStatText.SetValue(Math.Max(0, stats.StagesDefeated).ToString());
        overlay.UpgradesBoughtStatText.SetValue(Math.Max(0, stats.UpgradesBought).ToString());

        fullCauseText = GetReasonText(reason);
        typedCharacters = 0f;
        overlay.SetCauseOfFailure(string.Empty);
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
        overlay.SetCauseOfFailure(fullCauseText);
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

    private void UpdateCauseTypewriter(float dt)
    {
        if (overlay == null || typedCharacters >= fullCauseText.Length)
            return;

        typedCharacters = Math.Min(fullCauseText.Length, typedCharacters + dt * CauseTypewriterCharsPerSecond);
        int count = Math.Clamp((int)typedCharacters, 0, fullCauseText.Length);
        overlay.SetCauseOfFailure(fullCauseText[..count]);

        soundTimer += dt;
        if (soundTimer >= soundInterval)
        {
            soundService.PlayOnce(Sounds.PickupItem);
            soundTimer -= soundInterval;
        }
    }

}

