using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map;

public class WorldScroller
{
    public float TrainSpeed { get; set; } = 100f;
    private float scrollOffset = 0f;
    private Texture2D backgroundTexture;
    private int screenWidth;
    private int screenHeight;

    public WorldScroller(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        CreateBackgroundTexture(graphicsDevice);
    }

    private void CreateBackgroundTexture(GraphicsDevice graphicsDevice)
    {
        int textureWidth = 480;
        int textureHeight = screenHeight;
        backgroundTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight);

        Color[] data = new Color[textureWidth * textureHeight];
        Color baseColor = new Color(40, 40, 45);
        Color stripeColor = new Color(50, 50, 55);

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;
                if (x % 80 < 4)
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