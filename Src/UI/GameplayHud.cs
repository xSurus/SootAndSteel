using System;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Gamelab.UI;

public class GameplayHud
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    private readonly Label coalLabel;
    private readonly Label speedLabel;
    private readonly Label temperatureLabel;
    private readonly Label distanceLabel;
    private readonly Panel root;

    public Panel Root => root;

    public GameplayHud()
    {
        var font = GamelabGame.Instance.fontSystem;

        coalLabel = CreateLabel(font, "Coal: 0", 20, 20);
        speedLabel = CreateLabel(font, "Speed: 0", 20, 80);
        temperatureLabel = CreateLabel(font, "Temperature: 100", 20, 140);
        distanceLabel = CreateLabel(font, "Distance: 0", 20, 200);

        root = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        root.Widgets.Add(coalLabel);
        root.Widgets.Add(speedLabel);
        root.Widgets.Add(temperatureLabel);
        root.Widgets.Add(distanceLabel);
    }

    public void Update(LevelDefinition currentLevelDef, float levelStartDistance)
    {
        var state = gameplayContext.State;
        coalLabel.Text = $"Coal: {state.CoalAmount}";
        speedLabel.Text = $"Speed: {state.actualSpeed:F0}";
        temperatureLabel.Text = $"Temperature: {state.Temperature:F0}";

        float distanceInLevel = Math.Max(0f, state.DistanceTraveled - levelStartDistance);
        distanceLabel.Text = $"Distance: {distanceInLevel:F0} / {currentLevelDef?.LevelDistance ?? 0:F0}";
    }

    private static Label CreateLabel(FontStashSharp.FontSystem font, string text, int marginLeft, int marginTop)
    {
        return new Label
        {
            Text = text,
            Font = font.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(marginLeft, marginTop, 20, 20)
        };
    }
}
