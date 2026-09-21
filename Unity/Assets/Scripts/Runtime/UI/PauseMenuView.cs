using System.Collections.Generic;
using Gamelab.UI.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Pause paper with four rows. Shown only while paused with options and controls closed.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuView : MonoBehaviour
    {
        private static readonly string[] RowLabels = { "Continue", "Options", "Controls", "Exit" };

        private PauseMenuModel model;
        private readonly List<VisualElement> rows = new List<VisualElement>();

        public IReadOnlyList<VisualElement> Rows => rows;
        public VisualElement Root { get; private set; }
        public VisualElement Paper { get; private set; }
        public VisualElement Menu { get; private set; }

        public void Bind(PauseMenuModel pause, PanelSettings panel)
        {
            Unsubscribe();
            var doc = GetComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.visualTreeAsset = UiResources.LoadTree("PauseMenu");
            model = pause;
            var docRoot = doc.rootVisualElement;
            docRoot.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("PauseMenu"));
            UiResources.LoadTree("PauseMenu").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Paper = docRoot.Q("Paper");
            Menu = docRoot.Q("Menu");
            rows.Clear();
            for (int i = 0; i < RowLabels.Length; i++)
            {
                var row = new VisualElement();
                row.AddToClassList("pm-row");
                row.AddToClassList("pm-row--" + i);
                var chevron = new VisualElement();
                chevron.AddToClassList("pm-chevron");
                var label = new Label(RowLabels[i]);
                label.AddToClassList("pm-label");
                label.AddToClassList("font-serif");
                row.Add(chevron);
                row.Add(label);
                Menu.Add(row);
                rows.Add(row);
            }
            model.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            if (model == null || Root == null) return;
            bool show = model.IsPaused && !model.Options.IsOpen && !model.ControlsOpen;
            Root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            for (int i = 0; i < rows.Count; i++)
                rows[i].EnableInClassList("pm-row--selected", i == model.SelectionIndex);
        }

        private void Unsubscribe()
        {
            if (model != null) model.Changed -= Refresh;
        }

        private void OnEnable() { if (model != null) { Unsubscribe(); model.Changed += Refresh; Refresh(); } }
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
    }
}
