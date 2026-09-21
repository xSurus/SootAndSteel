using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// One world tooltip panel. Static content comes from the TooltipModel, the left button colour follows
    /// LeftCanAfford. Root is absolutely positioned (left and top) by the caller.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ToolTipView : MonoBehaviour
    {
        private TooltipModel model;

        public VisualElement Root { get; private set; }
        public Label Title { get; private set; }
        public Label Functionality { get; private set; }
        public Label Description { get; private set; }
        public VisualElement Category { get; private set; }
        public VisualElement CategoryIcon { get; private set; }
        public Label CategoryText { get; private set; }
        public VisualElement LeftButton { get; private set; }
        public VisualElement RightButton { get; private set; }

        public void Bind(TooltipModel tooltip, PanelSettings panel)
        {
            Unsubscribe();
            GetComponent<UIDocument>().panelSettings = panel;
            model = tooltip;
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
            docRoot.styleSheets.Add(UiResources.LoadStyle("ToolTip"));
            UiResources.LoadTree("ToolTip").CloneTree(docRoot);
            Root = docRoot.Q("Root");
            Title = docRoot.Q<Label>("Title");
            Functionality = docRoot.Q<Label>("Functionality");
            Description = docRoot.Q<Label>("Description");
            Category = docRoot.Q("Category");
            CategoryIcon = docRoot.Q("CategoryIcon");
            CategoryText = docRoot.Q<Label>("CategoryText");
            LeftButton = docRoot.Q("LeftButton");
            RightButton = docRoot.Q("RightButton");
            ButtonWithIconElement.Mount(LeftButton, true);
            ButtonWithIconElement.Mount(RightButton, true);

            Title.text = model.Title ?? "";
            Functionality.text = model.Functionality ?? "";
            Description.text = model.Description ?? "";
            ApplyCategory();
            model.Changed += Refresh;
            Refresh();
        }

        // Src ApplyCategoryRow: no category hides the row. Casing, Projectile and Propellant use their own art,
        // anything else crops IconSourceRect from the item sheet, and hides the icon when that is missing.
        private void ApplyCategory()
        {
            string name = model.CategoryName;
            Category.style.display = name == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (name == null) return;
            CategoryText.text = name;
            Sprite icon = null;
            if (name == "Casing" || name == "Projectile" || name == "Propellant")
                icon = Resources.Load<Sprite>("UI/Art/Basic" + name);
            else if (model.IconSourceRect is Gamelab.PhysicalEntities.Interfaces.SpriteRect rect)
                icon = UiSpriteCrop.Get(ShopItemIconAtlas.SheetFile, rect);
            CategoryIcon.style.display = icon == null ? DisplayStyle.None : DisplayStyle.Flex;
            CategoryIcon.style.backgroundImage = icon == null ? new StyleBackground() : new StyleBackground(icon);
        }

        public void Refresh()
        {
            if (model == null || Root == null) return;
            var left = model.Left;
            var right = model.Right;
            LeftButton.style.display = left.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            RightButton.style.display = right.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            ButtonWithIconElement.Set(LeftButton, left.Icon, left.Label, model.LeftCanAfford);
            ButtonWithIconElement.Set(RightButton, right.Icon, right.Label);
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
