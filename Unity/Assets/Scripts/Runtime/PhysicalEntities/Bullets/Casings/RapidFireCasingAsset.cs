using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Casings
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Casings/Rapid Fire Casing", fileName = "RapidFireCasing")]
    public class RapidFireCasingAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Casing;
        public override string ComponentId => ComponentIds.RapidFireCasing;

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Spread *= 5.0f;
            bullet.Stats.Damage *= 0.25f;
        }

        // Src plays a sound on staggered children here. Sound is a later wave.
        public override void OnSpawn(BulletRuntime bullet)
        {
            if (!bullet.IsRoot(this)) return;
            for (int i = 0; i < 10; i++)
            {
                bullet.SpawnChild(this, bullet.transform.position, bullet.AimDirection, i * 0.1f);
            }

            bullet.Deactivate();
        }
    }
}
