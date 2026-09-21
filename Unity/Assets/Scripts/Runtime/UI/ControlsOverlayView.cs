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

        public VisualElement Root { get; private set; }
        public VisualElement Paper { get; private set; }
        public VisualElement Image { get; private set; }

        public void Bind(PauseMenuModel pause, PanelSettings panel)
        {
            Unsubscribe();
            var doc = GetComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.visualTreeAsset = UiResources.LoadTree("ControlsOverlay");
            model = pause;
            var docRoot = doc.rootVisualElement;
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

        private void OnEnable() { if (model != null) { Unsubscribe(); model.Changed += Refresh; Refresh(); } }
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
    }
}
