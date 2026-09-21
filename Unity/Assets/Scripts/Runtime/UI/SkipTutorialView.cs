using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Hold-to-skip element (Gum SkipTutorial), bottom right. It has no clock and shows what the
    /// model holds, the caller feeds the model and calls Refresh. The caller also picks the
    /// document sorting order (the gameplay HUD uses -10).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SkipTutorialView : MonoBehaviour
    {
        // Gum ProgressBar container width, the value Src reads as MaxWidth.
        private const float BarWidth = 217f;

        private SkipTutorialModel model;

        public VisualElement Root { get; private set; }
        public VisualElement Icon { get; private set; }
        public Label Text { get; private set; }
        public VisualElement ProgressBar { get; private set; }
        public VisualElement Fill { get; private set; }

        public void Bind(SkipTutorialModel model, PanelSettings panel)
        {
            GetComponent<UIDocument>().panelSettings = panel;
            this.model = model;
            Rebuild();
        }

        private void Rebuild()
        {
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            if (model == null || docRoot == null) return;
            docRoot.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("HubCommon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("SkipTutorialView"));
            UiResources.LoadTree("SkipTutorialView").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Icon = docRoot.Q("Icon");
            Text = docRoot.Q<Label>("Text");
            ProgressBar = docRoot.Q("ProgressBar");
            Fill = docRoot.Q("ProgressBarProgress");
            Text.text = SkipTutorialModel.Text;
            Refresh();
        }

        public void Refresh()
        {
            if (Root == null || model == null) return;
            ProgressBar.style.width = Mathf.Lerp(0f, BarWidth, model.ProgressRatio);
        }

        private void OnEnable() => Rebuild();
    }
}
