using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Projectiles;

namespace Gamelab.Tests.Bullets
{
    public class BulletComponentModelTests
    {
        private class Counter { public int N; }

        // Spawns one child from its spawn phase unless it is non-root. Logs call order.
        private class SpawnerProjectileAsset : BasicProjectileAsset
        {
            public float ChildDelay;
            public int Creates, Spawns, ChildrenSpawned;
            public List<string> Log = new List<string>();

            public override void OnCreate(BulletRuntime bullet)
            {
                Creates++;
                Log.Add("create");
            }

            public override void OnSpawn(BulletRuntime bullet)
            {
                Spawns++;
                Log.Add("spawn");
                if (!bullet.IsRoot(this)) return;
                ChildrenSpawned++;
                bullet.SpawnChild(this, bullet.transform.position, Vector2.up, ChildDelay,
                    b => Log.Add(Creates == 0 ? "configure-first" : "configure-late"));
            }
        }

        private static int BulletCount()
        {
            int n = 0;
            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go.name.StartsWith("Bullet_")) n++;
            return n;
        }

        private static BulletDefinitionAsset DefinitionWith(SpawnerProjectileAsset spawner)
        {
            // Replace the basic projectile with the spawner (same type).
            var definition = BulletTestUtil.MakeBasicDefinition();
            var list = new List<BulletComponentAsset>(definition.Components);
            list.RemoveAll(c => c.Type == Gamelab.Items.Bullets.EComponentType.Projectile);
            list.Add(spawner);
            BulletTestUtil.SetPrivate(definition, "components", list);
            return definition;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go.name.StartsWith("Bullet_")) UnityEngine.Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetState_SameBulletSameInstance_DifferentBulletsDifferent()
        {
            var definition = BulletTestUtil.MakeBasicDefinition();
            var key = definition.Components[0];
            var a = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);
            var b = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);
            yield return null;

            Assert.AreSame(a.GetState<Counter>(key), a.GetState<Counter>(key));
            Assert.AreNotSame(a.GetState<Counter>(key), b.GetState<Counter>(key));
        }

        [UnityTest]
        public IEnumerator SpawnChild_MarksSpawnerNonRootAndDoesNotRecurse()
        {
            var spawner = ScriptableObject.CreateInstance<SpawnerProjectileAsset>();
            var definition = DefinitionWith(spawner);
            var root = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);
            yield return null;

            Assert.AreEqual(1, spawner.ChildrenSpawned);
            Assert.AreEqual(2, BulletCount()); // root + one child, no recursion
            Assert.IsTrue(root.IsRoot(spawner));
            BulletRuntime child = null;
            foreach (var b in UnityEngine.Object.FindObjectsByType<BulletRuntime>(FindObjectsSortMode.None))
                if (b != root) child = b;
            Assert.IsFalse(child.IsRoot(spawner));
            Assert.IsTrue(child.IsRoot(definition.Components[0]));
            Assert.AreEqual(BulletFaction.Player, child.Faction);
            // configure ran before the child's OnCreate (Creates was 1 from the root only)
            CollectionAssert.AreEqual(
                new[] { "create", "spawn", "configure-late", "create", "spawn" }, spawner.Log);
        }

        [UnityTest]
        public IEnumerator SpawnChild_ConfigureRunsBeforeCreateAndSpawn()
        {
            var log = new List<string>();
            var probe = ScriptableObject.CreateInstance<ProbeCasingAsset>();
            probe.Log = log;
            var def2 = BulletTestUtil.MakeBasicDefinition(extra: probe);
            var r2 = BulletRuntime.Spawn(def2, Vector2.zero, Vector2.right, BulletFaction.Player);
            log.Clear();
            r2.SpawnChild(probe, Vector2.zero, Vector2.up, 0f, b => log.Add("configure"));
            yield return null;

            CollectionAssert.AreEqual(new[] { "configure", "create", "spawn" }, log);
        }

        private class ProbeCasingAsset : BasicCasingAsset
        {
            public List<string> Log;
            public override void OnCreate(BulletRuntime bullet) => Log.Add("create");
            public override void OnSpawn(BulletRuntime bullet) => Log.Add("spawn");
        }

        [UnityTest]
        public IEnumerator SpawnChild_Delayed_SpawnsAfterDelayViaFixedUpdate()
        {
            var spawner = ScriptableObject.CreateInstance<SpawnerProjectileAsset>();
            spawner.ChildDelay = 0.3f;
            var definition = DefinitionWith(spawner);
            var root = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);
            yield return null;

            BulletRuntime child = null;
            foreach (var b in UnityEngine.Object.FindObjectsByType<BulletRuntime>(FindObjectsSortMode.None))
                if (b != root) child = b;
            Assert.IsTrue(child.IsPending);
            Assert.IsFalse(child.PhysicsBody.simulated);
            Assert.AreEqual(1, spawner.Spawns); // root only
            Assert.AreEqual(0f, child.Age);

            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(child.IsPending); // about 0.1 s of 0.3 s, still pending with no OnSpawn
            Assert.AreEqual(1, spawner.Spawns);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (child.IsPending && Time.realtimeSinceStartup < deadline) yield return null;

            Assert.IsFalse(child.IsPending);
            Assert.IsTrue(child.PhysicsBody.simulated);
            Assert.AreEqual(2, spawner.Spawns);
            Assert.Greater(child.PhysicsBody.linearVelocity.y, 0f);
        }

        [UnityTest]
        public IEnumerator Deactivate_WhilePending_Destroys()
        {
            var spawner = ScriptableObject.CreateInstance<SpawnerProjectileAsset>();
            spawner.ChildDelay = 5f;
            var root = BulletRuntime.Spawn(DefinitionWith(spawner), Vector2.zero, Vector2.right, BulletFaction.Player);
            BulletRuntime child = null;
            foreach (var b in UnityEngine.Object.FindObjectsByType<BulletRuntime>(FindObjectsSortMode.None))
                if (b != root) child = b;
            Assert.IsTrue(child.IsPending);

            child.Deactivate();
            yield return null;

            Assert.IsTrue(child == null);
            Assert.AreEqual(1, spawner.Spawns);
        }

        [UnityTest]
        public IEnumerator SpawnChild_DelayZero_SpawnsImmediately()
        {
            var spawner = ScriptableObject.CreateInstance<SpawnerProjectileAsset>();
            var root = BulletRuntime.Spawn(DefinitionWith(spawner), Vector2.zero, Vector2.right, BulletFaction.Player);
            yield return null;

            Assert.AreEqual(2, spawner.Spawns);
            Assert.AreEqual(2, spawner.Creates);
        }

        private class UpdateCounterProjectileAsset : BasicProjectileAsset
        {
            public int Updates;
            public override void OnUpdate(BulletRuntime bullet, float deltaTime) => Updates++;
        }

        [UnityTest]
        public IEnumerator OnUpdate_NotCalledWhilePending()
        {
            var probe = ScriptableObject.CreateInstance<UpdateCounterProjectileAsset>();
            var root = BulletRuntime.Spawn(BulletTestUtil.MakeDefinitionWith(probe), Vector2.zero, Vector2.right, BulletFaction.Player);
            root.SpawnChild(probe, Vector2.zero, Vector2.up, 5f);
            root.Deactivate(); // stops the root's own updates
            probe.Updates = 0;
            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(0, probe.Updates);
        }

        [Test]
        public void Validate_BadLists_Throw()
        {
            var basic = BulletTestUtil.MakeBasicDefinition();
            var comps = new List<BulletComponentAsset>(basic.Components);

            var empty = BulletTestUtil.MakeBasicDefinition();
            BulletTestUtil.SetPrivate(empty, "components", new List<BulletComponentAsset>());
            StringAssert.Contains("empty", Assert.Throws<InvalidOperationException>(empty.Validate).Message);

            var dup = BulletTestUtil.MakeBasicDefinition();
            BulletTestUtil.SetPrivate(dup, "components", new List<BulletComponentAsset>(comps) { comps[0] });
            StringAssert.Contains("twice", Assert.Throws<InvalidOperationException>(dup.Validate).Message);

            var nul = BulletTestUtil.MakeBasicDefinition();
            BulletTestUtil.SetPrivate(nul, "components", new List<BulletComponentAsset>(comps) { null });
            StringAssert.Contains("null", Assert.Throws<InvalidOperationException>(nul.Validate).Message);

            var missing = BulletTestUtil.MakeBasicDefinition();
            BulletTestUtil.SetPrivate(missing, "components", comps.GetRange(0, 2));
            StringAssert.Contains("Projectile", Assert.Throws<InvalidOperationException>(missing.Validate).Message);
        }
    }
}
