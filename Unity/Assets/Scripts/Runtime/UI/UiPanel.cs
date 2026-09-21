using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Builds the PanelSettings shared by menu documents. Scale with screen size at 1920x1080,
    /// shrink match: uniform scale min(w/1920, h/1080), same as Gum.
    /// </summary>
    public static class UiPanel
    {
        public static readonly Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);

        public static PanelSettings Create()
        {
            var ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = ReferenceResolution;
            ps.screenMatchMode = PanelScreenMatchMode.Shrink;
            ps.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/RuntimeTheme");
            return ps;
        }
    }
}
