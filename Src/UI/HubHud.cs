using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace Gamelab.UI;

public class HubHud
{
    private readonly GamelabGame game;
    public Desktop Desktop { get; }

    private readonly Label creditsLabel;
    private readonly Label departBlockedLabel;

    public HubHud(GamelabGame game)
    {
        this.game = game;

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

        if (game.CurrentRun.Credits == 0 && game.CurrentRun.CurrentLevel == 0)
        {
            rootPanel.Widgets.Add(BuildControlsHelpPanel());
        }

        Desktop = new Desktop { Root = rootPanel };
    }

    public void Update(bool allInDepart, int pendingShopCount, float departHoldTimer, float departHoldSeconds)
    {
        int currentCredits = GamelabGame.Instance.CurrentRun.Credits;
        creditsLabel.Text = $"Credits: {currentCredits}";

        departBlockedLabel.Visible = true;
        if (allInDepart && pendingShopCount > 0)
        {
            departBlockedLabel.Text =
                "Purchase placed shop upgrades (Interact) or drag them back to the vendor before departing.";
            departBlockedLabel.TextColor = new Color(255, 120, 120);
        }
        else if (allInDepart)
        {
            if (departHoldSeconds <= 0f || departHoldTimer >= departHoldSeconds)
            {
                departBlockedLabel.Text = "Departing…";
            }
            else
            {
                departBlockedLabel.Text = $"Stay in zone to depart ({departHoldSeconds - departHoldTimer:0.0}s)…";
            }

            departBlockedLabel.TextColor = new Color(180, 230, 200);
        }
        else if (pendingShopCount > 0)
        {
            departBlockedLabel.Text =
                "Interact next to a colored tile on the train to buy it, or grab it and return it to the shop row to cancel.";
            departBlockedLabel.TextColor = new Color(200, 200, 120);
        }
        else
        {
            departBlockedLabel.Text = "Depart: move all players into the green-tinted zone at the bottom.";
            departBlockedLabel.TextColor = new Color(160, 200, 170);
        }
    }

    public void Draw()
    {
        Desktop.Render();
    }

    private Widget BuildControlsHelpPanel()
    {
        var font = game.fontSystem.GetFont(18);
        var headerFont = game.fontSystem.GetFont(22);
        var dimWhite = new Color(210, 210, 220);

        var stack = new VerticalStackPanel
        {
            Spacing = 3,
            Padding = new Thickness(12, 8, 12, 8),
            Background = new SolidBrush(new Color(10, 10, 15, 180)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14, 14, 0, 0)
        };

        stack.Widgets.Add(new Label
        {
            Text = "Controls",
            Font = headerFont,
            TextColor = new Color(255, 220, 100)
        });

        (string button, string action)[] entries =
        [
            ("Left Stick / D-Pad", "Move"),
            ("X", "Interact / Repair"),
            ("Y", "Grab items"),
            ("A", "Pick up / Confirm"),
            ("Start", "Pause")
        ];

        foreach ((string button, string action) in entries)
        {
            stack.Widgets.Add(new Label
            {
                Text = $"  {button}  —  {action}",
                Font = font,
                TextColor = dimWhite
            });
        }

        return stack;
    }
}