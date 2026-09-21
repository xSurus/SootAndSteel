using System.Collections.Generic;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Run;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// World tooltips of Src WorldUiManager, screen space. One ToolTipView per id, placed with TooltipLayout
    /// (above or below the anchor, overlap separation across visible tooltips, clamp). Call Tick every frame.
    /// </summary>
    public sealed class TooltipLayer
    {
        private sealed class Entry
        {
            public ITooltipable Item;
            public TooltipKind Kind;
            public TooltipModel Model;
            public ToolTipView View;
        }

        private readonly Transform parent;
        private readonly PanelSettings panel;
        private readonly RunCredits credits;
        private readonly IWorldToScreen projector;
        private readonly Dictionary<object, Entry> entries = new Dictionary<object, Entry>();
        private readonly List<Entry> visible = new List<Entry>();
        private readonly List<TooltipRect> rects = new List<TooltipRect>();
        private int nextOrder = 10;

        public TooltipLayer(Transform parent, PanelSettings panel, RunCredits credits, IWorldToScreen projector)
        {
            this.parent = parent;
            this.panel = panel;
            this.credits = credits;
            this.projector = projector;
        }

        public int Count => entries.Count;

        public ToolTipView ViewOf(object id) => entries.TryGetValue(id, out var e) ? e.View : null;

        /// <summary>Creates the tooltip for id, or rebuilds it when the item or kind changed.</summary>
        public void Set(object id, ITooltipable item, TooltipKind kind)
        {
            if (!entries.TryGetValue(id, out var e))
            {
                var go = new GameObject("ToolTip");
                go.transform.SetParent(parent, false);
                var doc = go.AddComponent<UIDocument>();
                doc.sortingOrder = nextOrder++;
                e = new Entry { View = go.AddComponent<ToolTipView>() };
                entries[id] = e;
            }
            else if (e.Item == item && e.Kind == kind) return;
            e.Item = item;
            e.Kind = kind;
            e.Model = TooltipModel.Create(item, kind, credits.Credits);
            e.View.Bind(e.Model, panel);
        }

        public void Clear(object id)
        {
            if (!entries.TryGetValue(id, out var e)) return;
            entries.Remove(id);
            Object.Destroy(e.View.gameObject);
        }

        public void ClearAll()
        {
            foreach (var e in entries.Values) Object.Destroy(e.View.gameObject);
            entries.Clear();
        }

        public void Tick()
        {
            visible.Clear();
            rects.Clear();
            Vector2 size = projector.ScreenSize;
            float scale = Mathf.Min(size.x / TooltipLayout.CanvasWidth, size.y / TooltipLayout.CanvasHeight);
            foreach (var e in entries.Values)
            {
                var root = e.View.Root;
                if (root == null) continue;
                bool show = e.Item.IsVisible;
                root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (!show) continue;
                e.Model.Refresh(credits.Credits);
                Vector2 canvas = projector.ToScreen(new Vector2(e.Item.Position.X, e.Item.Position.Y)) / scale;
                rects.Add(TooltipLayout.Ideal(new System.Numerics.Vector2(canvas.x, canvas.y),
                    TooltipLayout.PanelWidth, TooltipLayout.PanelHeight));
                visible.Add(e);
            }
            TooltipLayout.ResolveOverlaps(rects);
            TooltipLayout.ClampToCanvas(rects);
            for (int i = 0; i < visible.Count; i++)
            {
                var root = visible[i].View.Root;
                root.style.left = rects[i].X;
                root.style.top = rects[i].Y;
            }
        }
    }
}
