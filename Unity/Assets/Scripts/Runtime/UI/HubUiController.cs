using System;
using System.Collections.Generic;
using Gamelab.Players;
using Gamelab.Run;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Owns the hub overlay, crafting help, dialog bubble and tooltip layer, and ticks HubInput in Update
    /// (Src HubScreen.Update). The caller supplies the models and callbacks, and stops the controller
    /// (enabled = false) while paused, as Src skips the hub update then. Call Bind once.
    /// </summary>
    public class HubUiController : MonoBehaviour
    {
        private HubInput input;
        private HubDepartureModel departure;
        private CraftingHelpModel help;
        private Func<int> pendingOffBoard;
        private PanelSettings panel;

        public HubOverlayView OverlayView { get; private set; }
        public CraftingHelpView HelpView { get; private set; }
        public DialogBubbleView BubbleView { get; private set; }
        public TooltipLayer Tooltips { get; private set; }

        /// <summary>Raised when the depart hold timer completes (HubDepartureModel.DepartRequested).</summary>
        public event Action DepartRequested;

        /// <param name="pendingOffBoard">Bought items still off the train, evaluated each tick (needs the map).</param>
        public void Bind(RunCredits credits, HubDepartureModel departure, CraftingHelpModel help,
            Func<IReadOnlyList<PlayerSlot>> players, Func<int> pendingOffBoard, IWorldToScreen projector)
        {
            this.departure = departure;
            this.help = help;
            this.pendingOffBoard = pendingOffBoard;
            input = new HubInput(players);
            panel = UiPanel.Create();
            OverlayView = AddView<HubOverlayView>("HubOverlayView", 0);
            OverlayView.Bind(credits, departure, panel);
            HelpView = AddView<CraftingHelpView>("CraftingHelpView", 1);
            HelpView.Bind(help, panel);
            BubbleView = AddView<DialogBubbleView>("DialogBubbleView", 2);
            BubbleView.Bind(departure.Bubble, panel);
            Tooltips = new TooltipLayer(transform, panel, credits, projector);
            departure.DepartRequested += OnDepart;
        }

        private T AddView<T>(string name, int order) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.AddComponent<UIDocument>().sortingOrder = order;
            return go.AddComponent<T>();
        }

        public void Tick(float dt)
        {
            if (input == null) return;
            input.Tick(dt, departure, help, pendingOffBoard());
            Tooltips.Tick();
        }

        private void Update() => Tick(Time.deltaTime);

        private void OnDepart() => DepartRequested?.Invoke();

        private void OnDestroy()
        {
            if (departure != null) departure.DepartRequested -= OnDepart;
            if (panel != null) Destroy(panel);
        }
    }
}
