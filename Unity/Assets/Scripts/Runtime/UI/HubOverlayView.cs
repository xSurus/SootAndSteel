using Gamelab.Run;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Credits with coin (top right) and the players-ready row (bottom right).</summary>
    [RequireComponent(typeof(UIDocument))]
    public class HubOverlayView : MonoBehaviour
    {
        private RunCredits credits;
        private HubDepartureModel departure;

        public VisualElement Root { get; private set; }
        public Label Amount { get; private set; }
        public Label ReadyLabel { get; private set; }
        /// <summary>Icons by colour slot (joined order): blue, red, brown, yellow.</summary>
        public VisualElement[] ReadyIcons { get; } = new VisualElement[4];

        public void Bind(RunCredits credits, HubDepartureModel departure, PanelSettings panel)
        {
            Unsubscribe();
            GetComponent<UIDocument>().panelSettings = panel;
            this.credits = credits;
            this.departure = departure;
            Rebuild();
        }

        // UIDocument recreates rootVisualElement on disable/enable, so the tree is rebuilt on every enable.
        private void Rebuild()
        {
            Unsubscribe();
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            if (credits == null || departure == null || docRoot == null) return;
            docRoot.Clear();
            docRoot.styleSheets.Add(UiResources.LoadStyle("Common"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("HubCommon"));
            docRoot.styleSheets.Add(UiResources.LoadStyle("HubOverlay"));
            UiResources.LoadTree("HubOverlay").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Amount = docRoot.Q<Label>("Amount");
            ReadyLabel = docRoot.Q<Label>("ReadyLabel");
            // Src HubOverlay.SyncReadySprites maps colour slots to blue, red, brown, yellow.
            ReadyIcons[0] = docRoot.Q("BluePlayer");
            ReadyIcons[1] = docRoot.Q("RedPlayer");
            ReadyIcons[2] = docRoot.Q("BrownPlayer");
            ReadyIcons[3] = docRoot.Q("YellowPlayer");
            credits.Changed += Refresh;
            departure.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            if (Root == null || credits == null) return;
            Amount.text = credits.Credits.ToString();
            bool all = departure.AllReady;
            ReadyLabel.text = all ? "Ready for departure!" : "Players ready:";
            ReadyLabel.EnableInClassList("ho-ready-label--all", all);
            var slots = departure.ReadySlots;
            for (int i = 0; i < ReadyIcons.Length; i++)
                ReadyIcons[i].EnableInClassList("ho-player--shown", slots[i]);
        }

        private void Unsubscribe()
        {
            if (credits != null) credits.Changed -= Refresh;
            if (departure != null) departure.Changed -= Refresh;
        }

        private void OnEnable() => Rebuild();
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
    }
}
