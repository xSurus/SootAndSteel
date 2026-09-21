using UnityEngine;

namespace Gamelab.UI.Runtime
{
    /// <summary>World pixel position (Src frame, Y down) to a screen point in pixels, origin top left.</summary>
    public interface IWorldToScreen
    {
        Vector2 ToScreen(Vector2 worldPx);
        /// <summary>Screen size in pixels the returned points refer to.</summary>
        Vector2 ScreenSize { get; }
    }

    /// <summary>
    /// Camera.WorldToScreenPoint on world meters (px / 100). Unity screen Y is bottom up, so it is flipped.
    /// The Y-mirrored camera already puts larger world Y lower on screen. Only checked by a PlayMode test
    /// against MirroredCamera, not against a real scene camera or a non-16:9 window.
    /// </summary>
    public sealed class CameraWorldToScreen : IWorldToScreen
    {
        private readonly Camera cam;

        public CameraWorldToScreen(Camera camera)
        {
            cam = camera;
        }

        public Vector2 ScreenSize => new Vector2(cam.pixelWidth, cam.pixelHeight);

        public Vector2 ToScreen(Vector2 worldPx)
        {
            Vector3 p = cam.WorldToScreenPoint(new Vector3(WorldUnits.ToMeters(worldPx.x), WorldUnits.ToMeters(worldPx.y), 0f));
            return new Vector2(p.x, cam.pixelHeight - p.y);
        }
    }
}
