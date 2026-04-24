using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Gamelab.Screens.Camera;

public class CameraDirector(Point virtualScreenSize)
{
    private float LerpSpeed => GamelabGame.Instance.GameplayConfig.CameraLerpFactor;
    private float Padding => GamelabGame.Instance.GameplayConfig.TrainTileSize;

    private float screenShakeTimer;
    private float screenShakeIntensity;
    public Vector2 ShakeOffset { get; private set; }
    private readonly Random random = Random.Shared;

    public void TriggerShake(float intensity, float duration)
    {
        screenShakeIntensity = intensity;
        screenShakeTimer = duration;
    }

    public void Update(OrthographicCamera camera, float dt, List<Player> players, Rectangle baseWindow,
        float worldHeight)
    {
        if (players.Count == 0) return;

        Rectangle paddedZone = baseWindow;
        paddedZone.Inflate(Padding, Padding);

        bool allInside = players.Count != 0 && players.All(p => paddedZone.Contains(p.Position));

        float minY, maxY;

        if (allInside)
        {
            minY = baseWindow.Top;
            maxY = baseWindow.Bottom;
        }
        else
        {
            var first = players.First();
            minY = first.Position.Y - Padding;
            maxY = first.Position.Y + Padding;

            foreach (var player in players.Skip(1))
            {
                minY = Math.Min(minY, player.Position.Y - Padding);
                maxY = Math.Max(maxY, player.Position.Y + Padding);
            }
        }

        float targetX = baseWindow.X + baseWindow.Width / 2f;
        float targetY = minY + (maxY - minY) / 2f;
        Vector2 targetPosition = new Vector2(targetX, targetY);

        float lerpFactor = 1f - MathF.Exp(-LerpSpeed * dt);
        Vector2 trueCurrentCenter = camera.Position + camera.Origin - ShakeOffset;
        Vector2 trueNewCenter = Vector2.Lerp(trueCurrentCenter, targetPosition, lerpFactor);

        float proposedTopY = trueNewCenter.Y - camera.Origin.Y;
        float maxTopY = Math.Max(0, worldHeight - virtualScreenSize.Y);
        float clampedTopY = MathHelper.Clamp(proposedTopY, 0, maxTopY);

        UpdateScreenShake(dt);
        camera.Position = new Vector2(trueNewCenter.X - camera.Origin.X, clampedTopY) + ShakeOffset;
        camera.Zoom = 1f;
    }

    private void UpdateScreenShake(float dt)
    {
        if (screenShakeTimer <= 0)
        {
            ShakeOffset = Vector2.Zero;
            screenShakeIntensity = 0;
            return;
        }

        screenShakeTimer -= dt;
        ShakeOffset = new Vector2(
            (float)(random.NextDouble() * 2 - 1) * screenShakeIntensity,
            (float)(random.NextDouble() * 2 - 1) * screenShakeIntensity
        );

        screenShakeIntensity *= 1f - GamelabGame.Instance.GameplayConfig.ScreenShakeDecay * dt;
    }

    public void SnapToCenter(OrthographicCamera camera, Rectangle baseWindow, float worldHeight)
    {
        float targetX = baseWindow.X + baseWindow.Width / 2f;
        float targetY = baseWindow.Y + baseWindow.Height / 2f;
        float proposedTopY = targetY - camera.Origin.Y;
        float maxTopY = Math.Max(0, worldHeight - virtualScreenSize.Y);
        float clampedTopY = MathHelper.Clamp(proposedTopY, 0, maxTopY);
        camera.Position = new Vector2(targetX - camera.Origin.X, clampedTopY);
        camera.Zoom = 1f;
        ShakeOffset = Vector2.Zero;
        screenShakeIntensity = 0;
        screenShakeTimer = 0;
    }
}