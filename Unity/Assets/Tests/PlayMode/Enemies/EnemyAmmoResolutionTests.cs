using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Propellants;

namespace Gamelab.Tests.Enemies
{
    public class EnemyAmmoResolutionTests
    {
        private readonly List<Object> created = new List<Object>();

        private BulletComponentCatalogAsset MakeCatalog()
        {
            BulletComponentAsset[] assets =
            {
                Make<BasicProjectileAsset>(), Make<FrangibleProjectileAsset>(), Make<PiercingProjectileAsset>(),
                Make<MatryoshkaProjectileAsset>(), Make<BasicCasingAsset>(), Make<ScatterCasingAsset>(),
                Make<BurstCasingAsset>(), Make<EnemyCasingAsset>(), Make<RapidFireCasingAsset>(),
                Make<BasicPropellantAsset>(), Make<HomingPropellantAsset>(), Make<HeavyPropellantAsset>(),
                Make<BoomerangPropellantAsset>()
            };
            var catalog = Make<BulletComponentCatalogAsset>();
            catalog.Configure(assets);
            return catalog;
        }

        private T Make<T>() where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            created.Add(o);
            return o;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go.name.StartsWith("Bullet_")) Object.Destroy(go);
            foreach (var o in created) Object.Destroy(o);
            created.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllNineRecipes_ResolveValidateAndSpawn()
        {
            var catalog = MakeCatalog();
            Assert.AreEqual(13, catalog.Components.Count);
            foreach (EnemyAmmoDefinition ammo in EnemyAmmoCatalog.All)
            {
                var def = BulletDefinitionAsset.BuildFromRecipe(ammo.BuildRecipe(), catalog, BulletStats.CannonDefault());
                created.Add(def);
                CollectionAssert.AreEqual(ammo.OrderedComponentIds, System.Linq.Enumerable.Select(def.Components, c => c.ComponentId), ammo.Id);
                Assert.DoesNotThrow(def.Validate, ammo.Id);
                Assert.DoesNotThrow(() => BulletRuntime.Spawn(def, Vector2.zero, Vector2.right, BulletFaction.Enemy), ammo.Id);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator BasicRecipe_SpawnsWithEnemyDamage()
        {
            var def = BulletDefinitionAsset.BuildFromRecipe(
                EnemyAmmoCatalog.GetAmmoDefinition(EnemyAmmoIds.Basic).BuildRecipe(), MakeCatalog(), BulletStats.CannonDefault());
            created.Add(def);
            var b = BulletRuntime.Spawn(def, Vector2.zero, Vector2.right, BulletFaction.Enemy);
            Assert.AreEqual(10f, b.Stats.Damage);
            yield return null;
        }

        [Test]
        public void UnknownComponentId_ThrowsClearly()
        {
            var ex = Assert.Throws<KeyNotFoundException>(() => MakeCatalog().Get("Nope"));
            StringAssert.Contains("Nope", ex.Message);
        }
    }
}
