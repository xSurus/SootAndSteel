using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Casings
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Casings/Basic Casing", fileName = "BasicCasing")]
    public class BasicCasingAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Casing;
        public override string ComponentId => ComponentIds.BasicCasing;

        // ponytail: Src/.../Components/Casings/BasicCasing.cs spawns a particle trail
        // emitter here via IVfxService, which isn't built in Unity yet (Scope decisions #4).
        // No behavior needed for the vertical slice beyond identifying as a casing.
    }
}
