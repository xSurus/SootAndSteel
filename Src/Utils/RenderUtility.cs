using System;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace Gamelab.Utils;

public static class RenderUtility
{
    public const float BackgroundLayer = 0.01f;
    public const float FloorLayer = 0.05f;

    public const float TopEntityLayer = 0.9f;
    public const float ParticlesLayer = 0.91f;
    public const float OverlayBackLayer = 0.98f;
    public const float OverlayTopLayer = 0.99f;
    public const float HighlightEps = 0.000001f;
    public const float Eps = 0.00001f;

    public static readonly Color SnowBackgroundColor = new(208, 232, 242);
    public static readonly Color HighlightColor = new(80, 80, 80, 0);

    public static readonly BlendState AdditiveBlend = new BlendState
    {
        ColorSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.SourceAlpha,
        AlphaDestinationBlend = Blend.One
    };

    public static float CalculateDepth(float yPosition)
    {
        int worldHeight = GamelabGame.Instance.Services.GetService<GameplayContext>().WorldHeight;
        float normalizedY = Math.Clamp(yPosition / worldHeight, 0f, 1f);
        return 0.1f + (normalizedY * 0.8f);
    }

    public static void DrawWithLightBoost(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position,
        Rectangle? sourceRectangle, Color baseColor, Color idleLightColor, Color highlightLightColor, float rotation,
        Vector2 origin, float scale, SpriteEffects effects, float layerDepth, bool isHighlighted)
    {
        spriteBatch.Draw(texture, position, sourceRectangle, baseColor, rotation, origin, scale, effects, layerDepth);

        if (idleLightColor.A > 0)
        {
            spriteBatch.Draw(texture, position, sourceRectangle, idleLightColor, rotation, origin, scale, effects,
                layerDepth + HighlightEps);
        }

        if (isHighlighted && highlightLightColor.A > 0)
        {
            spriteBatch.Draw(texture, position, sourceRectangle, highlightLightColor, rotation, origin, scale,
                effects, layerDepth + 2 * HighlightEps);
        }
    }

    public static void DrawWithHighlight(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position,
        Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects,
        float layerDepth, bool isHighlighted)
    {
        spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);

        if (isHighlighted)
        {
            spriteBatch.Draw(texture, position, sourceRectangle, HighlightColor, rotation, origin, scale, effects,
                layerDepth + HighlightEps);
        }
    }

    public static void DrawWithHighlight(this SpriteBatch spriteBatch, AnimatedSprite sprite, Vector2 position,
        float rotation, Vector2 scale, bool isHighlighted)
    {
        spriteBatch.Draw(sprite, position, rotation, scale);

        if (isHighlighted)
        {
            Color originalColor = sprite.Color;
            float originalDepth = sprite.Depth;

            sprite.Color = HighlightColor;
            sprite.Depth = originalDepth + HighlightEps;

            spriteBatch.Draw(sprite, position, rotation, scale);

            sprite.Color = originalColor;
            sprite.Depth = originalDepth;
        }
    }
}