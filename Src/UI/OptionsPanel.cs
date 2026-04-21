using System;
using System.Collections.Generic;
using FontStashSharp;
using Gamelab.Assets;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.UI.ViewModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.UI;
public class OptionsPanel
{
    private readonly ISoundService soundService;
    private readonly OptionsViewModel viewModel;

    public bool IsOpen => viewModel.IsOpen;
    public OptionsViewModel ViewModel => viewModel;

    public event Action OnClosed
    {
        add => viewModel.OnClosed += value;
        remove => viewModel.OnClosed -= value;
    }

    public OptionsPanel(GamelabGame game)
        : this(game.Services.GetService<ISoundService>(), game.GameplayConfig.MenuVolumeStep)
    {
    }

    public OptionsPanel(ISoundService soundService, float volumeStep)
    {
        this.soundService = soundService;
        viewModel = new OptionsViewModel(soundService) { VolumeStep = volumeStep };
    }

    public void Open() => viewModel.Open();
    public void Close() => viewModel.Close();

    public void Update(IReadOnlyList<PlayerConfiguration> players)
    {
        if (!viewModel.IsOpen) return;

        foreach (PlayerConfiguration player in players)
        {
            var input = player.Input;

            if (input.IsPauseJustPressed())
            {
                viewModel.Close();
                return;
            }

            if (input.IsUpJustPressed())
            {
                viewModel.MoveSelectionUp();
                soundService.PlayOnce(Sounds.MenuSelect);
            }
            else if (input.IsDownJustPressed())
            {
                viewModel.MoveSelectionDown();
                soundService.PlayOnce(Sounds.MenuSelect);
            }

            OptionsViewModel.Row row = viewModel.SelectedRow;

            if (!OptionsViewModel.IsAdjustable(row))
            {
                if (input.IsPickupJustPressed())
                {
                    soundService.PlayOnce(Sounds.MenuSelect);
                    viewModel.ConfirmSelection();
                    return;
                }
                continue;
            }

            if (input.IsLeftJustPressed()) viewModel.DecreaseSelected();
            else if (input.IsRightJustPressed()) viewModel.IncreaseSelected();
        }
    }

    public void Draw(SpriteBatch spriteBatch, Point virtualScreenSize, SpriteFontBase titleFont,
        SpriteFontBase itemFont)
    {
        if (!viewModel.IsOpen) return;

        spriteBatch.Draw(AssetManager.BlankTexture,
            new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
            Color.Black * 0.75f);

        const int panelWidth = 820;
        const int panelHeight = 520;
        var panelRect = new Rectangle(
            (virtualScreenSize.X - panelWidth) / 2,
            (virtualScreenSize.Y - panelHeight) / 2,
            panelWidth,
            panelHeight);

        spriteBatch.Draw(AssetManager.BlankTexture, panelRect, new Color(25, 25, 30) * 0.95f);

        const string title = "OPTIONS";
        Vector2 titleSize = titleFont.MeasureString(title);
        Vector2 titlePos = new(panelRect.Center.X - titleSize.X / 2f, panelRect.Y + 30);
        spriteBatch.DrawString(titleFont, title, titlePos, Color.White);

        float rowFontHeight = itemFont.MeasureString("Ag").Y;
        const string widestValue = "< 100% >";
        float valueColumnWidth = itemFont.MeasureString(widestValue).X;
        const int sidePadding = 40;

        int labelX = panelRect.X + sidePadding;
        int valueColumnRight = panelRect.Right - sidePadding;
        int valueColumnLeft = (int)(valueColumnRight - valueColumnWidth);

        const int barToValueGap = 28;
        const int labelToBarGap = 32;
        int barLeft = (int)(panelRect.X + sidePadding + LongestAdjustableLabelWidth(itemFont) + labelToBarGap);
        int barRight = valueColumnLeft - barToValueGap;
        int barWidth = Math.Max(60, barRight - barLeft);
        const int barHeight = 18;

        int firstRowY = panelRect.Y + 150;
        int rowSpacing = 80;

        IReadOnlyList<OptionsViewModel.Row> rows = viewModel.Rows;
        for (int i = 0; i < rows.Count; i++)
        {
            OptionsViewModel.Row row = rows[i];
            int y = firstRowY + i * rowSpacing;
            bool selected = i == viewModel.SelectionIndex;
            Color color = selected ? Color.LightBlue : Color.White * 0.92f;

            if (!OptionsViewModel.IsAdjustable(row))
            {
                string label = selected
                    ? $"< {OptionsViewModel.LabelOf(row).ToUpperInvariant()} >"
                    : OptionsViewModel.LabelOf(row).ToUpperInvariant();
                Vector2 size = itemFont.MeasureString(label);
                spriteBatch.DrawString(itemFont, label,
                    new Vector2(panelRect.Center.X - size.X / 2f, y), color);
                continue;
            }

            string rowLabel = OptionsViewModel.LabelOf(row);
            float value = viewModel.VolumeOf(row);
            string valueText = $"< {(int)Math.Round(value * 100f)}% >";

            spriteBatch.DrawString(itemFont, rowLabel, new Vector2(labelX, y), color);

            Vector2 valueSize = itemFont.MeasureString(valueText);
            spriteBatch.DrawString(itemFont, valueText,
                new Vector2(valueColumnRight - valueSize.X, y), color);

            int barY = (int)(y + (rowFontHeight - barHeight) / 2f);
            DrawVolumeBar(spriteBatch,
                new Rectangle(barLeft, barY, barWidth, barHeight),
                value,
                selected);
        }
    }

    private float LongestAdjustableLabelWidth(SpriteFontBase font)
    {
        float max = 0f;
        foreach (OptionsViewModel.Row row in viewModel.Rows)
        {
            if (!OptionsViewModel.IsAdjustable(row)) continue;
            max = Math.Max(max, font.MeasureString(OptionsViewModel.LabelOf(row)).X);
        }
        return max;
    }

    private static void DrawVolumeBar(SpriteBatch spriteBatch, Rectangle rect, float value, bool highlighted)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;

        var borderColor = (highlighted ? Color.LightBlue : Color.White) * 0.6f;
        DrawRectOutline(spriteBatch, rect, borderColor, 2);

        var inner = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
        spriteBatch.Draw(AssetManager.BlankTexture, inner, Color.Black * 0.55f);

        int filled = (int)Math.Round(inner.Width * Math.Clamp(value, 0f, 1f));
        if (filled > 0)
        {
            Color fillColor = highlighted ? Color.LightBlue : Color.White * 0.8f;
            spriteBatch.Draw(AssetManager.BlankTexture,
                new Rectangle(inner.X, inner.Y, filled, inner.Height), fillColor);
        }
    }

    private static void DrawRectOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        spriteBatch.Draw(AssetManager.BlankTexture,
            new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        spriteBatch.Draw(AssetManager.BlankTexture,
            new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}
