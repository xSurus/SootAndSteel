using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Casings
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Casings/Scatter Casing", fileName = "ScatterCasing")]
    public class ScatterCasingAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Casing;
        public override string ComponentId => ComponentIds.ScatterCasing;

        private static readonly IRandomService RandomService = new RandomService();

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Spread *= 3.0f;
            bullet.Stats.Damage *= 0.2f;
            bullet.Stats.Speed += (RandomService.NextSingle() * 0.4f - 0.2f) * bullet.Stats.Speed;
        }

        public override void OnSpawn(BulletRuntime bullet)
        {
            if (!bullet.IsRoot(this)) return;
            for (int i = 0; i < 10; i++)
            {
                bullet.SpawnChild(this, bullet.transform.position, bullet.AimDirection);
            }

            bullet.Deactivate();
        }
    }
}
