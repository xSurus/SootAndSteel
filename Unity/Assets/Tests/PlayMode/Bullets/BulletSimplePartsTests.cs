using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Propellants;

namespace Gamelab.Tests.Bullets
{
    public class BulletSimplePartsTests
    {
        private static BulletRuntime SpawnWith(BulletComponentAsset part, float spread = 0.1f)
        {
            var definition = BulletTestUtil.MakeDefinitionWith(part);
            BulletTestUtil.SetPrivate(definition, "spread", spread);
            return BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);
        }

        private static List<BulletRuntime> Bullets(BulletRuntime except = null)
        {
            var list = new List<BulletRuntime>();
            foreach (var b in Object.FindObjectsByType<BulletRuntime>(FindObjectsSortMode.None))
                if (b != except) list.Add(b);
            return list;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go.name.StartsWith("Bullet_") || go.name == "Enemy_Test") Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyCasing_SetsDamageTo10()
        {
            var b = SpawnWith(ScriptableObject.CreateInstance<EnemyCasingAsset>());
            yield return null;
            Assert.AreEqual(10f, b.Stats.Damage);
        }

        [UnityTest]
        public IEnumerator HeavyPropellant_ScalesStats()
        {
            var b = SpawnWith(ScriptableObject.CreateInstance<HeavyPropellantAsset>());
            yield return null;
            Assert.AreEqual(100f, b.Stats.Damage, 1e-3f);
            Assert.AreEqual(560f, b.Stats.Speed, 1e-3f);
            Assert.AreEqual(18f, b.Stats.Size, 1e-3f);
        }

        [UnityTest]
        public IEnumerator PiercingProjectile_AddsPierceAndSpeed()
        {
            var b = SpawnWith(ScriptableObject.CreateInstance<PiercingProjectileAsset>());
            yield return null;
            Assert.AreEqual(4f, b.Stats.Pierce);
            Assert.AreEqual(1200f, b.Stats.Speed, 1e-3f);
        }

        [UnityTest]
        public IEnumerator ScatterCasing_SpawnsTenChildrenAndRootIsGone()
        {
            var root = SpawnWith(ScriptableObject.CreateInstance<ScatterCasingAsset>());
            yield return null;

            Assert.IsTrue(root == null);
            var children = Bullets();
            Assert.AreEqual(10, children.Count);
            foreach (var c in children)
            {
                Assert.AreEqual(0.3f, c.Stats.Spread, 1e-4f);
                Assert.AreEqual(10f, c.Stats.Damage, 1e-3f);
                Assert.That(c.Stats.Speed, Is.InRange(640f, 960f));
            }
        }

        [UnityTest]
        public IEnumerator BurstCasing_SpawnsThreeStaggeredChildren()
        {
            var root = SpawnWith(ScriptableObject.CreateInstance<BurstCasingAsset>());
            yield return null;

            Assert.IsTrue(root == null);
            var children = Bullets();
            Assert.AreEqual(3, children.Count);
            foreach (var c in children) Assert.AreEqual(25f, c.Stats.Damage, 1e-3f);

            int pendingNow = children.FindAll(c => c.IsPending).Count;
            Assert.GreaterOrEqual(pendingNow, 2); // delays 0.15 and 0.30 cannot have elapsed in one frame

            float deadline = Time.realtimeSinceStartup + 3f;
            while (children.Exists(c => c.IsPending) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsFalse(children.Exists(c => c.IsPending));
        }

        [UnityTest]
        public IEnumerator RapidFireCasing_SpawnsTenStaggeredChildrenWithStats()
        {
            var root = SpawnWith(ScriptableObject.CreateInstance<RapidFireCasingAsset>());
            yield return null;

            Assert.IsTrue(root == null);
            var children = Bullets();
            Assert.AreEqual(10, children.Count);
            Assert.GreaterOrEqual(children.FindAll(c => c.IsPending).Count, 5); // delays i * 0.1 cannot have elapsed in one frame
            foreach (var c in children)
            {
                Assert.AreEqual(0.5f, c.Stats.Spread, 1e-4f);
                Assert.AreEqual(12.5f, c.Stats.Damage, 1e-3f);
            }
        }

        [UnityTest]
        public IEnumerator FrangibleProjectile_HitSpawnsThreeChildrenWithCooldownAndClearsRoot()
        {
            var go = new GameObject("Enemy_Test");
            go.transform.position = new Vector2(2f, 0f);
            go.AddComponent<Rigidbody2D>();
            var enemy = go.AddComponent<DummyEnemyRuntime>();
            enemy.Initialize(new EnemyCatalogEntry { type = EnemyType.Dummy, health = 1000f, size = 72f });

            var frangible = ScriptableObject.CreateInstance<FrangibleProjectileAsset>();
            var definition = BulletTestUtil.MakeDefinitionWith(frangible);
            BulletTestUtil.SetPrivate(definition, "pierce", 5f); // so the parent survives to be inspected
            var root = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (root.IsRoot(frangible) && Time.realtimeSinceStartup < deadline) yield return null;

            Assert.IsFalse(root.IsRoot(frangible));
            var children = Bullets(root);
            Assert.AreEqual(3, children.Count);
            foreach (var c in children)
            {
                Assert.IsFalse(c.IsRoot(frangible));
                Assert.AreEqual(10f, c.Stats.Damage, 1e-3f); // 50 * 0.2
            }

            // Children overlap the enemy but carry its hit cooldown (1 s): only the parent's hit landed.
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(950f, enemy.Health);
        }
    }
}
