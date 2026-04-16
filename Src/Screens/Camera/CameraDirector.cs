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

    public void Update(OrthographicCamera camera, float dt, List<Player> players, Rectangle baseWindow)
    {
        if (players.Count == 0) return;

        Rectangle paddedZone = baseWindow;
        paddedZone.Inflate(Padding, Padding);

        bool allInside = players.Count != 0 && players.All(p => paddedZone.Contains(p.Position));

        float minX, maxX, minY, maxY;

        if (allInside)
        {
            minX = baseWindow.Left;
            maxX = baseWindow.Right;
            minY = baseWindow.Top;
            maxY = baseWindow.Bottom;
        }
        else
        {
            var first = players.First();
            minX = first.Position.X - Padding;
            maxX = first.Position.X + Padding;
            minY = first.Position.Y - Padding;
            maxY = first.Position.Y + Padding;

            foreach (var player in players.Skip(1))
            {
                minX = Math.Min(minX, player.Position.X - Padding);
                maxX = Math.Max(maxX, player.Position.X + Padding);
                minY = Math.Min(minY, player.Position.Y - Padding);
                maxY = Math.Max(maxY, player.Position.Y + Padding);
            }
        }

        float width = maxX - minX;
        float height = maxY - minY;
        Vector2 targetPosition = new Vector2(minX + width / 2f, minY + height / 2f);

        float targetZoom = 1f;
        if (width > 0 && height > 0)
        {
            float zoomX = virtualScreenSize.X / width;
            float zoomY = virtualScreenSize.Y / height;
            targetZoom = MathHelper.Clamp(Math.Min(zoomX, zoomY), 0.1f, 1f);
        }

        float lerpFactor = 1f - MathF.Exp(-LerpSpeed * dt);
        Vector2 currentCenter = camera.Position + camera.Origin;
        Vector2 newCenter = Vector2.Lerp(currentCenter, targetPosition, lerpFactor);
        camera.Position = newCenter - camera.Origin;
        camera.Zoom = MathHelper.Lerp(camera.Zoom, targetZoom, lerpFactor);
    }
}