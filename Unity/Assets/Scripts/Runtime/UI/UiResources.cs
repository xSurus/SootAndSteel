using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>Loads UXML and USS from Assets/Resources/UI/.</summary>
    public static class UiResources
    {
        public static VisualTreeAsset LoadTree(string name) => Resources.Load<VisualTreeAsset>("UI/" + name);

        public static StyleSheet LoadStyle(string name) => Resources.Load<StyleSheet>("UI/" + name);
    }
}
