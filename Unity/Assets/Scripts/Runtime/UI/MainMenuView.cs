using System.Collections.Generic;
using Gamelab.UI.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Main menu screen. Rows are built from the view model entries.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuView : MonoBehaviour
    {
        private MainMenuViewModel vm;
        private PanelSettings panel;
        private readonly List<VisualElement> rows = new List<VisualElement>();
        private readonly List<Label> tags = new List<Label>();

        public IReadOnlyList<VisualElement> Rows => rows;
        public VisualElement Root { get; private set; }
        public VisualElement Menu { get; private set; }

        public void Bind(MainMenuViewModel model, PanelSettings panel)
        {
            Unsubscribe();
            GetComponent<UIDocument>().panelSettings = panel;
            this.panel = panel;
            vm = model;
            Rebuild();
        }

        // UIDocument recreates rootVisualElement on disable/enable, so the tree is rebuilt on every enable.
        private void Rebuild()
        {
            Unsubscribe();
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            if (vm == null || docRoot == null) return;
            docRoot.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("MainMenu"));
            UiResources.LoadTree("MainMenu").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Menu = docRoot.Q("Menu");
            rows.Clear();
            tags.Clear();
            foreach (var entry in vm.Entries)
            {
                var row = new VisualElement();
                row.AddToClassList("mm-row");
                var tag = new Label();
                tag.AddToClassList("mm-tags");
                tag.AddToClassList("font-mono");
                var label = new Label(entry.Label);
                label.AddToClassList("mm-label");
                label.AddToClassList("font-mono");
                row.Add(tag);
                row.Add(label);
                Menu.Add(row);
                rows.Add(row);
                tags.Add(tag);
            }
            vm.OnSelectionChanged += OnSelectionChanged;
            Refresh();
        }

        public void Refresh()
        {
            if (vm == null) return;
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].EnableInClassList("mm-row--selected", vm.IsEntrySelectedByAnyone(i));
                var names = new List<string>();
                foreach (var kv in vm.PlayerSelections)
                    if (kv.Value == i) names.Add("P" + (kv.Key + 1));
                names.Sort();
                tags[i].text = string.Join(" ", names);
            }
        }

        private void OnSelectionChanged(int player, int entry) => Refresh();

        private void Unsubscribe()
        {
            if (vm != null) vm.OnSelectionChanged -= OnSelectionChanged;
        }

        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
        private void OnEnable() => Rebuild();
    }
}
