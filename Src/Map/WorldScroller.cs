using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map;

public class WorldScroller
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private Texture2D backgroundTexture;

    private readonly List<Vector2> tilePositions = new();
    private readonly List<int> tileTypes = new();
    private readonly Random random = Random.Shared;

    private int ScreenWidth => gameplayContext.ScreenWidth;
    private int ScreenHeight => gameplayContext.ScreenHeight;

    private int TileWidth => AssetManager.TrainTrackTexture[0].Width;
    private float centerY;

    public WorldScroller(GraphicsDevice graphicsDevice)
    {
        centerY = (gameplayContext.ScreenHeight / 2.0f) - (AssetManager.TrainTrackTexture[0].Height / 2.0f);
        int numTilesNeeded = (gameplayContext.ScreenWidth / TileWidth) + 3;
        for (int i = 0; i < numTilesNeeded; i++)
        {
            SpawnTile(i * TileWidth);
        }

        CreateBackgroundTexture(graphicsDevice);
    }

    private void CreateBackgroundTexture(GraphicsDevice graphicsDevice)
    {
        var config = GamelabGame.Instance.GameplayConfig;
        int textureWidth = config.ScrollerWorldTextureWidth;
        int textureHeight = ScreenHeight;
        backgroundTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
        Color[] data = new Color[textureWidth * textureHeight];
        Color baseColor = new Color(208, 232, 242);
        Array.Fill(data, baseColor);
        backgroundTexture.SetData(data);
    }

    private void SpawnTile(float xOffset)
    {
        tilePositions.Add(new Vector2(xOffset, centerY));
        tileTypes.Add(random.Next(0, 2));
    }

    public void Update(float deltaTime)
    {
        float speed = gameplayContext.State.actualSpeed;

        for (int i = 0; i < tilePositions.Count; i++)
        {
            tilePositions[i] = new Vector2(tilePositions[i].X - (speed * deltaTime), centerY + 20);
        }

        if (tilePositions.Count > 0 && tilePositions[0].X < -TileWidth)
        {
            float lastX = tilePositions[tilePositions.Count - 1].X;
            tilePositions.RemoveAt(0);
            tileTypes.RemoveAt(0);
            SpawnTile(lastX + TileWidth);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int numCopies = (ScreenWidth / backgroundTexture.Width) + 3;
        for (int i = -1; i < numCopies; i++)
        {
            float xPos = i * backgroundTexture.Width;
            spriteBatch.Draw(backgroundTexture, new Vector2(xPos, 0), Color.White);
        }

        for (int i = 0; i < tilePositions.Count; i++)
        {
            spriteBatch.Draw(AssetManager.TrainTrackTexture[tileTypes[i]], tilePositions[i], null, Color.White, 0f,
                Vector2.Zero, 1f, SpriteEffects.None, RenderUtility.BackgroundLayer);
        }
    }

    public void Dispose()
    {
        backgroundTexture?.Dispose();
    }
}