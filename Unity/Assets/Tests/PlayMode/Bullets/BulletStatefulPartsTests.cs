using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Propellants;

namespace Gamelab.Tests.Bullets
{
    public class BulletStatefulPartsTests
    {
        private static DummyEnemyRuntime MakeEnemy(Vector2 pos, float health = 1000f)
        {
            var go = new GameObject("Enemy_Test");
            go.transform.position = pos;
            var enemy = go.AddComponent<DummyEnemyRuntime>();
            enemy.Initialize(new EnemyCatalogEntry { type = EnemyType.Dummy, health = health, size = 72f });
            return enemy;
        }

        private static BulletRuntime Fire(BulletFaction faction, params BulletComponentAsset[] extra)
        {
            // Basic parts stay (they set the velocity and lifetime), modifiers are added on top like Src.
            var definition = BulletTestUtil.MakeBasicDefinition(0f, 5f, 800f, 50f, extra);
            return BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, faction);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go.name.StartsWith("Bullet_") || go.name == "Enemy_Test") Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Boomerang_VelocityFollowsConstantDecelerationByAge()
        {
            var b = Fire(BulletFaction.Player, ScriptableObject.CreateInstance<BoomerangPropellantAsset>());
            yield return new WaitForFixedUpdate();
            Assert.Greater(b.PhysicsBody.linearVelocity.x, 0f);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (b.Age < 1f && Time.realtimeSinceStartup < deadline) yield return new WaitForFixedUpdate();

            // Src decel = Speed / 2 px/s^2 = 4 m/s^2 from 8 m/s. Read Age and velocity in the same frame.
            Assert.GreaterOrEqual(b.Age, 1f);
            Assert.AreEqual(8f - 4f * b.Age, b.PhysicsBody.linearVelocity.x, 0.3f);
        }

        private static IEnumerator RunUntilEnemyHurt(DummyEnemyRuntime enemy, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (enemy.Health >= 1000f && Time.realtimeSinceStartup < deadline) yield return null;
        }

        [UnityTest]
        public IEnumerator Homing_HitsEnemyOffAxis()
        {
            var enemy = MakeEnemy(new Vector2(8f, 1f));
            Fire(BulletFaction.Player, ScriptableObject.CreateInstance<HomingPropellantAsset>());
            yield return RunUntilEnemyHurt(enemy, 5f);
            Assert.Less(enemy.Health, 1000f);
        }

        [UnityTest]
        public IEnumerator BasicBullet_MissesSameEnemy()
        {
            var enemy = MakeEnemy(new Vector2(8f, 1f));
            Fire(BulletFaction.Player);
            yield return RunUntilEnemyHurt(enemy, 2.5f);
            Assert.AreEqual(1000f, enemy.Health);
        }

        [UnityTest]
        public IEnumerator Homing_EnemyFactionIgnoresEnemies()
        {
            var enemy = MakeEnemy(new Vector2(8f, 1f));
            var b = Fire(BulletFaction.Enemy, ScriptableObject.CreateInstance<HomingPropellantAsset>());
            float deadline = Time.realtimeSinceStartup + 2f;
            while (b != null && Time.realtimeSinceStartup < deadline) yield return new WaitForFixedUpdate();
            if (b != null)
            {
                // Never locked (no speed-up) and never turned toward the enemy.
                Assert.AreEqual(0f, b.PhysicsBody.linearVelocity.y, 1e-4f);
                Assert.AreEqual(4f, b.PhysicsBody.linearVelocity.x, 1e-3f);
            }

            Assert.AreEqual(1000f, enemy.Health);
        }

        [UnityTest]
        public IEnumerator Matryoshka_HitSpawnsLevel2ChildWithScaledStats()
        {
            var matryoshka = ScriptableObject.CreateInstance<MatryoshkaProjectileAsset>();
            var enemy = MakeEnemy(new Vector2(2f, 0f), 100000f);
            var root = Fire(BulletFaction.Player, matryoshka);

            var child = default(BulletRuntime);
            float deadline = Time.realtimeSinceStartup + 3f;
            while (child == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                foreach (var b in Object.FindObjectsByType<BulletRuntime>(FindObjectsSortMode.None))
                    if (b != root && b.GetState<MatryoshkaProjectileAsset.MatryoshkaState>(matryoshka).Level == 2) child = b;
            }

            Assert.IsTrue(child != null);
            Assert.AreEqual(2, child.GetState<MatryoshkaProjectileAsset.MatryoshkaState>(matryoshka).Level);
            Assert.AreEqual(50f * Mathf.Lerp(0.3f, 2f, 2f / 3f), child.Stats.Damage, 1e-2f);
            Assert.AreEqual(12f * Mathf.Lerp(0.5f, 1.3f, 2f / 3f), child.Stats.Size, 1e-2f);
        }

        [UnityTest]
        public IEnumerator Matryoshka_Level0Child_KeepsPierceHasScaledStatsAndSpawnsNothing()
        {
            var matryoshka = ScriptableObject.CreateInstance<MatryoshkaProjectileAsset>();
            var enemy = MakeEnemy(new Vector2(2f, 0f), 100000f);
            var definition = BulletTestUtil.MakeBasicDefinition(0f, 5f, 800f, 50f, matryoshka);
            // Root flies away from the enemy so only the Level 0 child can hit it.
            var root = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.left, BulletFaction.Player);
            var child = root.SpawnChild(matryoshka, Vector2.zero, Vector2.right, 0f,
                c => c.GetState<MatryoshkaProjectileAsset.MatryoshkaState>(matryoshka).Level = 0);

            Assert.Greater(child.Stats.Pierce, 0f); // Level 0 does not zero Pierce
            Assert.AreEqual(50f * 0.3f, child.Stats.Damage, 1e-2f);
            Assert.AreEqual(12f * 0.5f, child.Stats.Size, 1e-2f);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (child != null && Time.realtimeSinceStartup < deadline) yield return null;
            yield return null;

            Assert.IsTrue(child == null, "Level 0 child should die on its hit.");
            Assert.Less(enemy.Health, 100000f);
            var others = new System.Collections.Generic.List<BulletRuntime>(
                Object.FindObjectsByType<BulletRuntime>(FindObjectsSortMode.None));
            Assert.IsTrue(others.TrueForAll(o => o == root), "Level 0 hit must not spawn children.");
        }

        [UnityTest]
        public IEnumerator Targeting_FiltersByFactionAndPicksClosest()
        {
            var near = MakeEnemy(new Vector2(3f, 0f));
            var far = MakeEnemy(new Vector2(6f, 0f));
            var playerBullet = Fire(BulletFaction.Player);
            var enemyBullet = Fire(BulletFaction.Enemy);
            yield return null;

            Assert.AreEqual(2, BulletTargeting.GetHostileTargets(playerBullet).Count);
            Assert.AreEqual(1, BulletTargeting.GetHostileTargets(playerBullet, near).Count);
            Assert.AreEqual(0, BulletTargeting.GetHostileTargets(enemyBullet).Count); // enemies are friendly to Enemy bullets
            Assert.AreEqual(near.PhysicsBody, BulletTargeting.GetClosestHostileTarget(playerBullet));
            Assert.AreEqual(far.PhysicsBody, BulletTargeting.GetClosestHostileTarget(playerBullet, near));
            Assert.IsNull(BulletTargeting.GetClosestHostileTarget(enemyBullet));
        }
    }
}
