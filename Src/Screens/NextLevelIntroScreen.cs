using System;
using System.Collections.Generic;
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

public class NextLevelIntroScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private enum Phase
    {
        Hold,
        FadeOutToGameplay,
    }

    private GameplayContext gameplayContext;
    private Desktop desktop;
    private readonly WhiteFilterTransition whiteToGameplay = new WhiteFilterTransition();
    private Phase phase;
    private float holdTimer;

    public override void LoadContent()
    {
        base.LoadContent();

        gameplayContext = new GameplayContext(virtualScreenSize);
        Services.AddService(gameplayContext);

        int nextLevelNumber = Game.CurrentLevel + 1;

        var overlay = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new SolidBrush(new Color(0, 0, 0, 110))
        };

        var label = new Label
        {
            Text = $"Next level {nextLevelNumber}",
            Font = Game.fontSystem.GetFont(92),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        overlay.Widgets.Add(label);
        desktop = new Desktop { Root = overlay };

        var vfx = Services.GetService<IVfxService>();
        var emitter = ParticleFactory.CreateSnowstorm(new Random());
        ParticleFactory.ApplySnowstormSteadyIntensity(emitter, 1f);
        vfx.AddContinuous(emitter);
        phase = Phase.Hold;
        holdTimer = 0f;
    }

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        base.Update(gameTime, keyboard, gamePads);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        whiteToGameplay.Update(dt);

        switch (phase)
        {
            case Phase.Hold:
                holdTimer += dt;
                if (holdTimer >= 1.1f)
                {
                    whiteToGameplay.FadeIn(0.8f);
                    phase = Phase.FadeOutToGameplay;
                }
                break;
            case Phase.FadeOutToGameplay:
                if (whiteToGameplay.IsDone)
                {
                    Game.CurrentLevel++;
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

    public override void UnloadContent()
    {
        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        base.UnloadContent();
    }
}

