using UnityEngine;
using UnityEngine.UIElements;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Applies a StampRevealTimer to a stamp element. Gum resizes the sprite (Width and Height times the
    /// scale factor) and rotates it about its top-left origin. Scaling about a top-left transform origin
    /// gives the same picture. Gum rotation is counter-clockwise, USS rotate is clockwise, so the angle is negated.
    /// </summary>
    public static class StampView
    {
        public static void Apply(VisualElement el, StampRevealTimer timer, float baseRotationDegGum)
        {
            el.style.display = timer.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            el.style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(0));
            el.style.scale = new Scale(new Vector3(timer.ScaleFactor, timer.ScaleFactor, 1f));
            el.style.rotate = new Rotate(new Angle(-(baseRotationDegGum + timer.RotationOffset), AngleUnit.Degree));
        }
    }
}
