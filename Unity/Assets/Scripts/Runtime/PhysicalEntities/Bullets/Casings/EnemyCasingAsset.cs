using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Casings
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Casings/Enemy Casing", fileName = "EnemyCasing")]
    public class EnemyCasingAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Casing;
        public override string ComponentId => ComponentIds.EnemyCasing;

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Damage = 10f;
        }
    }
}
