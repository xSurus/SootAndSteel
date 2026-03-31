using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Assets;

public static class AssetManager
{
    public static Texture2D BlankTexture { get; private set; }
    public static Texture2D PlayerTexture { get; private set; }
    public static Texture2D TileTexture { get; private set; }
    public static Texture2D EnemyTexture { get; private set; }

    public static void LoadContent(GraphicsDevice graphicsDevice)
    {
        // temporary textures used for player and stations before real assets are used

        // single white pixel that can be differently colored and scaled later for stations etc for testing
        BlankTexture = new Texture2D(graphicsDevice, 1, 1);
        BlankTexture.SetData([Color.White]);

        LoadPlayerTexture(graphicsDevice);
        LoadTileTexture(graphicsDevice);
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

    private static void LoadTileTexture(GraphicsDevice graphicsDevice)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        TileTexture = new Texture2D(graphicsDevice, tileSize, tileSize);
        Color[] data = new Color[tileSize * tileSize];
        for (int i = 0; i < data.Length; i++)
        {
            int x = i % tileSize;
            int y = i / tileSize;
            bool isBorder = x == 0 || y == 0 || x == tileSize - 1 || y == tileSize - 1;
            data[i] = isBorder ? Color.DarkGray : Color.Gray;
        }

        TileTexture.SetData(data);
    }

    public static void UnloadContent()
    {
        BlankTexture?.Dispose();
        BlankTexture = null;

        PlayerTexture?.Dispose();
        PlayerTexture = null;

        TileTexture?.Dispose();
        TileTexture = null;
        
        EnemyTexture?.Dispose();
        EnemyTexture = null;
    }
}
