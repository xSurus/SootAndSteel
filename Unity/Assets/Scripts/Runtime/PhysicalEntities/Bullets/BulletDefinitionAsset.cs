using System;
using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    // The catalog-driven casing+propellant+projectile combination the Wave A gate asks for.
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Bullet Definition", fileName = "BulletDefinition")]
    public class BulletDefinitionAsset : ScriptableObject
    {
        [SerializeField] private BulletComponentAsset casing;
        [SerializeField] private BulletComponentAsset propellant;
        [SerializeField] private BulletComponentAsset projectile;
        [SerializeField] private float speed = 800f;
        [SerializeField] private float damage = 50f;
        [SerializeField] private float pierce = 1f;
        [SerializeField] private float size = 12f;
        [SerializeField] private float spread = 0.2f;
        [SerializeField] private float lifetime = 5f;

        public BulletComponentAsset Casing => casing;
        public BulletComponentAsset Propellant => propellant;
        public BulletComponentAsset Projectile => projectile;

        public void Validate()
        {
            Check(casing, EComponentType.Casing, "casing");
            Check(propellant, EComponentType.Propellant, "propellant");
            Check(projectile, EComponentType.Projectile, "projectile");
        }

        private void Check(BulletComponentAsset component, EComponentType expected, string slot)
        {
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"BulletDefinition '{name}': {slot} slot is empty.");
            }

            if (component.Type != expected)
            {
                throw new InvalidOperationException(
                    $"BulletDefinition '{name}': {slot} slot type mismatch, expected {expected} but '{component.name}' is {component.Type}.");
            }
        }

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
