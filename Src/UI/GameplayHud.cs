using System;
using FontStashSharp;
using Gamelab.Assets;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.UI;

public class GameplayHud
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly SpriteFontBase labelFont;
    private readonly SpriteFontBase smallFont;
    private readonly SpriteFontBase gaugeFont;

    private float distanceRatio;
    private float temperatureRatio;
    private float speedRatio;
    private string distanceText = "";
    private string coalText = "";
    private string speedText = "";
    private string tempText = "";

    private LevelDefinition currentLevelDef;
    private float distanceInLevel;

    private const int SpawnMarkerSize = 6;
    private static readonly Color SpawnMarkerUpcoming = new(230, 60, 60);
    private static readonly Color SpawnMarkerPassed = new(120, 40, 40, 180);

    private const int ArcSegments = 40;
    private const float ArcRadius = 58f;
    private const float ArcThickness = 8f;
    private const float TickLength = 6f;
    private const float ArcStartAngle = MathF.PI;
    private const float ArcSweep = MathF.PI;
    private const float GaugeDiameter = (ArcRadius + TickLength + 10f) * 2f;
    private const float GaugeSpacing = 16f;

    public GameplayHud()
    {
        var fs = GamelabGame.Instance.fontSystem;
        labelFont = fs.GetFont(26);
        smallFont = fs.GetFont(22);
        gaugeFont = fs.GetFont(36);
    }

    public void Update(LevelDefinition currentLevelDef)
    {
        var state = gameplayContext.State;
        var config = GamelabGame.Instance.GameplayConfig;

        this.currentLevelDef = currentLevelDef;
        distanceInLevel = Math.Max(0f, state.DistanceTraveled);
        float levelDistance = currentLevelDef?.LevelDistance ?? 1f;
        distanceRatio = Math.Clamp(distanceInLevel / Math.Max(levelDistance, 1f), 0f, 1f);
        distanceText = $"{distanceInLevel:F0} / {levelDistance:F0} m";

        float maxTemp = config.TrainMaxTemperature;
        temperatureRatio = Math.Clamp(state.Temperature / Math.Max(maxTemp, 1f), 0f, 1f);
        tempText = $"{state.Temperature:F0}\u00b0";

        float maxSpeed = config.TrainSpeedQuadruple;
        speedRatio = Math.Clamp(state.actualSpeed / Math.Max(maxSpeed, 1f), 0f, 1f);
        speedText = $"{state.actualSpeed:F0}";

        coalText = $"Coal: {state.CoalAmount}";
    }

    public void Draw(SpriteBatch sb, Point screen)
    {
        var blank = AssetManager.BlankTexture;

        DrawDistanceBar(sb, blank, screen.X);

        float leftGaugeCenterX = 24f + ArcRadius + 10f;
        float gaugeCenterY = screen.Y - 24f - ArcRadius - 40f;

        DrawArcGauge(sb, blank, new Vector2(leftGaugeCenterX, gaugeCenterY),
            temperatureRatio, tempText, GetTemperatureColor);

        float rightGaugeCenterX = leftGaugeCenterX + GaugeDiameter + GaugeSpacing;
        DrawArcGauge(sb, blank, new Vector2(rightGaugeCenterX, gaugeCenterY),
            speedRatio, speedText, GetSpeedColor, "km/h");

        float midX = (leftGaugeCenterX + rightGaugeCenterX) / 2f;
        float coalY = gaugeCenterY + ArcRadius + 20f;
        Vector2 coalSize = labelFont.MeasureString(coalText);
        float coalX = midX - coalSize.X / 2f;
        sb.DrawString(labelFont, coalText, new Vector2(coalX + 1, coalY + 1), Color.Black * 0.5f);
        sb.DrawString(labelFont, coalText, new Vector2(coalX, coalY), new Color(255, 200, 80));
    }

    private void DrawDistanceBar(SpriteBatch sb, Texture2D blank, int screenWidth)
    {
        const int barHeight = 10;
        const int barMargin = 24;
        int barWidth = screenWidth - barMargin * 2;
        int barX = barMargin;
        int barY = 16;

        sb.Draw(blank, new Rectangle(barX, barY, barWidth, barHeight), new Color(0, 0, 0, 140));

        int fillWidth = (int)(barWidth * distanceRatio);
        Color fillColor = Color.Lerp(new Color(140, 180, 220), new Color(220, 240, 255), distanceRatio);
        if (fillWidth > 0)
            sb.Draw(blank, new Rectangle(barX, barY, fillWidth, barHeight), fillColor);

        sb.Draw(blank, new Rectangle(barX, barY, barWidth, 1), Color.White * 0.3f);
        sb.Draw(blank, new Rectangle(barX, barY + barHeight - 1, barWidth, 1), Color.White * 0.3f);

        DrawSpawnMarkers(sb, blank, barX, barY, barWidth, barHeight);

        Vector2 textSize = smallFont.MeasureString(distanceText);
        float textX = barX + (barWidth - textSize.X) / 2f;
        float textY = barY + barHeight + 4;
        sb.DrawString(smallFont, distanceText, new Vector2(textX + 1, textY + 1), Color.Black * 0.5f);
        sb.DrawString(smallFont, distanceText, new Vector2(textX, textY), Color.White * 0.9f);
    }

    private void DrawSpawnMarkers(SpriteBatch sb, Texture2D blank, int barX, int barY, int barWidth, int barHeight)
    {
        if (currentLevelDef == null || currentLevelDef.SpawnEvents.Count == 0) return;

        float levelDistance = Math.Max(currentLevelDef.LevelDistance, 1f);
        int markerY = barY + (barHeight - SpawnMarkerSize) / 2;
        int half = SpawnMarkerSize / 2;

        foreach (SpawnEvent spawn in currentLevelDef.SpawnEvents)
        {
            float ratio = Math.Clamp(spawn.Distance / levelDistance, 0f, 1f);
            int centerX = barX + (int)(ratio * barWidth);
            Color color = spawn.Distance <= distanceInLevel ? SpawnMarkerPassed : SpawnMarkerUpcoming;

            sb.Draw(blank,
                new Rectangle(centerX - half - 1, markerY - 1, SpawnMarkerSize + 2, SpawnMarkerSize + 2),
                Color.Black * 0.6f);
            sb.Draw(blank,
                new Rectangle(centerX - half, markerY, SpawnMarkerSize, SpawnMarkerSize),
                color);
        }
    }

    private void DrawArcGauge(SpriteBatch sb, Texture2D blank, Vector2 center,
        float ratio, string valueText, Func<float, Color> colorFunc, string unitText = null)
    {
        var origin = new Vector2(0f, 0.5f);
        int filledSegments = (int)(ArcSegments * ratio);

        for (int i = 0; i < ArcSegments; i++)
        {
            float t = (float)i / ArcSegments;
            float angle = ArcStartAngle + ArcSweep * t;
            float segRatio = (float)i / ArcSegments;

            Color segColor = i < filledSegments
                ? colorFunc(segRatio)
                : new Color(30, 30, 35, 200);

            var pos = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ArcRadius;
            sb.Draw(blank, pos, null, segColor, angle,
                origin, new Vector2(ArcThickness, 3f), SpriteEffects.None, 0f);
        }

        const int tickCount = 10;
        for (int i = 0; i <= tickCount; i++)
        {
            float t = (float)i / tickCount;
            float angle = ArcStartAngle + ArcSweep * t;
            float outerR = ArcRadius + 6f;
            bool isMajor = i % 5 == 0;
            float len = isMajor ? TickLength + 3f : TickLength;
            Color tickColor = isMajor ? Color.White * 0.6f : Color.White * 0.3f;
            float thickness = isMajor ? 2f : 1.5f;

            var tickStart = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * outerR;
            sb.Draw(blank, tickStart, null, tickColor, angle,
                origin, new Vector2(len, thickness), SpriteEffects.None, 0f);
        }

        Color readoutColor = colorFunc(ratio);
        Vector2 valSize = gaugeFont.MeasureString(valueText);
        float valX = center.X - valSize.X / 2f;
        float valY = center.Y + 6;
        sb.DrawString(gaugeFont, valueText, new Vector2(valX + 1, valY + 1), Color.Black * 0.6f);
        sb.DrawString(gaugeFont, valueText, new Vector2(valX, valY), readoutColor);

        if (unitText != null)
        {
            Vector2 unitSize = smallFont.MeasureString(unitText);
            float unitX = center.X - unitSize.X / 2f;
            float unitY = valY + valSize.Y - 2;
            sb.DrawString(smallFont, unitText, new Vector2(unitX + 1, unitY + 1), Color.Black * 0.4f);
            sb.DrawString(smallFont, unitText, new Vector2(unitX, unitY), Color.White * 0.5f);
        }
    }

    private static Color GetTemperatureColor(float ratio)
    {
        if (ratio > 0.6f)
        {
            float t = (ratio - 0.6f) / 0.4f;
            return Color.Lerp(new Color(255, 200, 60), new Color(255, 100, 40), t);
        }

        if (ratio > 0.3f)
        {
            float t = (ratio - 0.3f) / 0.3f;
            return Color.Lerp(new Color(100, 200, 255), new Color(255, 200, 60), t);
        }

        float tCold = ratio / 0.3f;
        return Color.Lerp(new Color(60, 140, 255), new Color(100, 200, 255), tCold);
    }

    private static Color GetSpeedColor(float ratio)
    {
        if (ratio > 0.75f)
        {
            float t = (ratio - 0.75f) / 0.25f;
            return Color.Lerp(new Color(255, 160, 60), new Color(255, 70, 40), t);
        }

        if (ratio > 0.5f)
        {
            float t = (ratio - 0.5f) / 0.25f;
            return Color.Lerp(new Color(200, 200, 210), new Color(255, 160, 60), t);
        }

        return new Color(180, 185, 195);
    }
}
