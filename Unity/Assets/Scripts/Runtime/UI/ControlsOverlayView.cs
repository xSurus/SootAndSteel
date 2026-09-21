using Gamelab.UI.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Controller diagram overlay. Visible only while paused with controls open.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class ControlsOverlayView : MonoBehaviour
    {
        private PauseMenuModel model;
        private PanelSettings panel;

        public VisualElement Root { get; private set; }
        public VisualElement Paper { get; private set; }
        public VisualElement Image { get; private set; }

        public void Bind(PauseMenuModel pause, PanelSettings panel)
        {
            Unsubscribe();
            GetComponent<UIDocument>().panelSettings = panel;
            this.panel = panel;
            model = pause;
            Rebuild();
        }

        // UIDocument recreates rootVisualElement on disable/enable, so the tree is rebuilt on every enable.
        private void Rebuild()
        {
            Unsubscribe();
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            if (model == null || docRoot == null) return;
            docRoot.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("ControlsOverlay"));
            UiResources.LoadTree("ControlsOverlay").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Paper = docRoot.Q("Paper");
            Image = docRoot.Q("Image");
            model.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            if (model == null || Root == null) return;
            Root.style.display = model.IsPaused && model.ControlsOpen ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Unsubscribe()
        {
            if (model != null) model.Changed -= Refresh;
        }

        private void OnEnable() => Rebuild();
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
    }
}
