using System;
using System.Collections.Generic;
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
    private const float PixelsPerMeter = Player.PixelsPerMeter;
    private TrainMap trainMap;
    private WorldScroller worldScroller;

    public override void LoadContent()
    {
        base.LoadContent();
        world = new World(Vector2.Zero);

        int tileSize = 80;
        int trainWidth = 8;
        int trainHeight = 6;

        trainMap = new TrainMap(trainWidth, trainHeight, tileSize, GraphicsDevice, world, PixelsPerMeter);

        Vector2 trainPosition = new Vector2(
            (virtualScreenSize.X - trainWidth * tileSize) / 2f,
            (virtualScreenSize.Y - trainHeight * tileSize) / 2f
        );
        trainMap.SetPosition(trainPosition);
        trainMap.PlaceObject(1, 1, new CoalResource());
        trainMap.PlaceObject(6, 1, new CoalOven());
        trainMap.PlaceObject(3, 2, new Counter());

        worldScroller = new WorldScroller(GraphicsDevice, virtualScreenSize.X, virtualScreenSize.Y);
        worldScroller.TrainSpeed = 150f;

        var spawnPositions = new Vector2[]
        {
            new Vector2(virtualScreenSize.X / 2f - 100, virtualScreenSize.Y / 2f - 100),
            new Vector2(virtualScreenSize.X / 2f + 100, virtualScreenSize.Y / 2f - 100),
            new Vector2(virtualScreenSize.X / 2f - 100, virtualScreenSize.Y / 2f + 100),
            new Vector2(virtualScreenSize.X / 2f + 100, virtualScreenSize.Y / 2f + 100),
        };

        players = [];
        foreach (var config in Game.playerManager.Configs)
        {
            Vector2 pixelPos = spawnPositions[config.PlayerIndex % spawnPositions.Length];
            Body playerBody = world.CreateCircle(24f / PixelsPerMeter, 3f, pixelPos / PixelsPerMeter, BodyType.Dynamic);
            playerBody.LinearDamping = 20f;
            playerBody.FixedRotation = true;

            players.Add(new Player(config.PlayerIndex, playerBody, config.Input, trainMap));
        }
    }

    private float accumulator = 0f;
    private const float FixedTimeStep = 1f / 60f;

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        worldScroller.Update(dt);
        trainMap.Update(dt);
        accumulator += Math.Min(dt, 0.25f);
        while (accumulator >= FixedTimeStep)
        {
            foreach (Player player in players)
            {
                player.Update();
            }

            world.Step(FixedTimeStep);
            accumulator -= FixedTimeStep;
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