using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    // Resolves recipe component ids to the shared component assets.
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Component Catalog", fileName = "BulletComponentCatalog")]
    public class BulletComponentCatalogAsset : ScriptableObject
    {
        [SerializeField] private List<BulletComponentAsset> components = new List<BulletComponentAsset>();

        public IReadOnlyList<BulletComponentAsset> Components => components;

        public void Configure(IEnumerable<BulletComponentAsset> assets)
        {
            components = new List<BulletComponentAsset>(assets);
        }

        public BulletComponentAsset Get(string componentId)
        {
            foreach (BulletComponentAsset c in components)
            {
                if (c != null && c.ComponentId == componentId) return c;
            }

            throw new KeyNotFoundException(
                $"BulletComponentCatalog '{name}': no component with id '{componentId}'.");
        }
    }
}
