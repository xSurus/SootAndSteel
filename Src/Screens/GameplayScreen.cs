using System;
using System.Collections.Generic;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Stations;
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
    private Desktop desktop;
    private Label coalLabel;
    private Label speedLabel;

    public override void LoadContent()
    {
        base.LoadContent();
        world = new World(Vector2.Zero);

        trainMap = new TrainMap(GraphicsDevice, world);
        trainContext = new TrainContext(trainMap, new TrainState());

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
            Body playerBody = world.CreateCircle(
                Game.GameplayConfig.PlayerRadiusPixels / Game.GameplayConfig.PixelsPerMeter,
                Game.GameplayConfig.PlayerDensity, pixelPos / Game.GameplayConfig.PixelsPerMeter, BodyType.Dynamic);
            playerBody.LinearDamping = Game.GameplayConfig.PlayerLinearDamping;
            playerBody.FixedRotation = true;

            players.Add(new Player(playerBody, playerConfig.Input, trainContext));
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
    }

    private float accumulator;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        trainContext.State.Update(dt);
        worldScroller.TrainSpeed = trainContext.State.actualSpeed;
        worldScroller.Update(dt);
        trainMap.Update(dt, trainContext);
        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            foreach (Player player in players)
            {
                player.Update();
            }

            world.Step(Game.GameplayConfig.FixedTimeStep);
            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }

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
        base.UnloadContent();
    }
}