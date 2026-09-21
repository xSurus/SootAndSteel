using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Gameplay HUD: distance gauge with train marker and enemy dots, speedometer and frost overlay.
    /// Positions are Gum numbers in 1920x1080 canvas units. Placement and draw order cannot be checked
    /// headless. Frost sits first in the tree (under the gauge), which is a judgement call because
    /// the Src draw order of frost against the Gum layer was not confirmed.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class HudView : MonoBehaviour
    {
        // Must match .hud-track width in Hud.uss. The HudMath.TrackTravelRange "width <= 1" guard is
        // therefore never hit here (kept for Src parity).
        private const float TrackWidth = 503.5f;
        private const float DotSize = 50f;

        private HudModel model;
        private int builtVersion = -1;

        public VisualElement Root { get; private set; }
        public VisualElement Parchment { get; private set; }
        public VisualElement Track { get; private set; }
        public VisualElement Speedometer { get; private set; }
        public VisualElement NeedleContainer { get; private set; }
        public VisualElement Needle { get; private set; }
        public VisualElement TrainMarker { get; private set; }
        public VisualElement GoalX { get; private set; }
        public VisualElement Frost1 { get; private set; }
        public VisualElement Frost2 { get; private set; }
        public VisualElement Frost3 { get; private set; }
        public List<VisualElement> Dots { get; } = new List<VisualElement>();

        public void Bind(HudModel model, PanelSettings panel)
        {
            GetComponent<UIDocument>().panelSettings = panel;
            this.model = model;
            Rebuild();
        }

        // UIDocument recreates rootVisualElement on disable/enable, so the tree is rebuilt on every enable.
        private void Rebuild()
        {
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            if (model == null || docRoot == null) return;
            docRoot.Clear();
            docRoot.styleSheets.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("Hud"));
            UiResources.LoadTree("Hud").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Parchment = docRoot.Q("Parchment");
            Track = docRoot.Q("Track");
            Speedometer = docRoot.Q("Speedometer");
            NeedleContainer = docRoot.Q("NeedleContainer");
            Needle = docRoot.Q("Needle");
            TrainMarker = docRoot.Q("TrainMarker");
            GoalX = docRoot.Q("GoalX");
            Frost1 = docRoot.Q("Frost1");
            Frost2 = docRoot.Q("Frost2");
            Frost3 = docRoot.Q("Frost3");
            Dots.Clear();
            builtVersion = -1;
            Refresh();
        }

        public void Refresh()
        {
            if (Root == null || model == null) return;
            float range = HudMath.TrackTravelRange(TrackWidth);
            if (builtVersion != model.LevelVersion)
            {
                var host = Root.Q("Dots");
                host.Clear();
                Dots.Clear();
                foreach (float ratio in model.DotRatios)
                {
                    var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                    dot.AddToClassList("hud-dot");
                    dot.style.left = ratio * range - DotSize / 2f;
                    host.Add(dot);
                    Dots.Add(dot);
                }
                builtVersion = model.LevelVersion;
            }
            TrainMarker.style.left = model.DistanceRatio * range - 16f;
            // Gum rotation is counter-clockwise, USS is clockwise.
            NeedleContainer.style.rotate = new Rotate(new Angle(-model.NeedleDegrees, AngleUnit.Degree));
            Frost1.style.opacity = model.FrostOpacity1;
            Frost2.style.opacity = model.FrostOpacity2;
            Frost3.style.opacity = model.FrostOpacity3;
        }

        private void OnEnable() => Rebuild();
    }
}
