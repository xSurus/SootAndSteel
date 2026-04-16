using System.Collections.Generic;
using System.Linq;
using Gamelab.PhysicalEntities;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

namespace Gamelab.UI;

public class WorldUiManager(Desktop desktop, GamelabGame game)
{
    private readonly Dictionary<ITooltipable, VerticalStackPanel> activeTooltips = new();

    public void Update(IEnumerable<IPhysicalEntity> mapObjects, Matrix cameraMatrix)
    {
        var tooltipables = mapObjects.OfType<ITooltipable>().ToList();
        var itemsToRemove = activeTooltips.Keys.Where(k => !k.IsActive || !tooltipables.Contains(k)).ToList();
        foreach (var item in itemsToRemove)
        {
            RemoveTooltip(item);
        }

        foreach (var item in tooltipables)
        {
            if (!item.IsActive) continue;
            if (!activeTooltips.ContainsKey(item))
            {
                CreateTooltip(item);
            }

            UpdateTooltipPosition(item, cameraMatrix);
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
            Padding = new Myra.Graphics2D.Thickness(5)
        };

        var titleLabel = new Label
        {
            Id = "Title",
            Text = item.GetTitle(),
            Font = game.fontSystem.GetFont(24),
            TextColor = Color.White
        };

        var descLabel = new Label
        {
            Id = "Desc",
            Text = item.GetDescription(),
            Font = game.fontSystem.GetFont(24),
            TextColor = item.GetTextColor()
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
            descLabel.Text = item.GetDescription();
            descLabel.TextColor = item.GetTextColor();
        }

        Vector2 screenPos = Vector2.Transform(item.WorldPosition, cameraMatrix);

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

    public void ClearAll()
    {
        foreach (var panel in activeTooltips.Values)
        {
            desktop.Widgets.Remove(panel);
        }
        activeTooltips.Clear();
    }
}