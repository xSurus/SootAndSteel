using System;
using Gamelab.Map.Train.State;

namespace Gamelab.Utils;

public static class RenderUtility
{
    public const float BackgroundLayer = 0.01f;
    public const float FloorLayer = 0.05f;

    public const float ParticlesLayer = 0.91f;
    public const float TopEntityLayer = 0.92f;
    public const float OverlayBackLayer = 0.98f;
    public const float OverlayTopLayer = 0.99f;
    public const float Eps = 0.00001f;

    public static float CalculateDepth(float yPosition)
    {
        int worldHeight = GamelabGame.Instance.Services.GetService<GameplayContext>().WorldHeight;
        float normalizedY = Math.Clamp(yPosition / worldHeight, 0f, 1f);
        return 0.1f + (normalizedY * 0.8f);
    }
}