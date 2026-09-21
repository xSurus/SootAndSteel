using System;
using System.Collections.Generic;
using System.Numerics;

namespace Gamelab.UI
{
    public sealed class TooltipRect
    {
        public float X;
        public float Y;
        public float W;
        public float H;

        public TooltipRect(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
    }

    /// <summary>Placement maths of Src WorldUiManager, in 1920x1080 canvas units.</summary>
    public static class TooltipLayout
    {
        public const float CanvasWidth = 1920f;
        public const float CanvasHeight = 1080f;
        public const float PanelWidth = 370f;
        public const float PanelHeight = 266f;
        public const float TileSize = 80f;
        public const float Lift = 25f;
        public const float Margin = 14f;
        public const float XMargin = 4f;
        public const float OverlapGap = 10f;
        public const int OverlapIterations = 8;

        public static TooltipRect Ideal(Vector2 anchorCanvas, float w, float h)
        {
            float halfTile = TileSize / 2f;
            float x = anchorCanvas.X - w / 2f;
            float yAbove = anchorCanvas.Y - halfTile - h - Lift;
            float yBelow = anchorCanvas.Y + halfTile + Lift;
            float y;
            if (yAbove >= Margin) y = yAbove;
            else if (yBelow + h <= CanvasHeight - Margin) y = yBelow;
            else y = Math.Min(Math.Max(yAbove, Margin), Math.Max(Margin, CanvasHeight - h - Margin));
            return new TooltipRect(x, y, w, h);
        }

        public static void ResolveOverlaps(IList<TooltipRect> placements)
        {
            if (placements.Count <= 1) return;
            for (int iter = 0; iter < OverlapIterations; iter++)
            {
                for (int i = 0; i < placements.Count; i++)
                {
                    for (int j = i + 1; j < placements.Count; j++)
                    {
                        TooltipRect a = placements[i];
                        TooltipRect b = placements[j];
                        if (!TryGetOverlap(a, b, out float overlapW, out float overlapH)) continue;

                        if (overlapH >= overlapW)
                        {
                            float push = overlapH * 0.5f + OverlapGap * 0.5f;
                            if (a.Y + a.H * 0.5f < b.Y + b.H * 0.5f) { a.Y -= push; b.Y += push; }
                            else { a.Y += push; b.Y -= push; }
                        }
                        else
                        {
                            float push = overlapW * 0.5f + OverlapGap * 0.5f;
                            if (a.X + a.W * 0.5f < b.X + b.W * 0.5f) { a.X -= push; b.X += push; }
                            else { a.X += push; b.X -= push; }
                        }
                    }
                }
            }
        }

        public static void ClampToCanvas(IList<TooltipRect> placements)
        {
            foreach (TooltipRect p in placements)
            {
                p.X = Math.Min(Math.Max(p.X, XMargin), Math.Max(XMargin, CanvasWidth - p.W - XMargin));
                p.Y = Math.Min(Math.Max(p.Y, Margin), Math.Max(Margin, CanvasHeight - p.H - Margin));
            }
        }

        private static bool TryGetOverlap(TooltipRect a, TooltipRect b, out float overlapW, out float overlapH)
        {
            overlapW = Math.Min(a.X + a.W, b.X + b.W) - Math.Max(a.X, b.X);
            overlapH = Math.Min(a.Y + a.H, b.Y + b.H) - Math.Max(a.Y, b.Y);
            return overlapW > 0.5f && overlapH > 0.5f;
        }
    }
}
