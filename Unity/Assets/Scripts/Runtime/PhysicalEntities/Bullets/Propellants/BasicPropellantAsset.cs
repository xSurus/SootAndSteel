using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Propellants
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Propellants/Basic Propellant", fileName = "BasicPropellant")]
    public class BasicPropellantAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Propellant;
        public override string ComponentId => ComponentIds.BasicPropellant;

        private static readonly IRandomService RandomService = Gamelab.Services.Random.RandomService.Shared;

        public override void OnSpawn(BulletRuntime bullet)
        {
            // Mirrors Src/PhysicalEntities/Bullets/Components/Propellants/BasicPropellant.cs
            // OnSpawn: sample a gaussian spread angle, rotate the aim direction by it, then
            // set the Rigidbody2D velocity. bullet.Stats.Spread / 3f matches the original's
            // spread-to-standard-deviation scaling.
            float randomSpread = (float)RandomService.SampleGaussian(0, bullet.Stats.Spread / 3f);
            Vector2 direction = RotateVector(bullet.AimDirection, randomSpread);
            bullet.PhysicsBody.linearVelocity = direction * WorldUnits.ToMeters(bullet.Stats.Speed); // stats stay in pixels (ruling 1)
        }

        private static Vector2 RotateVector(Vector2 v, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
