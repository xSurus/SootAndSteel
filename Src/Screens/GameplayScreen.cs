using System;
using System.Collections.Generic;
using Gamelab.Config;
using Gamelab.Map;
using Gamelab.Players;
using Gamelab.Stations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

    public override void LoadContent()
    {
        base.LoadContent();
        gameplayConfig = Game.GameplayConfig;
        world = new World(Vector2.Zero);

        int tileSize = gameplayConfig.TrainTileSize;
        int trainWidth = gameplayConfig.TrainWidth;
        int trainHeight = gameplayConfig.TrainHeight;

        trainMap = new TrainMap(trainWidth, trainHeight, tileSize, GraphicsDevice, world, PixelsPerMeter);

        Vector2 trainPosition = new Vector2(
            (virtualScreenSize.X - trainWidth * tileSize) / 2f,
            (virtualScreenSize.Y - trainHeight * tileSize) / 2f
        );
        trainMap.SetPosition(trainPosition);
        trainMap.PlaceObject(1, 1, new CoalResource());
        trainMap.PlaceObject(6, 1, new CoalOven(gameplayConfig));
        trainMap.PlaceObject(3, 2, new Counter());

        worldScroller = new WorldScroller(GraphicsDevice, virtualScreenSize.X, virtualScreenSize.Y, gameplayConfig);
        worldScroller.TrainSpeed = gameplayConfig.TrainSpeed;

        var spawnPositions = new Vector2[]
        {
            new Vector2(virtualScreenSize.X / 2f - gameplayConfig.SpawnOffsetPixels, virtualScreenSize.Y / 2f - gameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f + gameplayConfig.SpawnOffsetPixels, virtualScreenSize.Y / 2f - gameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f - gameplayConfig.SpawnOffsetPixels, virtualScreenSize.Y / 2f + gameplayConfig.SpawnOffsetPixels),
            new Vector2(virtualScreenSize.X / 2f + gameplayConfig.SpawnOffsetPixels, virtualScreenSize.Y / 2f + gameplayConfig.SpawnOffsetPixels),
        };

        players = [];
        foreach (var config in Game.playerManager.Configs)
        {
            Vector2 pixelPos = spawnPositions[config.PlayerIndex % spawnPositions.Length];
            Body playerBody = world.CreateCircle(gameplayConfig.PlayerRadiusPixels / PixelsPerMeter, gameplayConfig.PlayerDensity, pixelPos / PixelsPerMeter, BodyType.Dynamic);
            playerBody.LinearDamping = gameplayConfig.PlayerLinearDamping;
            playerBody.FixedRotation = true;

            players.Add(new Player(config.PlayerIndex, playerBody, config.Input, trainMap, gameplayConfig));
        }
    }

    private float accumulator = 0f;

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        worldScroller.Update(dt);
        trainMap.Update(dt);
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
        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        trainMap?.Dispose();
        worldScroller?.Dispose();
        base.UnloadContent();
    }
}