using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Propellants;
using Gamelab.PhysicalEntities.Bullets.Projectiles;

namespace Gamelab.Tests.Gate
{
    // Wave A verification gate (docs/superpowers/plans/2026-09-18-unity-port-plan.md,
    // "Wave A verification gate", A2 row): a catalog-driven bullet (casing+propellant+
    // projectile combination) and one enemy spawn correctly from data.
    public class CatalogDrivenSpawnSmokeTest
    {
        [UnityTest]
        public IEnumerator CatalogDrivenBulletAndEnemy_BothSpawnFromData()
        {
            // --- Bullet: casing + propellant + projectile combination from a catalog asset ---
            var definition = ScriptableObject.CreateInstance<BulletDefinitionAsset>();
            SetPrivate(definition, "casing", ScriptableObject.CreateInstance<BasicCasingAsset>());
            SetPrivate(definition, "propellant", ScriptableObject.CreateInstance<BasicPropellantAsset>());
            SetPrivate(definition, "projectile", ScriptableObject.CreateInstance<BasicProjectileAsset>());
            SetPrivate(definition, "spread", 0f);

            BulletRuntime bullet = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);

            // --- Enemy: spawned from a catalog entry ---
            var catalog = ScriptableObject.CreateInstance<EnemyCatalogAsset>();
            var entry = new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f };
            SetPrivate(catalog, "entries", new List<EnemyCatalogEntry> { entry });

            var enemyGo = new GameObject("DummyEnemy");
            enemyGo.transform.position = new Vector2(0f, 50f); // away from the bullet so real physics does not hit it
            enemyGo.AddComponent<Rigidbody2D>();
            var enemy = enemyGo.AddComponent<DummyEnemyRuntime>();
            enemy.Initialize(catalog.Get(EnemyType.Dummy));

            yield return null;

            Assert.IsNotNull(bullet);
            Assert.AreEqual(800f, bullet.Stats.Speed, "Bullet stats should come from the catalog definition.");
            Assert.Greater(bullet.PhysicsBody.linearVelocity.x, 0f, "Propellant should apply velocity on spawn.");

            Assert.IsNotNull(enemy);
            Assert.AreEqual(100f, enemy.Health, "Enemy health should come from the catalog entry.");
            Assert.IsTrue(enemy.IsAlive);

            bool tookDamage = enemy.OnHit(bullet);
            Assert.IsTrue(tookDamage, "The catalog-spawned player bullet should be able to damage the catalog-spawned enemy.");
            Assert.AreEqual(50f, enemy.Health, "Damage should come from the bullet's catalog-driven stats.");

            Object.Destroy(bullet.gameObject);
            Object.Destroy(enemyGo);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }
    }
}
