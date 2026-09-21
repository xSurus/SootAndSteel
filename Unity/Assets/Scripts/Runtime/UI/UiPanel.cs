using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Builds the PanelSettings shared by menu documents. Scale with screen size at 1920x1080 with the
    /// Shrink match mode. Measured: Unity resolves Shrink to the larger of the two axis scales,
    /// max(w/1920, h/1080), so a 640x480 panel has scale 0.444 and a root 1440 wide. Gum letterboxes with
    /// the smaller one. Use CanvasScale instead of recomputing it.
    /// </summary>
    public static class UiPanel
    {
        public static readonly Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);

        /// <summary>The uniform canvas scale a Create() panel really uses at this screen size (measured, see the class comment).</summary>
        public static float CanvasScale(Vector2 screenSize) =>
            Mathf.Max(screenSize.x / ReferenceResolution.x, screenSize.y / ReferenceResolution.y);

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
