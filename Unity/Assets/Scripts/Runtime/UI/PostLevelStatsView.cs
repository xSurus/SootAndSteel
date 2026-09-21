using Gamelab.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Post-level waybill (Gum PostStatsDisplay). The controller calls Refresh every frame.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class PostLevelStatsView : MonoBehaviour
    {
        private const int PunchCount = 15;

        private PostLevelStatsModel model;
        private string stageTitle;

        public VisualElement Root { get; private set; }
        public VisualElement Paper { get; private set; }
        public VisualElement Stamp { get; private set; }
        public VisualElement Fade { get; private set; }
        public VisualElement DeliveryRow { get; private set; }
        public VisualElement TimeRow { get; private set; }
        public VisualElement SummaryRow { get; private set; }
        public VisualElement Punches { get; private set; }
        public VisualElement ContinueButton { get; private set; }
        public Label StageTitle { get; private set; }
        public Label Summary { get; private set; }
        public Label SummaryDescription { get; private set; }

        public void Bind(PostLevelStatsModel model, string stageTitle, PanelSettings panel)
        {
            GetComponent<UIDocument>().panelSettings = panel;
            this.model = model;
            this.stageTitle = stageTitle;
            Rebuild();
        }

        // UIDocument recreates rootVisualElement on disable/enable, so the tree is rebuilt on every enable.
        private void Rebuild()
        {
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            if (model == null || docRoot == null) return;
            docRoot.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("HubCommon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("ButtonWithIcon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("PostLevelStatsView"));
            UiResources.LoadTree("PostLevelStatsView").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Paper = docRoot.Q("Paper");
            Stamp = docRoot.Q("Stamp");
            Fade = docRoot.Q("Fade");
            DeliveryRow = docRoot.Q("DeliveryRow");
            TimeRow = docRoot.Q("TimeRow");
            SummaryRow = docRoot.Q("SummaryRow");
            Punches = docRoot.Q("Punches");
            ContinueButton = docRoot.Q("ContinueButton");
            StageTitle = docRoot.Q<Label>("StageTitle");
            Summary = docRoot.Q<Label>("Summary");
            SummaryDescription = docRoot.Q<Label>("SummaryDescription");

            // Punch holes: x 7, y 20 + 25 i.
            for (int i = 0; i < PunchCount; i++)
            {
                var hole = new VisualElement();
                hole.AddToClassList("plv-punch");
                hole.style.top = 20 + 25 * i;
                Punches.Add(hole);
            }

            var rewards = model.Rewards;
            StageTitle.text = stageTitle ?? "";
            SetRow("Delivery", "01", "Delivery Reward", "Coal shipment delivered", LevelRewardBreakdown.FormatSignedAmount(rewards.DeliveryReward));
            SetRow("Time", "02", rewards.TimeLineLabel, rewards.TimeLineDescription, LevelRewardBreakdown.FormatSignedAmount(rewards.TimeAdjustment));
            ButtonWithIconElement.Mount(ContinueButton, false);
            ButtonWithIconElement.Set(ContinueButton, ButtonIcon.A, "Continue");
            Refresh();
        }

        private void SetRow(string prefix, string index, string label, string description, string amount)
        {
            Root.Q<Label>(prefix + "Index").text = index;
            Root.Q<Label>(prefix + "Label").text = label;
            Root.Q<Label>(prefix + "Description").text = description;
            Root.Q<Label>(prefix + "Amount").text = amount;
        }

        public void Refresh()
        {
            if (Root == null || model == null) return;
            DeliveryRow.style.display = model.DeliveryVisible ? DisplayStyle.Flex : DisplayStyle.None;
            TimeRow.style.display = model.TimeVisible ? DisplayStyle.Flex : DisplayStyle.None;
            SummaryRow.style.display = model.SummaryVisible ? DisplayStyle.Flex : DisplayStyle.None;
            Summary.text = model.SummaryText;
            StampView.Apply(Stamp, model.Stamp, 10f);
            Fade.style.opacity = model.FadeOpacity;
        }

        private void OnEnable() => Rebuild();
    }
}
