using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Enemies
{
    public class EnemyRuntimeSpawnTests
    {
        [UnityTest]
        public IEnumerator Spawn_FromCatalogEntry_AppliesHealthAndTakesDamageFromPlayerBullet()
        {
            var catalog = ScriptableObject.CreateInstance<EnemyCatalogAsset>();
            var entry = new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f };
            typeof(EnemyCatalogAsset).GetField("entries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(catalog, new System.Collections.Generic.List<EnemyCatalogEntry> { entry });

            var go = new GameObject("DummyEnemy");
            go.AddComponent<Rigidbody2D>();
            var enemy = go.AddComponent<DummyEnemyRuntime>();
            enemy.Initialize(catalog.Get(EnemyType.Dummy));

            yield return null;

            Assert.AreEqual(100f, enemy.Health);
            Assert.IsTrue(enemy.IsAlive);

            var bulletGo = new GameObject("FakeBullet");
            bulletGo.AddComponent<Rigidbody2D>();
            var bullet = bulletGo.AddComponent<BulletRuntime>();
            // Reflection is used here only to arrange a BulletRuntime with a known Faction/
            // Stats for the test, since BulletRuntime's real constructor path is Spawn()
            // (Task 5), which needs a BulletDefinitionAsset this test doesn't need.
            typeof(BulletRuntime).GetProperty("Faction").SetValue(bullet, BulletFaction.Player);
            typeof(BulletRuntime).GetProperty("Stats").SetValue(bullet, new BulletStats { Damage = 30f });

            bool hit = enemy.OnHit(bullet);

            Assert.IsTrue(hit);
            Assert.AreEqual(70f, enemy.Health);

            Object.Destroy(go);
            Object.Destroy(bulletGo);
        }
    }
}
