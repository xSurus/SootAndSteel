using System;
using System.Collections.Generic;
using Gamelab.Components.IngameHUD;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;

namespace Gamelab.UI;

public class GameplayHud : IDisposable
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly IngameHudOverlay overlay;
    private readonly DistanceTravelledDisplay distanceDisplay;
    private readonly Speedometer speedometer;
    private readonly List<EnemyDotMarker> enemyDots = [];
    private bool disposed;
    private LevelDefinition dotsLevelDef;

    // Speedometer calibration from Gum-authored marks:
    // 0 km/h -> 115.7099 deg, 600 km/h -> -119.0546 deg.
    private const float MinDisplaySpeed = 0f;
    private const float MaxDisplaySpeed = 600f;
    private const float AngleAtMinDisplaySpeed = 115.7099f;
    private const float AngleAtMaxDisplaySpeed = -119.0546f;
    private const float NeedleSmoothing = 0.15f;
    private const float MinimumTrackWidthPixels = 1f;
    private const float TrainMarkerPaddingPixels = 24f;
    private const float DefaultMovingDistanceBoxYOffset = 35f;
    private const float EnemyDotYOffset = 9f;
    private float smoothedSpeedRatio;
    private bool needlePivotConfigured;

    public GameplayHud()
    {
        overlay = new IngameHudOverlay();
        overlay.AddToRoot();
        distanceDisplay = overlay.DistanceTravelledDisplayInstance;
        speedometer = overlay.SpeedometerInstance;
        smoothedSpeedRatio = 0f;
        needlePivotConfigured = false;
    }

    public void Update(LevelDefinition currentLevelDef, float deltaTime)
    {
        _ = deltaTime;
        if (disposed || distanceDisplay == null || speedometer == null || gameplayContext?.State == null)
            return;

        var state = gameplayContext.State;
        var config = GamelabGame.Instance.GameplayConfig;
        EnsureNeedlePivotConfigured();
        float levelDistance = GetSanitizedLevelDistance(currentLevelDef);
        float distanceInLevel = Math.Max(0f, state.DistanceTraveled);
        float distanceRatio = Math.Clamp(distanceInLevel / levelDistance, 0f, 1f);
        EnsureEnemyDots(currentLevelDef, levelDistance);

        distanceDisplay.DistanceVSMaxText = $"{FormatMeters(distanceInLevel)} / {FormatMeters(levelDistance)}";
        distanceDisplay.FinalDistanceText = FormatMeters(levelDistance);
        UpdateTrainMarkerPosition(distanceRatio);
        UpdateEnemyDotPositions();

        float maxSpeed = Math.Max(config.TrainSpeedFast, 1f);
        float speedRatio = Math.Clamp(state.actualSpeed / maxSpeed, 0f, 1f);
        smoothedSpeedRatio = MathHelper.Lerp(smoothedSpeedRatio, speedRatio, NeedleSmoothing);
        float smoothedSpeed = smoothedSpeedRatio * maxSpeed;
        speedometer.NeedleContainer.Rotation = GetNeedleContainerRotation(smoothedSpeed);
    }

    private void UpdateTrainMarkerPosition(float distanceRatio)
    {
        if (!TryGetTrackTravelRange(out float travelRange))
            return;

        distanceDisplay.MovingTrainX = distanceRatio * travelRange;
    }

    private void EnsureEnemyDots(LevelDefinition currentLevelDef, float levelDistance)
    {
        if (currentLevelDef == null || distanceDisplay?.DistanceContainer == null)
        {
            ClearEnemyDots();
            dotsLevelDef = null;
            return;
        }

        if (ReferenceEquals(dotsLevelDef, currentLevelDef))
            return;

        ClearEnemyDots();
        dotsLevelDef = currentLevelDef;
        foreach (var spawn in currentLevelDef.SpawnEvents)
        {
            float ratio = Math.Clamp(spawn.Distance / Math.Max(levelDistance, 1f), 0f, 1f);
            var dot = new EnemyDot();
            distanceDisplay.DistanceContainer.Children.Add(dot.Visual);
            dot.Visual.XOrigin = HorizontalAlignment.Center;
            dot.Visual.YOrigin = VerticalAlignment.Center;
            enemyDots.Add(new EnemyDotMarker(dot, ratio));
        }
    }

    private void UpdateEnemyDotPositions()
    {
        if (enemyDots.Count == 0 || !TryGetTrackTravelRange(out float travelRange))
            return;

        float markerY = (distanceDisplay.MovingTrain?.Y ?? 0f)
            + (distanceDisplay.MovingDistanceBox?.Y ?? DefaultMovingDistanceBoxYOffset)
            + EnemyDotYOffset;
        foreach (var marker in enemyDots)
        {
            marker.Dot.Visual.X = marker.Ratio * travelRange;
            marker.Dot.Visual.Y = markerY;
            marker.Dot.Visual.Visible = true;
        }
    }

    private bool TryGetTrackTravelRange(out float travelRange)
    {
        travelRange = 0f;
        var distanceContainer = distanceDisplay?.DistanceContainer;
        if (distanceContainer == null)
            return false;

        float trackWidth = distanceContainer.GetAbsoluteWidth();
        if (trackWidth <= 0f)
            trackWidth = distanceContainer.Width;
        if (trackWidth <= MinimumTrackWidthPixels)
            return false;

        travelRange = Math.Max(0f, trackWidth - (TrainMarkerPaddingPixels * 2f));
        return true;
    }

    private void EnsureNeedlePivotConfigured()
    {
        if (needlePivotConfigured || speedometer?.Needle == null || speedometer?.NeedleContainer == null)
            return;

        var needle = speedometer.Needle;
        needle.Visible = true;

        // Keep the needle on top of the dial sprite.
        var parent = needle.Parent;
        if (parent != null)
        {
            parent.Children.Remove(needle);
            parent.Children.Add(needle);
        }

        needlePivotConfigured = true;
    }

    private static float GetNeedleContainerRotation(float speed)
    {
        float clampedSpeed = Math.Clamp(speed, MinDisplaySpeed, MaxDisplaySpeed);
        float t = (clampedSpeed - MinDisplaySpeed) / (MaxDisplaySpeed - MinDisplaySpeed);
        float angle = MathHelper.Lerp(AngleAtMinDisplaySpeed, AngleAtMaxDisplaySpeed, t);
        return NormalizeDegrees(angle);
    }

    private static float NormalizeDegrees(float degrees)
    {
        float normalized = degrees % 360f;
        if (normalized < 0f)
            normalized += 360f;
        return normalized;
    }

    private static float GetSanitizedLevelDistance(LevelDefinition levelDefinition)
        => Math.Max(levelDefinition?.LevelDistance ?? 1f, 1f);

    private static string FormatMeters(float value)
        => $"{value:F0} m";

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        ClearEnemyDots();
        if (overlay?.Visual != null && MonoGameGum.GumService.Default.Root.Children.Contains(overlay.Visual))
            MonoGameGum.GumService.Default.Root.Children.Remove(overlay.Visual);
    }

    private void ClearEnemyDots()
    {
        foreach (var marker in enemyDots)
        {
            if (marker.Dot?.Visual?.Parent != null)
                marker.Dot.Visual.Parent.Children.Remove(marker.Dot.Visual);
            else if (marker.Dot?.Visual != null && MonoGameGum.GumService.Default.Root.Children.Contains(marker.Dot.Visual))
                MonoGameGum.GumService.Default.Root.Children.Remove(marker.Dot.Visual);
        }
        enemyDots.Clear();
    }

    private readonly record struct EnemyDotMarker(EnemyDot Dot, float Ratio);
}