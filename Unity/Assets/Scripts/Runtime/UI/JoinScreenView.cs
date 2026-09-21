using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Join screen (Gum JoinScreen with four JoinPlayerComponent slots). The model does not raise
    /// Changed for the initial all-empty state, so the view refreshes on Bind and rebuild and again
    /// on Changed. The owner calls model.Refresh() every frame.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class JoinScreenView : MonoBehaviour
    {
        private JoinScreenModel model;
        private readonly VisualElement[] figures = new VisualElement[JoinScreenModel.SlotCount];
        private readonly VisualElement[] joinButtons = new VisualElement[JoinScreenModel.SlotCount];

        public VisualElement Root { get; private set; }
        public Label Title { get; private set; }
        public VisualElement Figures { get; private set; }

        public VisualElement Slot(int i) => Root.Q("Slot" + i);
        public VisualElement Figure(int i) => figures[i];
        public VisualElement JoinButton(int i) => joinButtons[i];
        public Label JoinText(int i) => joinButtons[i].Q<Label>("Text");
        public VisualElement JoinIcon(int i) => joinButtons[i].Q("Icon");

        public void Bind(JoinScreenModel model, PanelSettings panel)
        {
            if (this.model != null) this.model.Changed -= Refresh;
            GetComponent<UIDocument>().panelSettings = panel;
            this.model = model;
            model.Changed += Refresh;
            Rebuild();
        }

        private void Rebuild()
        {
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            if (model == null || docRoot == null) return;
            docRoot.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("HubCommon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("ButtonWithIcon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("JoinScreenView"));
            UiResources.LoadTree("JoinScreenView").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Title = docRoot.Q<Label>("Title");
            Figures = docRoot.Q("Figures");
            for (int i = 0; i < JoinScreenModel.SlotCount; i++)
            {
                var player = new VisualElement { name = "Player" + i, pickingMode = PickingMode.Ignore };
                player.AddToClassList("jsv-player");
                var figure = new VisualElement { name = "Figure" + i, pickingMode = PickingMode.Ignore };
                figure.AddToClassList("jsv-figure");
                var join = new VisualElement { name = "Join" + i, pickingMode = PickingMode.Ignore };
                join.AddToClassList("jsv-join");
                ButtonWithIconElement.Mount(join, false);
                player.Add(figure);
                player.Add(join);
                Slot(i).Add(player);
                figures[i] = figure;
                joinButtons[i] = join;
            }
            Refresh();
        }

        public void Refresh()
        {
            if (Root == null || model == null) return;
            Title.text = model.Title;
            for (int i = 0; i < JoinScreenModel.SlotCount; i++)
            {
                figures[i].style.backgroundImage = new StyleBackground(Resources.Load<Sprite>("UI/Art/" + model.FigureId(i)));
                ButtonWithIconElement.Set(joinButtons[i], model.IsJoined(i) ? ButtonIcon.Start : ButtonIcon.A, model.ButtonText(i));
            }
        }

        private void OnEnable() => Rebuild();

        private void OnDestroy()
        {
            if (model != null) model.Changed -= Refresh;
        }
    }
}
