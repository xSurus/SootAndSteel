using System;
using Gamelab.Players.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Binds the join screen view to an existing JoinFlowController. Slots read IsSlotJoined, and
    /// OnReadyToAdvance calls onAdvance. There is no clock: the join screen has no animation.
    /// Src JoinScreen calls playerManager.Reset() on init. That is not done here. A caller that wants
    /// the Src behaviour resets the roster (PlayerJoinManager.ResetJoins) before showing the screen.
    /// </summary>
    public class JoinScreenController : MonoBehaviour
    {
        private JoinFlowController flow;
        private Action onAdvance;
        private PanelSettings panel;

        public JoinScreenModel Model { get; private set; }
        public JoinScreenView View { get; private set; }

        public void Bind(JoinFlowController flow, Action onAdvance)
        {
            if (Model != null) throw new InvalidOperationException("Bind once");
            this.flow = flow;
            this.onAdvance = onAdvance;
            Model = new JoinScreenModel(JoinedCount);
            panel = UiPanel.Create();
            var go = new GameObject("JoinScreenView");
            go.transform.SetParent(transform, false);
            View = go.AddComponent<JoinScreenView>();
            View.Bind(Model, panel);
            flow.OnReadyToAdvance.AddListener(OnAdvance);
        }

        private int JoinedCount()
        {
            int n = 0;
            for (int i = 0; i < JoinScreenModel.SlotCount; i++)
                if (flow.IsSlotJoined(i)) n++;
            return n;
        }

        // Raises Changed only on change, and the view refreshes on Changed.
        private void Update() => Model?.Refresh();

        private void OnAdvance() => onAdvance?.Invoke();

        private void OnDestroy()
        {
            if (flow != null) flow.OnReadyToAdvance.RemoveListener(OnAdvance);
            if (panel != null) Destroy(panel);
        }
    }
}
