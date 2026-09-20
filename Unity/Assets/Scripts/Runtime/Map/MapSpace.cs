using UnityEngine;

namespace Gamelab.Map
{
    /// <summary>
    /// The only place Y is flipped. The physics world stays in Src's pixel frame (Y down), in meters
    /// (px / 100). To show that world Y-down on screen the camera projection is mirrored in Y
    /// (<see cref="ApplyTo(Camera)"/>). The mirror also turns sprite art upside down, and
    /// <see cref="SpriteFlip"/> (tiles here, SpriteRenderer.flipY via <see cref="ApplyToSprite"/>) cancels
    /// that so art shows upright. No position or velocity is ever negated.
    /// </summary>
    public static class MapSpace
    {
        public static readonly Matrix4x4 SpriteFlip = Matrix4x4.Scale(new Vector3(1f, -1f, 1f));

        public static Vector2 ToUnity(System.Numerics.Vector2 v) => new Vector2(v.X, v.Y);

        public static System.Numerics.Vector2 ToNumerics(Vector2 v) => new System.Numerics.Vector2(v.x, v.y);

        public static float PxToMeters(float px) => WorldUnits.ToMeters(px);

        public static Vector2 PxToMeters(Vector2 px) => ToUnity(WorldUnits.ToMeters(ToNumerics(px)));

        /// <summary>
        /// Transform scale that makes a sprite with the given PPU as wide as Src draws it: a sprite of
        /// native width W px at Src scale s is s*W px = s*W/100 units, and it is W/PPU units at scale 1.
        /// </summary>
        public static float SpriteScale(float pixelsPerUnit, float srcScale) => srcScale * pixelsPerUnit / 100f;

        public static void ApplyTo(Camera cam)
        {
            cam.ResetProjectionMatrix();
            cam.projectionMatrix *= Matrix4x4.Scale(new Vector3(1f, -1f, 1f));
        }

        public static void ApplyToSprite(SpriteRenderer r) => r.flipY = true;
    }
}
