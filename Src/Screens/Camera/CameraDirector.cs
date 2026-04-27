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
    private float MinZoom => GamelabGame.Instance.GameplayConfig.CameraMinZoom;
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
        int worldWidth, int worldHeight, bool allowOffWorldOverflow = false)
    {
        if (players.Count == 0) return;

        Rectangle paddedZone = baseWindow;
        paddedZone.Inflate(Padding, Padding);

        bool allInside = players.All(p => paddedZone.Contains(p.Position));

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

        // Zoom out only when the player vertical span exceeds the default viewport height.
        float spanY = MathF.Max(maxY - minY, virtualScreenSize.Y);
        float fitZoom = virtualScreenSize.Y / spanY;
        float targetZoom = MathHelper.Clamp(fitZoom, ComputeMinAllowedZoom(worldWidth, worldHeight, allowOffWorldOverflow), 1f);

        // Camera horizontal center is locked to the world's horizontal center; players never pull X.
        float targetCenterX = worldWidth / 2f;
        float targetCenterY = (minY + maxY) / 2f;
        Vector2 targetCenter = new Vector2(targetCenterX, targetCenterY);

        // Lerp current zoom + center toward target.
        float lerpFactor = 1f - MathF.Exp(-LerpSpeed * dt);
        float newZoom = MathHelper.Lerp(camera.Zoom, targetZoom, lerpFactor);
        Vector2 currentCenter = ComputeWorldCenter(camera) - ShakeOffset;
        Vector2 newCenter = Vector2.Lerp(currentCenter, targetCenter, lerpFactor);

        UpdateScreenShake(dt);
        ApplyCamera(camera, newZoom, newCenter, worldWidth, worldHeight);
    }

    public void SnapToCenter(OrthographicCamera camera, Rectangle baseWindow, int worldWidth, int worldHeight,
        bool allowOffWorldOverflow = false)
    {
        Vector2 center = new Vector2(
            worldWidth / 2f,
            baseWindow.Y + baseWindow.Height / 2f);

        ShakeOffset = Vector2.Zero;
        screenShakeIntensity = 0;
        screenShakeTimer = 0;

        float initialZoom = MathHelper.Clamp(1f, ComputeMinAllowedZoom(worldWidth, worldHeight, allowOffWorldOverflow), 1f);
        ApplyCamera(camera, initialZoom, center, worldWidth, worldHeight);
    }

    private Vector2 ComputeWorldCenter(OrthographicCamera camera)
    {
        return camera.Position + camera.Origin;
    }

    private float ComputeMinAllowedZoom(int worldWidth, int worldHeight, bool allowOffWorldOverflow)
    {
        if (allowOffWorldOverflow) return MinZoom;

        float fitWorldWidthZoom = virtualScreenSize.X / (float)worldWidth;
        float fitWorldHeightZoom = virtualScreenSize.Y / (float)worldHeight;
        return Math.Max(MinZoom, MathF.Max(fitWorldWidthZoom, fitWorldHeightZoom));
    }

    private void ApplyCamera(OrthographicCamera camera, float zoom, Vector2 worldCenter, int worldWidth,
        int worldHeight)
    {
        Vector2 visibleSize = new Vector2(virtualScreenSize.X, virtualScreenSize.Y) / zoom;
        Vector2 topLeft = worldCenter - visibleSize / 2f;

        // If visible area fits inside the world, clamp inside [0, world - visible]. If it doesn't
        // fit (zoomed out far enough that visible > world), center the world inside the viewport.
        topLeft.X = visibleSize.X >= worldWidth
            ? (worldWidth - visibleSize.X) / 2f
            : MathHelper.Clamp(topLeft.X, 0f, worldWidth - visibleSize.X);
        topLeft.Y = visibleSize.Y >= worldHeight
            ? (worldHeight - visibleSize.Y) / 2f
            : MathHelper.Clamp(topLeft.Y, 0f, worldHeight - visibleSize.Y);

        Vector2 finalCenter = topLeft + visibleSize / 2f;
        camera.Zoom = zoom;
        camera.Position = finalCenter - camera.Origin + ShakeOffset;
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
}
