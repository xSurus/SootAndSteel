using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
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

    private GameplayContext gameplayContext;
    private Desktop desktop;
    private readonly WhiteFilterTransition whiteToHub = new WhiteFilterTransition();
    private Phase phase;

    public override void LoadContent()
    {
        base.LoadContent();

        gameplayContext = new GameplayContext(virtualScreenSize);
        Services.AddService(gameplayContext);

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
            Text = $"Level {ScreenPayloads.LastPostLevelResults.CompletedLevelNumber} complete",
            Font = Game.fontSystem.GetFont(72),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var coal = new Label
        {
            Text = $"Coal remaining: {ScreenPayloads.LastPostLevelResults.CoalRemaining}",
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
        var emitter = ParticleFactory.CreateSnowstorm(new Random());
        vfx.AddContinuous(emitter);
        phase = Phase.WaitInput;
    }

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        base.Update(gameTime, keyboard, gamePads);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        whiteToHub.Update(dt);

        if (phase == Phase.WaitInput)
        {
            bool pressedByInput = Game.playerManager.Configs.Any(c => c.Input.IsPickupJustPressed() || c.Input.IsStartJustPressed());
            bool pressedByKeyboard = (keyboard.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter)) ||
                                     (keyboard.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space));

            if (pressedByInput || pressedByKeyboard)
            {
                // TODO: Calculate reward based on level difficulty and coal remaining.
                int reward = 25 + ScreenPayloads.LastPostLevelResults.CoalRemaining * 3;
                Game.AddCredits(reward);
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
        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        base.UnloadContent();
    }
}

