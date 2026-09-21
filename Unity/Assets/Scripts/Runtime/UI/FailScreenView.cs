using Gamelab.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Fail incident report (Gum PostDeathOverlay). The controller calls Refresh every frame.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class FailScreenView : MonoBehaviour
    {
        private FailScreenModel model;
        private PostDeathStatsSnapshot stats;
        private string incidentLine;

        public VisualElement Root { get; private set; }
        public VisualElement Paper { get; private set; }
        public VisualElement Vignette { get; private set; }
        public VisualElement Stamp { get; private set; }
        public VisualElement ReturnButton { get; private set; }
        public Label IncidentLine { get; private set; }
        public Label CauseText { get; private set; }
        public Label EnemiesStat { get; private set; }
        public Label DistanceStat { get; private set; }
        public Label StagesStat { get; private set; }
        public Label UpgradesStat { get; private set; }

        public void Bind(FailScreenModel model, PostDeathStatsSnapshot stats, string incidentLine, PanelSettings panel)
        {
            GetComponent<UIDocument>().panelSettings = panel;
            this.model = model;
            this.stats = stats;
            this.incidentLine = incidentLine;
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
            docRoot.styleSheets.Add(UiResources.LoadStyle("FailScreenView"));
            UiResources.LoadTree("FailScreenView").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Paper = docRoot.Q("Paper");
            Vignette = docRoot.Q("Vignette");
            Stamp = docRoot.Q("Stamp");
            ReturnButton = docRoot.Q("ReturnButton");
            IncidentLine = docRoot.Q<Label>("IncidentLine");
            CauseText = docRoot.Q<Label>("CauseText");
            EnemiesStat = docRoot.Q<Label>("EnemiesStat");
            DistanceStat = docRoot.Q<Label>("DistanceStat");
            StagesStat = docRoot.Q<Label>("StagesStat");
            UpgradesStat = docRoot.Q<Label>("UpgradesStat");

            IncidentLine.text = incidentLine ?? "";
            EnemiesStat.text = PostDeathStatsText.EnemiesDefeated(stats);
            DistanceStat.text = PostDeathStatsText.Distance(stats);
            StagesStat.text = PostDeathStatsText.StagesDefeated(stats);
            UpgradesStat.text = PostDeathStatsText.UpgradesBought(stats);
            ButtonWithIconElement.Mount(ReturnButton, false);
            ButtonWithIconElement.Set(ReturnButton, ButtonIcon.A, "Return");
            Refresh();
        }

        public void Refresh()
        {
            if (Root == null || model == null) return;
            CauseText.text = model.CauseText;
            StampView.Apply(Stamp, model.Stamp, 10f);
        }

        private void OnEnable() => Rebuild();
    }
}
