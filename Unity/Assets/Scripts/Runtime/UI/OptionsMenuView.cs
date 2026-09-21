using System;
using System.Collections.Generic;
using Gamelab.UI.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Options overlay with three volume rows. Hidden while the view model is closed.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class OptionsMenuView : MonoBehaviour
    {
        // Mirror OptionsMenu.uss (.om-track width 120, .om-thumb width 5). The USS is the source of truth.
        private const float TrackWidth = 120f;
        private const float ThumbWidth = 5f;
        private static readonly string[] RowLabels = { "Master", "Ambient", "Effects" };

        private OptionsViewModel vm;
        private PanelSettings panel;
        private readonly List<VisualElement> rows = new List<VisualElement>();
        private readonly List<VisualElement> thumbs = new List<VisualElement>();
        private readonly List<Label> percents = new List<Label>();

        public IReadOnlyList<VisualElement> Rows => rows;
        public IReadOnlyList<VisualElement> Thumbs => thumbs;
        public IReadOnlyList<Label> Percents => percents;
        public VisualElement Root { get; private set; }
        public VisualElement Paper { get; private set; }

        public void Bind(OptionsViewModel model, PanelSettings panel)
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
            docRoot.styleSheets.Add(UiResources.LoadStyle("OptionsMenu"));
            UiResources.LoadTree("OptionsMenu").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Paper = docRoot.Q("Paper");
            var container = docRoot.Q("Rows");
            rows.Clear(); thumbs.Clear(); percents.Clear();
            for (int i = 0; i < vm.Rows.Count; i++)
            {
                var row = new VisualElement();
                row.AddToClassList("om-row");
                var chevron = new VisualElement();
                chevron.AddToClassList("om-chevron");
                var label = new Label(RowLabels[i]);
                label.AddToClassList("om-label");
                label.AddToClassList("font-serif");
                var track = new VisualElement();
                track.AddToClassList("om-track");
                var thumb = new VisualElement();
                thumb.AddToClassList("om-thumb");
                track.Add(thumb);
                var percent = new Label();
                percent.AddToClassList("om-percent");
                percent.AddToClassList("font-serif");
                row.Add(chevron); row.Add(label); row.Add(track); row.Add(percent);
                container.Add(row);
                rows.Add(row); thumbs.Add(thumb); percents.Add(percent);
            }
            Subscribe();
            Refresh();
        }

        public void Refresh()
        {
            if (vm == null || Root == null) return;
            Root.style.display = vm.IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
            for (int i = 0; i < rows.Count; i++)
            {
                float v = vm.VolumeOf(vm.Rows[i]);
                rows[i].EnableInClassList("om-row--selected", i == vm.SelectionIndex);
                thumbs[i].style.left = v * (TrackWidth - ThumbWidth);
                percents[i].text = Mathf.RoundToInt(v * 100f) + " %";
            }
        }

        private void Subscribe()
        {
            vm.OnOpened += Refresh;
            vm.OnClosed += Refresh;
            vm.OnSelectionChanged += Refresh;
            vm.OnVolumeChanged += OnVolume;
        }

        private void OnVolume(OptionsViewModel.Row row) => Refresh();

        private void Unsubscribe()
        {
            if (vm == null) return;
            vm.OnOpened -= Refresh;
            vm.OnClosed -= Refresh;
            vm.OnSelectionChanged -= Refresh;
            vm.OnVolumeChanged -= OnVolume;
        }

        private void OnEnable() => Rebuild();
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
    }
}
