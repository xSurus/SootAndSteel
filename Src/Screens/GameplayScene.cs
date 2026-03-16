using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Screens;

public class GameplayScene(GamelabGame game) : AbstractGameScreen(game)
{
    private List<Player> players;
    private Texture2D playerTexture;
    private World world;
    private const float PixelsPerMeter = Player.PixelsPerMeter;

    public override void LoadContent()
    {
        base.LoadContent();
        world = new World(Vector2.Zero);
        int textureSize = 128;
        playerTexture = new Texture2D(GraphicsDevice, textureSize, textureSize);
        Color[] data = new Color[textureSize * textureSize];
        Vector2 center = new Vector2(textureSize / 2f);
        float radius = textureSize / 2f;

        for (int y = 0; y < textureSize; y++) {
            for (int x = 0; x < textureSize; x++) {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                data[y * textureSize + x] = distance <= radius ? Color.White : Color.Transparent;
            }
        }
        playerTexture.SetData(data);

        // Distribute players around the center
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
            
            players.Add(new Player(config.PlayerIndex, playerBody, config.Input));
        }
        
        float worldWidth = virtualScreenSize.X / PixelsPerMeter;
        float worldHeight = virtualScreenSize.Y / PixelsPerMeter;
        
        world.CreateEdge(new Vector2(0, 0), new Vector2(worldWidth, 0));
        world.CreateEdge(new Vector2(0, worldHeight), new Vector2(worldWidth, worldHeight));
        world.CreateEdge(new Vector2(0, 0), new Vector2(0, worldHeight));
        world.CreateEdge(new Vector2(worldWidth, 0), new Vector2(worldWidth, worldHeight));
    }

    private float accumulator = 0f;
    private const float FixedTimeStep = 1f / 60f;

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        accumulator += Math.Min(dt, 0.25f); 
        while (accumulator >= FixedTimeStep)
        {
            foreach (Player player in players)
            {
                player.Input.Update(gameTime);
                player.Update();
            }

            world.Step(FixedTimeStep);
            accumulator -= FixedTimeStep;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        foreach (Player player in players)
        {
            player.Draw(spriteBatch, playerTexture);
        }
        
        spriteBatch.End();
        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        playerTexture?.Dispose();
        base.UnloadContent();
    }
}
