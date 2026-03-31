using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map;

public class WorldScroller
{
    private GameplayContext gameplayContext;
    private float scrollOffset;
    private Texture2D backgroundTexture;
    private int ScreenWidth => gameplayContext.ScreenWidth;
    private int ScreenHeight => gameplayContext.ScreenHeight;

    public WorldScroller(GraphicsDevice graphicsDevice, GameplayContext gameplayContext)
    {
        this.gameplayContext = gameplayContext;
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

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;
                if (x % config.WorldScrollerStripeSpacingPixels < config.WorldScrollerStripeThicknessPixels)
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
        scrollOffset -= gameplayContext.State.actualSpeed * deltaTime;
        if (scrollOffset <= -backgroundTexture.Width)
        {
            scrollOffset += backgroundTexture.Width;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int numCopies = (ScreenWidth / backgroundTexture.Width) + 3;
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