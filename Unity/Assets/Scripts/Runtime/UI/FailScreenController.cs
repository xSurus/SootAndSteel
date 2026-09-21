using System;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.Players;
using Gamelab.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Owns the fail incident report model, view and panel. Call Configure once, then it ticks in Update.
    /// The caller owns screen switching and sound setup (Src ResetGlobalParameters, LoadSound). The Src
    /// snowstorm particles are not ported (no IVfxService).
    /// </summary>
    public class FailScreenController : MonoBehaviour
    {
        private Func<IReadOnlyList<IInputActions>> players;
        private Action<string> playSound;
        private Action onReturn;
        private PanelSettings panel;

        public FailScreenModel Model { get; private set; }
        public FailScreenView View { get; private set; }

        public void Configure(FailureReason reason, PostDeathStatsSnapshot stats, int levelNumber, DateTime now,
            Action onReturn, Func<IReadOnlyList<IInputActions>> players, Action<string> playSound)
        {
            if (Model != null) throw new InvalidOperationException("Configure once");
            this.players = players;
            this.playSound = playSound;
            this.onReturn = onReturn;
            Model = new FailScreenModel(reason);
            Model.Cue += OnCue;
            Model.ReturnRequested += OnReturn;
            panel = UiPanel.Create();
            var go = new GameObject("FailScreenView");
            go.transform.SetParent(transform, false);
            View = go.AddComponent<FailScreenView>();
            View.Bind(Model, stats, PostDeathStatsText.BuildIncidentLine(levelNumber, now), panel);
        }

        public void Tick(float dt)
        {
            if (Model == null) return;
            Model.Update(dt, players?.Invoke().AnyPressedMenuConfirm() ?? false);
            View.Refresh();
        }

        // Unscaled time on purpose. This screen replaces gameplay, so a pause menu that left
        // Time.timeScale at 0 must not freeze the typewriter, its tick cadence, the stamp pop or the return delay.
        private void Update() => Tick(Time.unscaledDeltaTime);

        private void OnCue(string sound) => playSound?.Invoke(sound);

        private void OnReturn() => onReturn?.Invoke();

        private void OnDestroy()
        {
            if (Model != null)
            {
                Model.Cue -= OnCue;
                Model.ReturnRequested -= OnReturn;
            }
            if (panel != null) Destroy(panel);
        }
    }
}
