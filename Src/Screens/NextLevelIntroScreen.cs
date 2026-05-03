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

public class NextLevelIntroScreen(GamelabGame game) : GamelabGameScreen(game)
{
    private enum Phase
    {
        Hold,
        FadeOutToGameplay,
    }

    private Desktop desktop;
    private readonly WhiteFilterTransition whiteToGameplay = new WhiteFilterTransition();
    private Phase phase;
    private ISoundService soundService;
    private ParticleEmitter snowstormEmitter;
    private SnowstormTransition.Baseline snowstormBaseline;

    public override void LoadContent()
    {
        base.LoadContent();

        int stageNumber = Game.CurrentRun.CurrentLevel + 1;

        var overlay = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new SolidBrush(new Color(0, 0, 0, 110))
        };

        string[] tips = GetTipsForLevel(stageNumber);

        var content = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 20
        };

        content.Widgets.Add(new Label
        {
            Text = StageNaming.GetStageTitle(stageNumber),
            Font = Game.fontSystem.GetFont(92),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        foreach (string tip in tips)
        {
            content.Widgets.Add(new Label
            {
                Text = tip,
                Font = Game.fontSystem.GetFont(28),
                TextColor = new Color(200, 210, 230),
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

        content.Widgets.Add(new Label
        {
            Text = "Press Start / Enter to continue",
            Font = Game.fontSystem.GetFont(34),
            TextColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 18, 0, 0)
        });

        overlay.Widgets.Add(content);
        desktop = new Desktop { Root = overlay };

        var vfx = Services.GetService<IVfxService>();
        snowstormEmitter = ParticleFactory.CreateSnowstorm();
        snowstormEmitter.Modifiers.Add(new BlizzardGustModifier(() =>
            phase == Phase.FadeOutToGameplay ? whiteToGameplay.Opacity : 0f));
        snowstormBaseline = SnowstormTransition.Capture(snowstormEmitter);
        vfx.AddContinuous(snowstormEmitter);
        soundService = Services.GetService<ISoundService>();
        phase = Phase.Hold;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        whiteToGameplay.Update(dt);
        float blizzardT = phase == Phase.FadeOutToGameplay ? whiteToGameplay.Opacity : 0f;
        if (blizzardT > 0.0001f)
            SnowstormTransition.ApplyBlizzardIntensity(snowstormEmitter, snowstormBaseline, blizzardT);

        switch (phase)
        {
            case Phase.Hold:
                if (Game.playerManager.Configs.Any(c => c.Input.IsPickupJustPressed() || c.Input.IsStartJustPressed()))
                {
                    soundService.PlayOnce(Sounds.MenuSelect);
                    whiteToGameplay.FadeIn(0.8f);
                    phase = Phase.FadeOutToGameplay;
                }

                break;
            case Phase.FadeOutToGameplay:
                if (whiteToGameplay.IsDone)
                {
                    Game.CurrentRun.CurrentLevel++;
                    Game.SwitchToScreen(new GameplayScreen(Game));
                }

                break;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        Services.GetService<IVfxService>().Render(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        desktop.Render();
        float w = whiteToGameplay.Opacity;
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

    private static string[] GetTipsForLevel(int levelNumber) => levelNumber switch
    {
        1 =>
        [
            "Keep the train moving — feed coal into the oven to keep the engine running.",
            "Use the speed lever to shift gears: Stopped → Default → Double → Quadruple.",
            "Enemies will attack from both sides, repair broken walls to keep the heat in.",
            "Combine projectile, casing, and propellant at a workbench to craft bullets.",
            "Upgrade bullets by combining components with unlocked upgrades.",
        ],
        _ => []
    };

    public override void UnloadContent()
    {
        Services.GetService<IVfxService>().ClearAll();
        base.UnloadContent();
    }
}