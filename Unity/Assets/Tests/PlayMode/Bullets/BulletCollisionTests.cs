using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Bullets
{
    // Real Unity physics only: nothing here calls OnHit or TakeDamage.
    // Tunnelling limit: 8 m/s * 1/60 s (fixed timestep 0.0167) = about 0.133 m per physics step vs enemy trigger radius 0.36 m.
    public class BulletCollisionTests
    {
        private static DummyEnemyRuntime SpawnEnemy(Vector2 position)
        {
            var go = new GameObject("Enemy_Test");
            go.transform.position = position;
            go.AddComponent<Rigidbody2D>();
            var enemy = go.AddComponent<DummyEnemyRuntime>();
            enemy.Initialize(new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f });
            return enemy;
        }

        private static BulletRuntime Fire(BulletFaction faction, float pierce = 1f)
        {
            var definition = BulletTestUtil.MakeBasicDefinition(damage: 50f);
            BulletTestUtil.SetPrivate(definition, "pierce", pierce);
            return BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, faction);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.StartsWith("Bullet_") || go.name == "Enemy_Test") Object.Destroy(go);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerBullet_HitsEnemy_DamagesAndConsumesBullet()
        {
            var enemy = SpawnEnemy(new Vector2(2f, 0f));
            var bullet = Fire(BulletFaction.Player);
            yield return new WaitForSeconds(1f);

            Assert.AreEqual(50f, enemy.Health);
            Assert.IsTrue(bullet == null);
        }

        [UnityTest]
        public IEnumerator EnemyBullet_DoesNotDamageEnemyOrGetConsumed()
        {
            var enemy = SpawnEnemy(new Vector2(2f, 0f));
            var bullet = Fire(BulletFaction.Enemy);
            yield return new WaitForSeconds(1f);

            Assert.AreEqual(100f, enemy.Health);
            Assert.IsTrue(bullet != null);
        }

        [UnityTest]
        public IEnumerator Pierce2Bullet_DamagesBothEnemiesInARowOnce()
        {
            var first = SpawnEnemy(new Vector2(2f, 0f));
            var second = SpawnEnemy(new Vector2(3f, 0f));
            var bullet = Fire(BulletFaction.Player, pierce: 2f);
            yield return new WaitForSeconds(1.5f);

            Assert.AreEqual(50f, first.Health);
            Assert.AreEqual(50f, second.Health);
            Assert.IsTrue(bullet == null);
        }

        [UnityTest]
        public IEnumerator Bullet_MissesEnemyOffAxis_LeavesItUntouched()
        {
            var enemy = SpawnEnemy(new Vector2(2f, 5f));
            Fire(BulletFaction.Player);
            yield return new WaitForSeconds(1f);

            Assert.AreEqual(100f, enemy.Health);
        }
    }
}
