using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Projectiles
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Projectiles/Basic Projectile", fileName = "BasicProjectile")]
    public class BasicProjectileAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Projectile;
        public override string ComponentId => ComponentIds.BasicProjectile;
    }
}
