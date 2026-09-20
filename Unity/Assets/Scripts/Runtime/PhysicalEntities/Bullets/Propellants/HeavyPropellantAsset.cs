using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Propellants
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Propellants/Heavy Propellant", fileName = "HeavyPropellant")]
    public class HeavyPropellantAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Propellant;
        public override string ComponentId => ComponentIds.HeavyPropellant;

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Damage *= 2.0f;
            bullet.Stats.Speed *= 0.7f;
            bullet.Stats.Size *= 1.5f;
        }
    }
}
