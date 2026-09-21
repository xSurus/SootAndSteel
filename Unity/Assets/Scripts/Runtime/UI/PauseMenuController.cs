using System;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.Players.Runtime;
using Gamelab.Services.Sound;
using Gamelab.UI.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Owns the pause models and views. Freezes time while paused (Time.timeScale 0) and restores the
    /// previous value on unpause, disable or destroy. Call Configure once.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        private MenuNavigator navigator;
        private PanelSettings panel;
        private bool freezing;
        private float savedTimeScale = 1f;

        public PauseMenuModel Model { get; private set; }
        public PauseMenuView PauseView { get; private set; }
        public ControlsOverlayView ControlsView { get; private set; }
        public OptionsMenuView OptionsView { get; private set; }

        /// <summary>Uses SoundServiceRunner for volume and MenuSelect, and the PlayerJoinManager roster for players.</summary>
        public void Configure(Action onExitRequested, float volumeStep = 0.05f)
        {
            ISoundService sound = SoundServiceRunner.Instance.SoundService;
            sound.LoadSound(Sounds.MenuSelect);
            var join = FindFirstObjectByType<PlayerJoinManager>();
            Configure(new SoundVolumeSettings(sound), volumeStep,
                () =>
                {
                    var list = new List<IInputActions>();
                    if (join != null) foreach (var s in join.Roster.Slots) list.Add(s.Input);
                    return list;
                },
                () => sound.PlayOnce(Sounds.MenuSelect), onExitRequested);
        }

        public void Configure(IVolumeSettings volume, float volumeStep, Func<IReadOnlyList<IInputActions>> players,
            Action playSelect, Action onExitRequested)
        {
            var options = new OptionsViewModel(volume) { VolumeStep = volumeStep };
            Model = new PauseMenuModel(options);
            Model.ExitRequested += () => onExitRequested?.Invoke();
            navigator = new MenuNavigator(players, playSelect);

            panel = UiPanel.Create();
            PauseView = AddView<PauseMenuView>("PauseMenuView", 0);
            PauseView.Bind(Model, panel);
            ControlsView = AddView<ControlsOverlayView>("ControlsOverlayView", 1);
            ControlsView.Bind(Model, panel);
            OptionsView = AddView<OptionsMenuView>("OptionsMenuView", 2);
            OptionsView.Bind(options, panel);
            Model.Changed += ApplyTimeScale;
        }

        private T AddView<T>(string name, int order) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var doc = go.AddComponent<UIDocument>();
            doc.sortingOrder = order;
            return go.AddComponent<T>();
        }

        private void ApplyTimeScale()
        {
            if (Model.IsPaused && !freezing)
            {
                freezing = true;
                savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else if (!Model.IsPaused) Restore();
        }

        private void Restore()
        {
            if (!freezing) return;
            freezing = false;
            Time.timeScale = savedTimeScale;
        }

        private void Update()
        {
            navigator?.TickPause(Model);
        }

        private void OnDisable() => Restore();

        private void OnDestroy()
        {
            Restore();
            if (Model != null) Model.Changed -= ApplyTimeScale;
            if (panel != null) Destroy(panel);
        }
    }
}
