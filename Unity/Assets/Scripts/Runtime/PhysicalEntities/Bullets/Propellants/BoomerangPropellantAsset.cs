using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Propellants
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Propellants/Boomerang Propellant", fileName = "BoomerangPropellant")]
    public class BoomerangPropellantAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Propellant;
        public override string ComponentId => ComponentIds.BoomerangPropellant;

        // Src: velocity -= Stats.Direction * (Speed / 2) * dt. AimDirection is Stats.Direction.
        public override void OnUpdate(BulletRuntime bullet, float deltaTime)
        {
            float returnAcceleration = bullet.Stats.Speed / 2f;
            bullet.PhysicsBody.linearVelocity -= bullet.AimDirection * WorldUnits.ToMeters(returnAcceleration * deltaTime);
        }
    }
}
