using UnityEngine;

namespace Gamelab.Map
{
    /// <summary>Keeps the Y mirror on a camera by re-applying it every LateUpdate.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class MirroredCamera : MonoBehaviour
    {
        private Camera cam;

        private void LateUpdate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            MapSpace.ApplyTo(cam);
        }
    }
}
