using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;

namespace Gamelab.Systems;

public class DebugOverlaySystem : IGameSystem
{
    private GamelabGame game;
    private double smoothedFps;
    private Desktop desktop;
    private Label fpsLabel;
    private Label screenLabel;
    private Label hintLabel;

    public void Initialize(GamelabGame game)
    {
        this.game = game;

        fpsLabel = new Label();
        screenLabel = new Label();
        hintLabel = new Label();

        var panel = new VerticalStackPanel
        {
            Left = 14,
            Top = 12,
            Spacing = 4
        };

        fpsLabel.TextColor = Color.LimeGreen;
        screenLabel.TextColor = Color.LimeGreen;
        hintLabel.TextColor = Color.LightGray;

        panel.Widgets.Add(fpsLabel);
        panel.Widgets.Add(screenLabel);
        panel.Widgets.Add(hintLabel);

        desktop = new Desktop
        {
            Root = panel
        };
    }

    public void Update(GameTime gameTime)
    {
        double elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
        if (elapsedSeconds > 0)
        {
            double instantFps = 1.0 / elapsedSeconds;
            smoothedFps = smoothedFps <= 0
                ? instantFps
                : smoothedFps * 0.9 + instantFps * 0.1;
        }

        string activeScreenName = game.screenManager.ActiveScreen?.GetType().Name ?? "None";

        fpsLabel.Text = $"FPS: {smoothedFps:0.0}";
        screenLabel.Text = $"Screen: {activeScreenName}";
        hintLabel.Text = "F3: Toggle Overlay";
    }

    public void Draw()
    {
        if (!game.IsDebugOverlayEnabled)
        {
            return;
        }

        desktop.Render();
    }

    public void Shutdown()
    {
    }
}

