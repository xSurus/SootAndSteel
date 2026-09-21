using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Hint and decision bubble at the bottom centre. Passive shows text only, decision adds two buttons
    /// (Y on the left, X on the right, as DialogueOverlay.ConfigureInteractionRow), hidden shows nothing.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DialogBubbleView : MonoBehaviour
    {
        private DialogBubbleModel model;

        public VisualElement Root { get; private set; }
        public VisualElement Bubble { get; private set; }
        public Label Speaker { get; private set; }
        public Label Body { get; private set; }
        public VisualElement Interactions { get; private set; }
        public VisualElement LeftButton { get; private set; }
        public VisualElement RightButton { get; private set; }

        public void Bind(DialogBubbleModel bubble, PanelSettings panel)
        {
            Unsubscribe();
            GetComponent<UIDocument>().panelSettings = panel;
            model = bubble;
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
            docRoot.styleSheets.Add(UiResources.LoadStyle("HubCommon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("ButtonWithIcon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("DialogBubble"));
            UiResources.LoadTree("DialogBubble").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Bubble = docRoot.Q("Bubble");
            Speaker = docRoot.Q<Label>("Speaker");
            Body = docRoot.Q<Label>("Body");
            Interactions = docRoot.Q("Interactions");
            LeftButton = docRoot.Q("LeftButton");
            RightButton = docRoot.Q("RightButton");
            ButtonWithIconElement.Mount(LeftButton, true);
            ButtonWithIconElement.Mount(RightButton, true);
            model.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            if (model == null || Root == null) return;
            var mode = model.Mode;
            Root.style.display = mode == DialogBubbleModel.BubbleMode.Hidden ? DisplayStyle.None : DisplayStyle.Flex;
            Speaker.text = model.Speaker ?? "";
            Body.text = model.Text ?? "";
            Interactions.style.display = mode == DialogBubbleModel.BubbleMode.Decision ? DisplayStyle.Flex : DisplayStyle.None;
            ButtonWithIconElement.Set(LeftButton, ButtonIcon.Y, model.LeftLabel);
            ButtonWithIconElement.Set(RightButton, ButtonIcon.X, model.RightLabel);
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
