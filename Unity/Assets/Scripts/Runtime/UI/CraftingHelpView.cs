using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Crafting recipe help panel at the top left. Visible follows CraftingHelpModel.Visible.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class CraftingHelpView : MonoBehaviour
    {
        private const string DescriptionText =
            "A bullet needs three parts: a projectile, a casing, and a propellant. Basics are on the train: the shop sells upgraded parts.\n\n" +
            "Combine a basic component with an upgrade of that type to create an upgraded component. " +
            "To craft a finished bullet, place one of each of the three types (Casing, Projectile, Propellant) in their basic or upgraded form and hold to interact";

        private CraftingHelpModel model;

        public VisualElement Root { get; private set; }

        public void Bind(CraftingHelpModel help, PanelSettings panel)
        {
            Unsubscribe();
            GetComponent<UIDocument>().panelSettings = panel;
            model = help;
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
            docRoot.styleSheets.Add(UiResources.LoadStyle("CraftingHelp"));
            UiResources.LoadTree("CraftingHelp").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            docRoot.Q<Label>("Description").text = DescriptionText;
            var toggle = docRoot.Q("ToggleButton");
            ButtonWithIconElement.Mount(toggle, false);
            ButtonWithIconElement.Set(toggle, ButtonIcon.None, "Toggle");
            toggle.Q("Icon").AddToClassList("bwi-icon--select");
            model.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            if (model == null || Root == null) return;
            Root.style.display = model.Visible ? DisplayStyle.Flex : DisplayStyle.None;
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
