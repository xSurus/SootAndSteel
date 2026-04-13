using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Assets;

public static class AssetManager
{
    public static Texture2D BlankTexture { get; private set; }
    public static Texture2D PlayerTexture { get; private set; }
    public static Texture2D TileTexture { get; private set; }
    public static Texture2D[] TrainTrackTexture { get; private set; }
    public static Texture2D EnemyTexture { get; private set; }
    public static Texture2D SmokeTexture { get; private set; }
    public static Texture2D SparkTexture { get; private set; }

    public static void LoadContent(GraphicsDevice graphicsDevice)
    {
        // temporary textures used for player and stations before real assets are used

        // single white pixel that can be differently colored and scaled later for stations etc for testing
        BlankTexture = new Texture2D(graphicsDevice, 1, 1);
        BlankTexture.SetData([Color.White]);

        LoadPlayerTexture(graphicsDevice);
        LoadTileTexture(graphicsDevice);
        LoadTrainTrackTexture(graphicsDevice);
        LoadParticleTextures(graphicsDevice);
        // TODO add texture loading from json
    }

    private static void LoadPlayerTexture(GraphicsDevice graphicsDevice)
    {
        // player texture
        int textureSize = 128;
        PlayerTexture = new Texture2D(graphicsDevice, textureSize, textureSize);
        Color[] data = new Color[textureSize * textureSize];
        Vector2 center = new Vector2(textureSize / 2f);
        float radius = textureSize / 2f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                data[y * textureSize + x] = distance <= radius ? Color.White : Color.Transparent;
            }
        }

        PlayerTexture.SetData(data);

        // enemy texture (square with darker center)
        int enemyTextureSize = 64;
        EnemyTexture = new Texture2D(graphicsDevice, enemyTextureSize, enemyTextureSize);
        Color[] enemyData = new Color[enemyTextureSize * enemyTextureSize];

        for (int y = 0; y < enemyTextureSize; y++)
        {
            for (int x = 0; x < enemyTextureSize; x++)
            {
                int border = 4;
                bool isBorder = x < border || x >= enemyTextureSize - border ||
                                y < border || y >= enemyTextureSize - border;
                enemyData[y * enemyTextureSize + x] = isBorder ? Color.DarkRed : Color.White;
            }
        }

        EnemyTexture.SetData(enemyData);
    }

    private static Texture2D LoadTexture(GraphicsDevice gd, string name)
    {
        string path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Assets", name);
        
        if (!System.IO.File.Exists(path))
        {
            Texture2D errorTex = new Texture2D(gd, 1, 1);
            errorTex.SetData(new Color[] { Color.HotPink });
            return errorTex;
        }

        using (System.IO.Stream stream = System.IO.File.OpenRead(path))
        {
            return Texture2D.FromStream(gd, stream);
        }
    }

    private static void LoadTrainTrackTexture(GraphicsDevice graphicsDevice)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        
        TrainTrackTexture = new Texture2D[2];
        TrainTrackTexture[0] = LoadTexture(graphicsDevice, "Rail_Tile_01.png");
        TrainTrackTexture[1] = LoadTexture(graphicsDevice, "Rail_Tile_02.png");
    }

    private static void LoadTileTexture(GraphicsDevice graphicsDevice)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        
        // TileTexture = new Texture2D[2];
        TileTexture = LoadTexture(graphicsDevice, "Train_Tile_A.png");
        // TileTexture[1] = LoadTexture(graphicsDevice, "Train_Tile_B.png");
    }

    private static void LoadParticleTextures(GraphicsDevice graphicsDevice)
    {
        int smokeSize = 64;
        SmokeTexture = new Texture2D(graphicsDevice, smokeSize, smokeSize);
        Color[] smokeData = new Color[smokeSize * smokeSize];
        Vector2 smokeCenter = new Vector2(smokeSize / 2f);
        float smokeRadius = smokeSize / 2f;

        for (int y = 0; y < smokeSize; y++)
        {
            for (int x = 0; x < smokeSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), smokeCenter);
                float alpha = Math.Clamp(1f - (distance / smokeRadius), 0f, 1f);
                smokeData[y * smokeSize + x] = new Color(Color.White, alpha);
            }
        }

        SmokeTexture.SetData(smokeData);

        int sparkSize = 8;
        SparkTexture = new Texture2D(graphicsDevice, sparkSize, sparkSize);
        Color[] sparkData = new Color[sparkSize * sparkSize];
        Vector2 sparkCenter = new Vector2(sparkSize / 2f);
        float sparkRadius = sparkSize / 2f;

        for (int y = 0; y < sparkSize; y++)
        {
            for (int x = 0; x < sparkSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), sparkCenter);
                sparkData[y * sparkSize + x] = distance <= sparkRadius ? Color.White : Color.Transparent;
            }
        }

        SparkTexture.SetData(sparkData);
    }

    public static void UnloadContent()
    {
        BlankTexture?.Dispose();
        BlankTexture = null;

        PlayerTexture?.Dispose();
        PlayerTexture = null;

        TileTexture?.Dispose();
        TileTexture = null;

        // TileTexture[0]?.Dispose();
        // TileTexture[0] = null;
        
        // TileTexture[1]?.Dispose();
        // TileTexture[1] = null;

        TrainTrackTexture[0]?.Dispose();
        TrainTrackTexture[0] = null;
        
        TrainTrackTexture[1]?.Dispose();
        TrainTrackTexture[1] = null;

        EnemyTexture?.Dispose();
        EnemyTexture = null;

        SparkTexture?.Dispose();
        SparkTexture = null;

        SmokeTexture?.Dispose();
        SmokeTexture = null;
    }
}