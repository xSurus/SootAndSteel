using System;
using System.Collections.Generic;
using Gamelab.Assets;
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

    private Desktop desktop;
    private readonly WhiteFilterTransition whiteToGameplay = new WhiteFilterTransition();
    private Phase phase;
    private float holdTimer;
    private float holdDuration;

    public override void LoadContent()
    {
        base.LoadContent();

        int nextLevelNumber = Game.CurrentLevel + 1;

        var overlay = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new SolidBrush(new Color(0, 0, 0, 110))
        };

        bool isFirstLevel = nextLevelNumber == 1;

        var content = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 20
        };

        content.Widgets.Add(new Label
        {
            Text = $"Next level {nextLevelNumber}",
            Font = Game.fontSystem.GetFont(92),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        if (isFirstLevel)
        {
            string[] tips =
            [
                "Keep the train moving — feed coal into the oven to keep the engine running.",
                "Use the speed lever to shift gears: Stopped → Default → Double → Quadruple.",
                "Enemies will attack from both sides, repair broken walls to keep the heat in.",
                "Craft ammo at the anvil and load the cannon to fight back.",
            ];

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
        }

        overlay.Widgets.Add(content);
        desktop = new Desktop { Root = overlay };

        var vfx = Services.GetService<IVfxService>();
        vfx.AddContinuous(ParticleFactory.CreateSnowstorm());
        phase = Phase.Hold;
        holdTimer = 0f;
        holdDuration = isFirstLevel ? 15f : 5f;
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
                if (holdTimer >= holdDuration)
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
        Services.GetService<IVfxService>().ClearAll();
        base.UnloadContent();
    }
}

