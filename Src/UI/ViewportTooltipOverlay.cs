using System;
using FontStashSharp;
using Gamelab.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.UI;

/// <summary>
/// Draws at most one tooltip per frame in virtual screen coordinates.
/// Typical use: <see cref="Clear"/> at the start of your screen Update, <see cref="OfferCloser"/> from gameplay,
/// then <see cref="Draw"/> inside a SpriteBatch begun with <c>viewportAdapter.GetScaleMatrix()</c>.
/// </summary>
public sealed class ViewportTooltipOverlay
{
    private string bestText = "";
    private Vector2 bestAnchorVirtual;
    private float bestMetric = float.MaxValue;
    private bool hasOffer;

    public void Clear()
    {
        hasOffer = false;
        bestText = "";
        bestMetric = float.MaxValue;
    }

    /// <summary>
    /// Keeps the candidate with the lowest <paramref name="sortMetric"/> (e.g. squared distance to the player).
    /// </summary>
    /// <param name="anchorVirtualBottomCenter">Virtual point above which the tooltip sits.</param>
    public void OfferCloser(float sortMetric, string text, Vector2 anchorVirtualBottomCenter)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!hasOffer || sortMetric < bestMetric)
        {
            hasOffer = true;
            bestMetric = sortMetric;
            bestText = text;
            bestAnchorVirtual = anchorVirtualBottomCenter;
        }
    }

    public void Draw(
        SpriteBatch spriteBatch,
        SpriteFontBase font,
        Point virtualSize,
        int marginPx = 16,
        int paddingPx = 10,
        int gapBelowTooltipPx = 8)
    {
        if (!hasOffer) return;

        string[] lines = bestText.Split('\n');
        float maxW = 0f;
        float lineStep = Math.Max(font.MeasureString("Mg").Y, 1f);
        foreach (string line in lines)
            maxW = Math.Max(maxW, font.MeasureString(line).X);

        float boxW = maxW + paddingPx * 2f;
        float boxH = lines.Length * lineStep + paddingPx * 2f;

        float left = bestAnchorVirtual.X - boxW / 2f;
        float top = bestAnchorVirtual.Y - gapBelowTooltipPx - boxH;

        left = Math.Clamp(left, marginPx, Math.Max(marginPx, virtualSize.X - marginPx - boxW));
        top = Math.Clamp(top, marginPx, Math.Max(marginPx, virtualSize.Y - marginPx - boxH));

        spriteBatch.Draw(AssetManager.BlankTexture,
            new Rectangle((int)left, (int)top, (int)Math.Ceiling(boxW), (int)Math.Ceiling(boxH)),
            new Color(0, 0, 0, 210));

        float textY = top + paddingPx;
        foreach (string line in lines)
        {
            spriteBatch.DrawString(font, line, new Vector2(left + paddingPx, textY), Color.White * 0.95f);
            textY += lineStep;
        }
    }
}
