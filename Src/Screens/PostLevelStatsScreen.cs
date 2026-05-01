using System;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Particles;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class PostLevelStatsScreen : GamelabGameScreen
{
    private enum Phase
    {
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

    private Desktop desktop;
    private readonly WhiteFilterTransition whiteToHub = new WhiteFilterTransition();
    private Phase phase;

    private int baseReward;
    private int expectedBonus;
    private int actualBonus;

    public override void LoadContent()
    {
        base.LoadContent();

        baseReward = Game.GameplayConfig.LevelBaseReward;
        expectedBonus = Game.GameplayConfig.LevelReferenceBonus;
        float rawBonus = expectedBonus * MathF.Sqrt(referenceTime / Math.Max(actualTime, 0.1f));
        actualBonus = (int)MathF.Round(MathF.Max(0f, rawBonus));

        int deliveryReward = baseReward + expectedBonus;
        int timeAdjustment = actualBonus - expectedBonus;
        float timeDelta = actualTime - referenceTime;

        var overlay = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new SolidBrush(new Color(0, 0, 0, 90))
        };

        var stack = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 18
        };

        stack.Widgets.Add(new Label
        {
            Text = $"Stage Complete: {StageNaming.GetStageTitle(Game.CurrentRun.CurrentLevel)}",
            Font = Game.fontSystem.GetFont(72),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Widgets.Add(new Label
        {
            Text = $"Delivery Reward:   +{deliveryReward}",
            Font = Game.fontSystem.GetFont(52),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        if (timeAdjustment != 0)
        {
            bool faster = timeDelta < 0;
            string label = faster ? "Time Bonus:" : "Time Penalty:";
            string sign = faster ? "+" : "";
            Color color = faster ? new Color(80, 200, 80) : new Color(220, 150, 50);

            stack.Widgets.Add(new Label
            {
                Text = $"{label,-18}{sign}{timeAdjustment}",
                Font = Game.fontSystem.GetFont(52),
                TextColor = color,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

        stack.Widgets.Add(new Label
        {
            Text = "──────────────────",
            Font = Game.fontSystem.GetFont(52),
            TextColor = new Color(120, 120, 120),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Widgets.Add(new Label
        {
            Text = $"Total:            +{deliveryReward + timeAdjustment}",
            Font = Game.fontSystem.GetFont(60),
            TextColor = new Color(230, 200, 120),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Widgets.Add(new Label
        {
            Text = "Press Start / Continue",
            Font = Game.fontSystem.GetFont(40),
            TextColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        overlay.Widgets.Add(stack);
        desktop = new Desktop { Root = overlay };

        var vfx = Services.GetService<IVfxService>();
        vfx.AddContinuous(ParticleFactory.CreateSnowstorm());
        phase = Phase.WaitInput;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        whiteToHub.Update(dt);

        if (phase == Phase.WaitInput)
        {
            if (Game.playerManager.Configs.Any(c => c.Input.IsPickupJustPressed() || c.Input.IsStartJustPressed()))
            {
                Game.CurrentRun.AddCredits(baseReward + actualBonus);
                whiteToHub.FadeIn(0.8f);
                phase = Phase.FadeOutToHub;
            }
        }

        if (phase == Phase.FadeOutToHub && whiteToHub.IsDone)
        {
            Game.SwitchToScreen(new HubScreen(Game));
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
}