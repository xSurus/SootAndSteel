using System;
using System.Collections.Generic;
using Gamelab.Interactable.Stations;
using Gamelab.Interactable.Stations.Resources;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Services.Music;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Screens;

public class GameplayScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private List<Player> players;
    private World world;
    private TrainMap trainMap;
    private WorldScroller worldScroller;
    private TrainContext trainContext;
    private TrainSound trainSound;
    private Desktop desktop;
    private Label coalLabel;
    private Label speedLabel;

    public override void LoadContent()
    {
        base.LoadContent();
        world = new World(Vector2.Zero);

        trainMap = new TrainMap(GraphicsDevice, world);
        trainContext = new TrainContext(trainMap, new TrainState());
        trainSound = new TrainSound(Services.GetService<ISoundService>());

        Vector2 trainPosition = new Vector2(
            (virtualScreenSize.X - trainMap.Width * trainMap.TileSize) / 2f,
            (virtualScreenSize.Y - trainMap.Height * trainMap.TileSize) / 2f
        );
        trainMap.SetPosition(trainPosition);
        trainMap.PlaceObject(1, 1, new CoalResource());
        trainMap.PlaceObject(6, 1, new CoalOven());
        trainMap.PlaceObject(3, 2, new Counter());
        trainMap.PlaceObject(4, 2, new SpeedLever());

        worldScroller = new WorldScroller(GraphicsDevice, virtualScreenSize.X, virtualScreenSize.Y);

        var spawnPositions = new[]
        {
            new Vector2(virtualScreenSize.X / 2f - Game.GameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f - Game.GameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f + Game.GameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f - Game.GameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f - Game.GameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f + Game.GameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f + Game.GameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f + Game.GameplayConfig.SpawnOffsetPixels),
        };

        players = [];
        foreach (var playerConfig in Game.playerManager.Configs)
        {
            Vector2 pixelPos = spawnPositions[playerConfig.PlayerIndex % spawnPositions.Length];
            players.Add(new Player(world, pixelPos, playerConfig, trainContext));
        }

        // Initialize Myra UI
        desktop = new Desktop();
        var mainPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        coalLabel = new Label
        {
            Text = "Coal: 0",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20)
        };

        speedLabel = new Label
        {
            Text = "Speed: 0",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 80, 20, 20)
        };

        mainPanel.Widgets.Add(coalLabel);
        mainPanel.Widgets.Add(speedLabel);
        desktop.Root = mainPanel;
        
        Services.GetService<IMusicService>().FadeOutAndPlay("tmp_ambient", 2, repeating: true, volume: Game.MusicVolume);
    }

    private float accumulator;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        worldScroller.TrainSpeed = trainContext.State.actualSpeed;
        worldScroller.Update(dt);
        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            foreach (Player player in players)
            {
                player.Update(Game.GameplayConfig.FixedTimeStep);
            }

            trainContext.State.Update(Game.GameplayConfig.FixedTimeStep);
            trainMap.Update(Game.GameplayConfig.FixedTimeStep, trainContext);
            world.Step(Game.GameplayConfig.FixedTimeStep);
            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }
        
        trainSound.Update(gameTime, worldScroller.TrainSpeed);

        // Update coal label
        coalLabel.Text = $"Coal: {trainContext.State.CoalAmount}";
        speedLabel.Text = $"Speed: {trainContext.State.actualSpeed:F0}";
    }

    public override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        worldScroller.Draw(spriteBatch);
        trainMap.Draw(spriteBatch);

        foreach (Player player in players)
        {
            player.Draw(spriteBatch);
        }

        spriteBatch.End();

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        desktop.Render();
        spriteBatch.End();

        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        trainMap?.Dispose();
        worldScroller?.Dispose();
        trainSound?.Dispose();
        base.UnloadContent();
    }
}