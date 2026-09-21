using System;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Owns the post-level waybill model, view and panel. Call Configure once, then it ticks in Update.
    /// The caller owns the save system, screen switching and sound setup (Src ResetGlobalParameters, LoadSound).
    /// </summary>
    public class PostLevelStatsController : MonoBehaviour
    {
        private Func<IReadOnlyList<IInputActions>> players;
        private Action<string> playSound;
        private Action onContinue;
        private PanelSettings panel;

        public PostLevelStatsModel Model { get; private set; }
        public PostLevelStatsView View { get; private set; }

        public void Configure(float actualTime, float referenceTime, int stageNumber, Action<int> grantCredits,
            Action onContinue, Func<IReadOnlyList<IInputActions>> players, Action<string> playSound)
        {
            if (Model != null) throw new InvalidOperationException("Configure once");
            this.players = players;
            this.playSound = playSound;
            this.onContinue = onContinue;
            var rewards = LevelRewardBreakdown.FromCompletion(actualTime, referenceTime);
            Model = new PostLevelStatsModel(rewards, grantCredits);
            Model.Cue += OnCue;
            Model.ContinueRequested += OnContinue;
            panel = UiPanel.Create();
            var go = new GameObject("PostLevelStatsView");
            go.transform.SetParent(transform, false);
            go.AddComponent<UIDocument>();
            View = go.AddComponent<PostLevelStatsView>();
            View.Bind(Model, StageNaming.GetStageTitle(stageNumber), panel);
        }

        public void Tick(float dt)
        {
            if (Model == null) return;
            Model.Update(dt, MenuConfirm.Any(players));
            View.Refresh();
        }

        // Unscaled time on purpose. This screen replaces gameplay, so a pause menu that left
        // Time.timeScale at 0 must not freeze the reveal clock, the count-up, the stamp pop or the fade.
        private void Update() => Tick(Time.unscaledDeltaTime);

        private void OnCue(string sound) => playSound?.Invoke(sound);

        private void OnContinue() => onContinue?.Invoke();

        private void OnDestroy()
        {
            if (Model != null)
            {
                Model.Cue -= OnCue;
                Model.ContinueRequested -= OnContinue;
            }
            if (panel != null) Destroy(panel);
        }
    }
}
