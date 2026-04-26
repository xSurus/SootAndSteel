using System.Linq;
using Gamelab.Assets;
using Gamelab.Particles;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class PostLevelStatsScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private enum Phase
    {
        WaitInput,
        FadeOutToHub,
    }

    private Desktop desktop;
    private readonly WhiteFilterTransition whiteToHub = new WhiteFilterTransition();
    private Phase phase;

    public override void LoadContent()
    {
        base.LoadContent();

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
            Spacing = 22
        };

        var title = new Label
        {
            Text = $"Stage Complete: {StageNaming.GetStageTitle(Game.CurrentRun.CurrentLevel)}",
            Font = Game.fontSystem.GetFont(72),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var coal = new Label
        {
            Text = $"Level completed!",
            Font = Game.fontSystem.GetFont(56),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var hint = new Label
        {
            Text = "Press Start / Continue",
            Font = Game.fontSystem.GetFont(40),
            TextColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        stack.Widgets.Add(title);
        stack.Widgets.Add(coal);
        stack.Widgets.Add(hint);
        overlay.Widgets.Add(stack);

        desktop = new Desktop { Root = overlay };

        var vfx = Services.GetService<IVfxService>();
        var emitter = ParticleFactory.CreateSnowstorm();
        vfx.AddContinuous(emitter);
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
                int reward = 25;
                Game.CurrentRun.AddCredits(reward);
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