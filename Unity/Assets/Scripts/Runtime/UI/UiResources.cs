using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Loads UXML and USS from Assets/Resources/UI/.</summary>
    public static class UiResources
    {
        public static VisualTreeAsset LoadTree(string name) => Resources.Load<VisualTreeAsset>("UI/" + name);

        /// <summary>
        /// A UXML that has inline styles exposes a StyleSheet named "inlineStyle" at the same Resources path
        /// as the USS of the same name, and Resources.Load returns either one depending on import order.
        /// LoadAll and a name match pick the USS.
        /// </summary>
        public static StyleSheet LoadStyle(string name)
        {
            foreach (var sheet in Resources.LoadAll<StyleSheet>("UI/" + name))
                if (sheet.name == name) return sheet;
            return null;
        }
    }
}
