using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Gum ButtonWithIcon as a cloned UXML (icon and label) inside a slot element.</summary>
    internal static class ButtonWithIconElement
    {
        public static void Mount(VisualElement slot, bool compact)
        {
            UiResources.LoadTree("ButtonWithIcon").CloneTree(slot);
            slot.Q("Button").EnableInClassList("bwi--compact", compact);
        }

        public static void Set(VisualElement slot, ButtonIcon icon, string label, bool canAfford = true)
        {
            var iconElement = slot.Q("Icon");
            iconElement.EnableInClassList("bwi-icon--coin", icon == ButtonIcon.Coin);
            iconElement.style.backgroundImage = StyleKeyword.Null;
            switch (icon)
            {
                case ButtonIcon.A: iconElement.style.backgroundImage = new StyleBackground(UiSpriteCrop.Glyph(XboxButtonAtlas.Face.A)); break;
                case ButtonIcon.X: iconElement.style.backgroundImage = new StyleBackground(UiSpriteCrop.Glyph(XboxButtonAtlas.Face.X)); break;
                case ButtonIcon.Y: iconElement.style.backgroundImage = new StyleBackground(UiSpriteCrop.Glyph(XboxButtonAtlas.Face.Y)); break;
            }
            slot.Q<Label>("Text").text = label ?? "";
            slot.Q("Button").EnableInClassList("bwi--cant-afford", !canAfford);
        }
    }
}
