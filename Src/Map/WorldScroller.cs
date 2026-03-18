using Gamelab.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map;

public class WorldScroller
{
    private readonly GameplayConfig gameplayConfig;
    public float TrainSpeed { get; set; }
    private float scrollOffset = 0f;
    private Texture2D backgroundTexture;
    private int screenWidth;
    private int screenHeight;

    public WorldScroller(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight, GameplayConfig gameplayConfig)
    {
        this.gameplayConfig = gameplayConfig;
        TrainSpeed = gameplayConfig.WorldScrollerDefaultSpeed;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        CreateBackgroundTexture(graphicsDevice);
    }

    private void CreateBackgroundTexture(GraphicsDevice graphicsDevice)
    {
        int textureWidth = gameplayConfig.WorldScrollerPatternWidthPixels;
        int textureHeight = screenHeight;
        backgroundTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight);

        Color[] data = new Color[textureWidth * textureHeight];
        Color baseColor = new Color(gameplayConfig.WorldScrollerBaseColorR, gameplayConfig.WorldScrollerBaseColorG,
            gameplayConfig.WorldScrollerBaseColorB);
        Color stripeColor = new Color(gameplayConfig.WorldScrollerStripeColorR, gameplayConfig.WorldScrollerStripeColorG,
            gameplayConfig.WorldScrollerStripeColorB);

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;
                if (x % gameplayConfig.WorldScrollerStripeSpacingPixels < gameplayConfig.WorldScrollerStripeThicknessPixels)
                {
                    data[index] = stripeColor;
                }
                else
                {
                    data[index] = baseColor;
                }
            }
        }

        backgroundTexture.SetData(data);
    }

    public void Update(float deltaTime)
    {
        scrollOffset -= TrainSpeed * deltaTime;
        if (scrollOffset <= -backgroundTexture.Width)
        {
            scrollOffset += backgroundTexture.Width;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int numCopies = (screenWidth / backgroundTexture.Width) + 3;
        for (int i = -1; i < numCopies; i++)
        {
            float xPos = scrollOffset + (i * backgroundTexture.Width);
            spriteBatch.Draw(backgroundTexture, new Vector2(xPos, 0), Color.White);
        }
    }

    public void Dispose()
    {
        backgroundTexture?.Dispose();
    }
}