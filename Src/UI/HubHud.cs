using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace Gamelab.UI;

public class HubHud
{
    public Desktop Desktop { get; }

    private readonly Label creditsLabel;
    private readonly Label departBlockedLabel;

    public HubHud(GamelabGame game)
    {
        creditsLabel = new Label
        {
            Text = $"Credits: {game.CurrentRun.Credits}",
            Font = game.fontSystem.GetFont(40),
            TextColor = new Color(230, 200, 120),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 14, 28, 0)
        };

        departBlockedLabel = new Label
        {
            Text = "",
            Font = game.fontSystem.GetFont(28),
            TextColor = new Color(255, 120, 120),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(80, 0, 80, 48),
            Visible = false
        };

        var rootPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        rootPanel.Widgets.Add(creditsLabel);
        rootPanel.Widgets.Add(departBlockedLabel);

        Desktop = new Desktop { Root = rootPanel };
    }

    public void Update(bool allInDepart, int pendingShopCount, float departHoldTimer, float departHoldSeconds, int readyCount = 0, int totalCount = 0)
    {
        int currentCredits = GamelabGame.Instance.CurrentRun.Credits;
        creditsLabel.Text = $"Credits: {currentCredits}";

        (string text, Color color) = GetDepartureInformation(
            allInDepart, pendingShopCount, departHoldTimer, departHoldSeconds, readyCount, totalCount);

        departBlockedLabel.Visible = true;
        departBlockedLabel.Text = text;
        departBlockedLabel.TextColor = color;
    }

    private static (string Text, Color Color) GetDepartureInformation(
        bool allInDepart, int pendingShopCount, float departHoldTimer, float departHoldSeconds, int readyCount, int totalCount)
    {
        if (allInDepart && pendingShopCount > 0)
        {
            return (
                "Purchase placed shop upgrades (Interact) or drag them back to the vendor before departing.",
                new Color(255, 120, 120));
        }

        if (allInDepart)
        {
            string text = departHoldSeconds <= 0f || departHoldTimer >= departHoldSeconds
                ? "Departing…"
                : $"All ready! Departing in {departHoldSeconds - departHoldTimer:0.0}s…";
            return (text, new Color(180, 230, 200));
        }

        if (pendingShopCount > 0)
        {
            return (
                "Interact next to a colored tile on the train to buy it, or grab it and return it to the shop row to cancel.",
                new Color(200, 200, 120));
        }

        return (
            $"Depart: all players interact with the speed lever ({readyCount}/{totalCount} ready)",
            new Color(160, 200, 170));
    }

    public void Draw()
    {
        Desktop.Render();
    }
}