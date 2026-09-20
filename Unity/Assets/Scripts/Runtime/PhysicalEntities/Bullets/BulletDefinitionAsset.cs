using System;
using System.Collections.Generic;
using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    // The catalog-driven casing+propellant+projectile combination the Wave A gate asks for.
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Bullet Definition", fileName = "BulletDefinition")]
    public class BulletDefinitionAsset : ScriptableObject
    {
        [SerializeField] private List<BulletComponentAsset> components = new List<BulletComponentAsset>();
        [SerializeField] private float speed = 800f;
        [SerializeField] private float damage = 50f;
        [SerializeField] private float pierce = 1f;
        [SerializeField] private float size = 12f;
        [SerializeField] private float spread = 0.2f;
        [SerializeField] private float lifetime = 5f;

        public IReadOnlyList<BulletComponentAsset> Components => components;

        public void Validate()
        {
            if (components == null || components.Count == 0)
            {
                throw new InvalidOperationException($"BulletDefinition '{name}': component list is empty.");
            }

            // Per-bullet state and root flags are keyed by asset, so one asset cannot appear twice.
            var seen = new HashSet<BulletComponentAsset>();
            var types = new HashSet<EComponentType>();
            for (int i = 0; i < components.Count; i++)
            {
                BulletComponentAsset c = components[i];
                if (c == null)
                {
                    throw new InvalidOperationException($"BulletDefinition '{name}': component at index {i} is null.");
                }

                if (!seen.Add(c))
                {
                    throw new InvalidOperationException(
                        $"BulletDefinition '{name}': component '{c.name}' appears twice (duplicate assets are not allowed).");
                }

                types.Add(c.Type);
            }

            foreach (EComponentType required in new[] { EComponentType.Casing, EComponentType.Propellant, EComponentType.Projectile })
            {
                if (!types.Contains(required))
                {
                    throw new InvalidOperationException($"BulletDefinition '{name}': missing a {required} component.");
                }
            }
        }

        public void Configure(IEnumerable<BulletComponentAsset> newComponents, BulletStats stats)
        {
            components = new List<BulletComponentAsset>(newComponents);
            speed = stats.Speed;
            damage = stats.Damage;
            pierce = stats.Pierce;
            size = stats.Size;
            spread = stats.Spread;
            lifetime = stats.Lifetime;
        }

        /// <summary>Runtime definition with the recipe's components in recipe order. It must outlive every bullet spawned from it and their delayed children; destroy it only after those are gone.</summary>
        public static BulletDefinitionAsset BuildFromRecipe(BulletRecipe recipe, BulletComponentCatalogAsset catalog, BulletStats baseStats)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var list = new List<BulletComponentAsset>();
            foreach (string id in recipe.ComponentIds) list.Add(catalog.Get(id));
            var definition = CreateInstance<BulletDefinitionAsset>();
            definition.Configure(list, baseStats);
            return definition;
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
