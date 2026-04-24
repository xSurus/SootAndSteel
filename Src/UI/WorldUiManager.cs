using System.Collections.Generic;
using System.Linq;
using Gamelab.PhysicalEntities.Interfaces;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.UI;

public class WorldUiManager(Desktop desktop, GamelabGame game)
{
    private readonly Dictionary<ITooltipable, VerticalStackPanel> activeTooltips = new();

    public void Update(IEnumerable<IPhysicalEntity> mapObjects, Matrix cameraMatrix)
    {
        var tooltipables = mapObjects.OfType<ITooltipable>().ToList();
        var itemsToRemove = activeTooltips.Keys.Where(k => !tooltipables.Contains(k)).ToList();
        foreach (var item in itemsToRemove)
        {
            RemoveTooltip(item);
        }

        foreach (var item in tooltipables)
        {
            if (!activeTooltips.ContainsKey(item))
            {
                CreateTooltip(item);
            }

            if (item.IsTooltipVisible) UpdateTooltipPosition(item, cameraMatrix);
            activeTooltips[item].Visible = item.IsTooltipVisible;
        }
    }

    private void CreateTooltip(ITooltipable item)
    {
        var panel = new VerticalStackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Background = new SolidBrush(new Color(0, 0, 0, 150)),
            Padding = new Thickness(5)
        };

        var titleLabel = new Label
        {
            Id = "Title",
            Text = item.GetTooltipTitle(),
            Font = game.fontSystem.GetFont(24),
            TextColor = Color.White
        };

        var descLabel = new Label
        {
            Id = "Desc",
            Text = item.GetTooltipDescription(),
            Font = game.fontSystem.GetFont(24),
            TextColor = item.GetTooltipTextColor()
        };

        panel.Widgets.Add(titleLabel);
        panel.Widgets.Add(descLabel);

        desktop.Widgets.Add(panel);
        activeTooltips[item] = panel;
    }

    private void UpdateTooltipPosition(ITooltipable item, Matrix cameraMatrix)
    {
        var panel = activeTooltips[item];
        var descLabel = (Label)panel.FindChildById("Desc");
        if (descLabel != null)
        {
            descLabel.Text = item.GetTooltipDescription();
            descLabel.TextColor = item.GetTooltipTextColor();
        }

        // myra needs to calculate the size of new UI panels after creation, so the first frame is not always where it should be
        // we spawn it offscreen so this is not visble
        if (panel.Bounds.Width == 0 || panel.Bounds.Height == 0)
        {
            panel.Left = -10000;
            panel.Top = -10000;
            return;
        }

        Vector2 screenPos = Vector2.Transform(item.Position, cameraMatrix);

        int tileSize = game.GameplayConfig.TrainTileSize;
        float liftAmount = 25f;

        panel.Left = (int)(screenPos.X - (panel.Bounds.Width / 2f));
        panel.Top = (int)(screenPos.Y - (tileSize / 2f) - panel.Bounds.Height - liftAmount);
    }

    private void RemoveTooltip(ITooltipable item)
    {
        if (activeTooltips.TryGetValue(item, out var panel))
        {
            desktop.Widgets.Remove(panel);
            activeTooltips.Remove(item);
        }
    }

    public void SetAllVisible(bool isVisible)
    {
        foreach (var panel in activeTooltips.Values)
        {
            panel.Visible = isVisible;
        }
    }

    public void ClearAll()
    {
        foreach (var panel in activeTooltips.Values)
        {
            desktop.Widgets.Remove(panel);
        }

        activeTooltips.Clear();
    }
}