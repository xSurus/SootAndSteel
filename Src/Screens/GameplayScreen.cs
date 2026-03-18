using System;
using System.Collections.Generic;
using Gamelab.Config;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Stations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Screens;

public class GameplayScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private List<Player> players;
    private World world;
    private GameplayConfig gameplayConfig;
    private float PixelsPerMeter => gameplayConfig.PixelsPerMeter;
    private TrainMap trainMap;
    private WorldScroller worldScroller;
    private TrainContext trainContext;
    private Desktop _desktop;
    private Label _coalLabel;
    private Label _speedLabel;

    public override void LoadContent()
    {
        base.LoadContent();
        gameplayConfig = Game.GameplayConfig;
        world = new World(Vector2.Zero);

        int tileSize = gameplayConfig.TrainTileSize;
        int trainWidth = gameplayConfig.TrainWidth;
        int trainHeight = gameplayConfig.TrainHeight;

        trainMap = new TrainMap(trainWidth, trainHeight, tileSize, GraphicsDevice, world, PixelsPerMeter);
        trainContext = new TrainContext(trainMap, new TrainState(gameplayConfig));

        Vector2 trainPosition = new Vector2(
            (virtualScreenSize.X - trainWidth * tileSize) / 2f,
            (virtualScreenSize.Y - trainHeight * tileSize) / 2f
        );
        trainMap.SetPosition(trainPosition);
        trainMap.PlaceObject(1, 1, new CoalResource());
        trainMap.PlaceObject(6, 1, new CoalOven(gameplayConfig));
        trainMap.PlaceObject(3, 2, new Counter());
        trainMap.PlaceObject(4, 2, new SpeedLever());

        worldScroller = new WorldScroller(GraphicsDevice, virtualScreenSize.X, virtualScreenSize.Y, gameplayConfig);

        var spawnPositions = new Vector2[]
        {
            new Vector2(virtualScreenSize.X / 2f - gameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f - gameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f + gameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f - gameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f - gameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f + gameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f + gameplayConfig.SpawnOffsetPixels,
                virtualScreenSize.Y / 2f + gameplayConfig.SpawnOffsetPixels),
        };

        players = [];
        foreach (var config in Game.playerManager.Configs)
        {
            Vector2 pixelPos = spawnPositions[config.PlayerIndex % spawnPositions.Length];
            Body playerBody = world.CreateCircle(gameplayConfig.PlayerRadiusPixels / PixelsPerMeter,
                gameplayConfig.PlayerDensity, pixelPos / PixelsPerMeter, BodyType.Dynamic);
            playerBody.LinearDamping = gameplayConfig.PlayerLinearDamping;
            playerBody.FixedRotation = true;

            players.Add(new Player(config.PlayerIndex, playerBody, config.Input, trainContext, gameplayConfig));
        }

        // Initialize Myra UI
        _desktop = new Desktop();
        var mainPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _coalLabel = new Label
        {
            Text = "Coal: 0",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20)
        };

        _speedLabel = new Label
        {
            Text = "Speed: 0",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 80, 20, 20)
        };

        mainPanel.Widgets.Add(_coalLabel);
        mainPanel.Widgets.Add(_speedLabel);
        _desktop.Root = mainPanel;
    }

    private float accumulator = 0f;

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        trainContext.State.Update(dt);
        worldScroller.TrainSpeed = trainContext.State.ActualSpeed;
        worldScroller.Update(dt);
        trainMap.Update(dt, trainContext);
        accumulator += Math.Min(dt, gameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= gameplayConfig.FixedTimeStep)
        {
            foreach (Player player in players)
            {
                player.Update();
            }

            world.Step(gameplayConfig.FixedTimeStep);
            accumulator -= gameplayConfig.FixedTimeStep;
        }

        // Update coal label
        _coalLabel.Text = $"Coal: {trainContext.State.CoalAmount}";
        _speedLabel.Text = $"Speed: {trainContext.State.ActualSpeed:F0}";
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
        _desktop.Render();
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