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
    /// <summary>Owns the main menu models, views and navigator. Call Configure once, then it ticks in Update.</summary>
    public class MainMenuController : MonoBehaviour
    {
        private MenuNavigator navigator;
        private PanelSettings panel;

        public MainMenuViewModel Menu { get; private set; }
        public OptionsViewModel Options { get; private set; }
        public MainMenuView MenuView { get; private set; }
        public OptionsMenuView OptionsView { get; private set; }

        /// <summary>
        /// Uses SoundServiceRunner for volume and MenuSelect, and the PlayerJoinManager roster for players.
        /// Both must exist in the scene. The main menu has no player before the join screen, so the roster
        /// may be empty (input gap recorded in CONVENTIONS).
        /// </summary>
        public void Configure(bool hasSave, Action onContinue, Action onNewGame, Action onQuit, float volumeStep = 0.05f)
        {
            ISoundService sound = SoundServiceRunner.Instance.SoundService;
            sound.LoadSound(Sounds.MenuSelect);
            var join = FindFirstObjectByType<PlayerJoinManager>();
            Configure(hasSave, onContinue, onNewGame, onQuit, new SoundVolumeSettings(sound), volumeStep,
                () =>
                {
                    var list = new List<IInputActions>();
                    if (join != null) foreach (var s in join.Roster.Slots) list.Add(s.Input);
                    return list;
                },
                () => sound.PlayOnce(Sounds.MenuSelect));
        }

        public void Configure(bool hasSave, Action onContinue, Action onNewGame, Action onQuit,
            IVolumeSettings volume, float volumeStep, Func<IReadOnlyList<IInputActions>> players, Action playSelect)
        {
            Options = new OptionsViewModel(volume) { VolumeStep = volumeStep };
            var entries = new List<MainMenuViewModel.Entry>();
            if (hasSave) entries.Add(new MainMenuViewModel.Entry("Continue", onContinue));
            entries.Add(new MainMenuViewModel.Entry("New Game", onNewGame));
            entries.Add(new MainMenuViewModel.Entry("Options", Options.Open));
            entries.Add(new MainMenuViewModel.Entry("Quit", onQuit));
            Menu = new MainMenuViewModel(entries);
            navigator = new MenuNavigator(players, playSelect);

            panel = UiPanel.Create();
            MenuView = AddView<MainMenuView>("MainMenuView");
            MenuView.Bind(Menu, panel);
            OptionsView = AddView<OptionsMenuView>("OptionsMenuView");
            OptionsView.Bind(Options, panel);
            MenuView.GetComponent<UIDocument>().sortingOrder = 0;
            OptionsView.GetComponent<UIDocument>().sortingOrder = 1;
        }

        private T AddView<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        private void Update()
        {
            navigator?.TickMainMenu(Menu, Options);
        }

        private void OnDestroy()
        {
            if (panel != null) Destroy(panel);
        }
    }
}
