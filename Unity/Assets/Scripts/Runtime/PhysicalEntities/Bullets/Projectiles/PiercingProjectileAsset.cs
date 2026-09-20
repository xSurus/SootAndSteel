using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Projectiles
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Projectiles/Piercing Projectile", fileName = "PiercingProjectile")]
    public class PiercingProjectileAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Projectile;
        public override string ComponentId => ComponentIds.PiercingProjectile;

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Pierce += 3f;
            bullet.Stats.Speed *= 1.5f;
        }
    }
}
