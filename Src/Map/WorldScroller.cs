using Gamelab.Map.Train.State;
using Gamelab.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map;

public class WorldScroller
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private float scrollOffset;
    private Texture2D backgroundTexture;

    private System.Collections.Generic.List<Vector2> tilePositions = new System.Collections.Generic.List<Vector2>();
    private System.Collections.Generic.List<int> tileTypes = new System.Collections.Generic.List<int>();
    private System.Random randomizer = new System.Random();

    private int ScreenWidth => gameplayContext.ScreenWidth;
    private int ScreenHeight => gameplayContext.ScreenHeight;

    private float tileScale => 1.6f;
    private int TileWidth => (int) (AssetManager.TrainTrackTexture[0].Width * tileScale);
    private float posY;
    

    public WorldScroller(GraphicsDevice graphicsDevice)
    {
        posY = (gameplayContext.ScreenHeight / 2) - (AssetManager.TrainTrackTexture[0].Height / 2) -30;
        int numTilesNeeded = (gameplayContext.ScreenWidth /TileWidth) + 3;
        for (int i = 0; i < numTilesNeeded; i++)
        {
            SpawnTile(i * TileWidth);
        }
        CreateBackgroundTexture(graphicsDevice);
    }

    private void CreateBackgroundTexture(GraphicsDevice graphicsDevice)
    {
        var config = GamelabGame.Instance.GameplayConfig;
        int textureWidth = config.WorldScrollerPatternWidthPixels;
        int textureHeight = ScreenHeight;
        backgroundTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
        Color[] data = new Color[textureWidth * textureHeight];
        Color baseColor = new Color(config.WorldScrollerBaseColorR, config.WorldScrollerBaseColorG,
            config.WorldScrollerBaseColorB);
        Color stripeColor = new Color(config.WorldScrollerStripeColorR, config.WorldScrollerStripeColorG,
            config.WorldScrollerStripeColorB);
        baseColor = new Color(208,232,242);

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;
                data[index] = baseColor;
            }
        }

        backgroundTexture.SetData(data);
    }

    private void SpawnTile(float xOffset)
    {   
        
        tilePositions.Add(new Vector2(xOffset, posY));
        tileTypes.Add(randomizer.Next(0, 2));
    }

    public void Update(float deltaTime)
    {
        float speed = gameplayContext.State.actualSpeed;

        for (int i = 0; i < tilePositions.Count; i++)
        {
            tilePositions[i] = new Vector2(tilePositions[i].X - (speed * deltaTime), posY);
        }

        if (tilePositions.Count > 0 && tilePositions[0].X < -TileWidth)
        {
            float lastX = tilePositions[tilePositions.Count - 1].X;
            tilePositions.RemoveAt(0);
            tileTypes.RemoveAt(0);
            SpawnTile(lastX + TileWidth);
        }

        // scrollOffset -= gameplayContext.State.actualSpeed * deltaTime;
        // if (scrollOffset <= -backgroundTexture.Width)
        // {
        //     scrollOffset += backgroundTexture.Width;
        // }
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
            spriteBatch.Draw(AssetManager.TrainTrackTexture[tileTypes[i]], tilePositions[i], null, Color.White, 0f, Vector2.Zero, tileScale, SpriteEffects.None, 0f);
        }
    }

    public void Dispose()
    {
        backgroundTexture?.Dispose();
    }
}