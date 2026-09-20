using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Casings
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Casings/Burst Casing", fileName = "BurstCasing")]
    public class BurstCasingAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Casing;
        public override string ComponentId => ComponentIds.BurstCasing;

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Damage *= 0.5f;
        }

        // Src plays a sound on staggered children here. Sound is a later wave.
        public override void OnSpawn(BulletRuntime bullet)
        {
            if (!bullet.IsRoot(this)) return;
            for (int i = 0; i < 3; i++)
            {
                bullet.SpawnChild(this, bullet.transform.position, bullet.AimDirection, i * 0.15f);
            }

            bullet.Deactivate();
        }
    }
}
