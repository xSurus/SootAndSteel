using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Map.Hub;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.Players;
using Gamelab.PhysicalEntities.Triggers;
using Gamelab.Services.Music;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class HubScreen(GamelabGame game) : AbstractGameScreen(game)
{
    public static void StartFromStation()
    {
        // Intentionally left blank (kept for call sites).
        // Hub currently has a single spawn scheme near its gate.
    }

    private List<Player> players;
    private GameplayContext gameplayContext;
    private HubMap hubMap;
    private Desktop desktop;
    private ExitZone gateToStationZone;
    private Rectangle gateMarker;

    private float accumulator;
    private ParticleEmitter hubSnowEmitter;
    private WhiteFilterTransition gateWhiteFilter;
    private bool isTransitioningToNextLevel;

    public override void LoadContent()
    {
        base.LoadContent();

        gameplayContext = new GameplayContext(virtualScreenSize);
        Services.AddService(gameplayContext);

        hubMap = new HubMap();

        players = [];
        // Hub entry/exit gate is at the bottom center.
        int gateWidth = 220;
        int gateHeight = 110;
        gateMarker = new Rectangle(
            hubMap.BoundsPixels.Center.X - gateWidth / 2,
            hubMap.BoundsPixels.Bottom - gateHeight,
            gateWidth,
            gateHeight
        );

        Vector2 spawn = new Vector2(gateMarker.Center.X, gateMarker.Top - 80);
        float spacing = 70f;

        int i = 0;
        foreach (var playerConfig in Game.playerManager.Configs)
        {
            players.Add(new Player(spawn + new Vector2(0f, i * spacing), playerConfig));
            i++;
        }

        gateToStationZone = new ExitZone(gameplayContext.PhysicsWorld, gateMarker);
        hubSnowEmitter = ParticleFactory.CreateSnowstorm(new Random());
        Services.GetService<IVfxService>().AddContinuous(hubSnowEmitter);

        desktop = new Desktop();
        desktop.Root = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        Services.GetService<IMusicService>()
            .FadeOutAndPlay("tmp_ambient", 2, repeating: true, volume: Game.MusicVolume);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (isTransitioningToNextLevel)
        {
            gateWhiteFilter?.Update(dt);
            if (gateWhiteFilter is { IsDone: true })
            {
                Game.SwitchToScreen(new NextLevelIntroScreen(Game));
            }
            return;
        }

        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            foreach (var player in players)
            {
                player.Update(Game.GameplayConfig.FixedTimeStep);
            }

            gameplayContext.PhysicsWorld.Step(Game.GameplayConfig.FixedTimeStep);
            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }

        if (gateToStationZone != null && gateToStationZone.HaveAllInside(players))
        {
            gateWhiteFilter ??= new WhiteFilterTransition();
            gateWhiteFilter.FadeIn(0.8f);
            isTransitioningToNextLevel = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(15, 15, 20));

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        hubMap.Draw(spriteBatch);
        spriteBatch.Draw(AssetManager.BlankTexture, gateMarker, new Color(40, 120, 90));
        Services.GetService<IVfxService>().Render(spriteBatch);
        foreach (var player in players)
        {
            player.Draw(spriteBatch);
        }
        spriteBatch.End();

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        desktop.Render();
        float gw = gateWhiteFilter?.Opacity ?? 0f;
        if (gw > 0.001f)
        {
            spriteBatch.Draw(
                AssetManager.BlankTexture,
                new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * gw);
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        gateToStationZone?.Dispose();
        gateToStationZone = null;

        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        hubMap?.Dispose();
        hubMap = null;
        base.UnloadContent();
    }
}

