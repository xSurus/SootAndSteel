using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    // Src BulletService.EmitBullet builds stats from GameplayConfig (= BulletStats.CannonDefault)
    // and the position/direction, then runs create + spawn. Definitions are built lazily per
    // ordered component-id list and owned here. Dispose only after the bullets are gone
    // (see BulletDefinitionAsset.BuildFromRecipe).
    public sealed class CatalogBulletSpawner : IBulletItemSpawner, IDisposable
    {
        private readonly BulletComponentCatalogAsset catalog;
        private readonly Dictionary<string, BulletDefinitionAsset> cache = new Dictionary<string, BulletDefinitionAsset>();

        public CatalogBulletSpawner(BulletComponentCatalogAsset catalog)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public BulletRuntime Emit(BulletRecipe recipe, Vector2 positionMeters, Vector2 direction, BulletFaction faction)
        {
            string key = string.Join(",", recipe.ComponentIds);
            if (!cache.TryGetValue(key, out BulletDefinitionAsset def))
            {
                def = BulletDefinitionAsset.BuildFromRecipe(recipe, catalog, BulletStats.CannonDefault());
                cache[key] = def;
            }

            return BulletRuntime.Spawn(def, positionMeters, direction, faction);
        }

        public void Dispose()
        {
            foreach (BulletDefinitionAsset def in cache.Values)
            {
                if (def != null) UnityEngine.Object.Destroy(def);
            }

            cache.Clear();
        }
    }
}
