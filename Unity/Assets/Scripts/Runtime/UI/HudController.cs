using System;
using Gamelab.Config;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Owns the HUD model, view and panel. Update reads the train state and refreshes the view every frame
    /// (HudModel has no events). It uses no clock, so it runs unchanged while paused: the needle smoothing is
    /// per call, as in Src. Call Bind once.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        // Below the hub views (0..2) and the tooltip layer, so menus and tooltips draw above the HUD.
        private const int SortingOrder = -10;

        private TrainState state;
        private float maxSpeed;
        private LevelDefinition level;
        private PanelSettings panel;

        public HudModel Model { get; private set; }
        public HudView View { get; private set; }

        public void Bind(TrainState state, LevelDefinition level, float maxSpeed = -1f)
        {
            if (Model != null) throw new InvalidOperationException("Bind once");
            this.state = state;
            this.level = level;
            this.maxSpeed = maxSpeed >= 0f ? maxSpeed : TrainSpeedTuning.Default.SpeedFast;
            Model = new HudModel(level);
            panel = UiPanel.Create();
            var go = new GameObject("HudView");
            go.transform.SetParent(transform, false);
            go.AddComponent<UIDocument>().sortingOrder = SortingOrder;
            View = go.AddComponent<HudView>();
            View.Bind(Model, panel);
        }

        public void Bind(TrainStateRuntime train, LevelRuntime level) =>
            Bind(train.State, level.Definition, train.Speeds.Fast.TargetSpeed);

        /// <summary>Rebuilds the dots only when the level reference changes (Src EnsureEnemyDots).</summary>
        public void SetLevel(LevelDefinition def)
        {
            if (Model == null || ReferenceEquals(def, level)) return;
            level = def;
            Model.SetLevel(def);
        }

        public void Tick()
        {
            if (Model == null || state == null) return;
            Model.Update(state.DistanceTraveled, state.actualSpeed, state.Temperature, state.MaxTemperature, maxSpeed);
            View.Refresh();
        }

        private void Update() => Tick();

        private void OnDestroy()
        {
            if (panel != null) Destroy(panel);
        }
    }
}
