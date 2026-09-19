using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Propellants;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    // The catalog-driven casing+propellant+projectile combination the Wave A gate asks for.
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Bullet Definition", fileName = "BulletDefinition")]
    public class BulletDefinitionAsset : ScriptableObject
    {
        [SerializeField] private BasicCasingAsset casing;
        [SerializeField] private BasicPropellantAsset propellant;
        [SerializeField] private BasicProjectileAsset projectile;
        [SerializeField] private float speed = 800f;
        [SerializeField] private float damage = 50f;
        [SerializeField] private float pierce = 1f;
        [SerializeField] private float size = 12f;
        [SerializeField] private float spread = 0.2f;
        [SerializeField] private float lifetime = 5f;

        public BasicCasingAsset Casing => casing;
        public BasicPropellantAsset Propellant => propellant;
        public BasicProjectileAsset Projectile => projectile;

        public BulletStats ToStats()
        {
            return new BulletStats
            {
                Speed = speed,
                Damage = damage,
                Pierce = pierce,
                Size = size,
                Spread = spread,
                Lifetime = lifetime
            };
        }
    }
}
