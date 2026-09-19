using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;

namespace Gamelab.Tests.Bullets
{
    public class BulletRuntimeTickTests
    {
        private class CountingCasingAsset : BasicCasingAsset
        {
            public int Cleanups;
            public override void OnCleanup(BulletRuntime bullet) => Cleanups++;
        }

        private static int BulletObjectCount()
        {
            int n = 0;
            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.StartsWith("Bullet_")) n++;
            }
            return n;
        }

        [UnityTest]
        public IEnumerator Spawn_Speed800_VelocityIsEightMetersPerSecond()
        {
            var bullet = BulletRuntime.Spawn(
                BulletTestUtil.MakeBasicDefinition(), Vector2.zero, Vector2.right, BulletFaction.Player);
            yield return null;

            Assert.AreEqual(8f, bullet.PhysicsBody.linearVelocity.x, 0.001f);
            Assert.AreEqual(0f, bullet.PhysicsBody.linearVelocity.y, 0.001f);
            UnityEngine.Object.Destroy(bullet.gameObject);
        }

        [UnityTest]
        public IEnumerator FixedUpdate_ExpiredLifetime_DestroysBulletAndCleansUpOnce()
        {
            var definition = BulletTestUtil.MakeBasicDefinition(lifetime: 0.2f);
            var casing = ScriptableObject.CreateInstance<CountingCasingAsset>();
            BulletTestUtil.SetPrivate<BulletComponentAsset>(definition, "casing", casing);

            var bullet = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);
            yield return new WaitForSeconds(0.6f);

            Assert.IsTrue(bullet == null);
            Assert.AreEqual(1, casing.Cleanups);
        }

        [Test]
        public void Spawn_EmptyCasingSlot_ThrowsAndCreatesNoObject()
        {
            var definition = BulletTestUtil.MakeBasicDefinition();
            BulletTestUtil.SetPrivate<BulletComponentAsset>(definition, "casing", null);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player));
            StringAssert.Contains("casing", ex.Message);
            Assert.AreEqual(0, BulletObjectCount());
        }

        [Test]
        public void Spawn_CasingInProjectileSlot_ThrowsTypeMismatch()
        {
            var definition = BulletTestUtil.MakeBasicDefinition();
            BulletTestUtil.SetPrivate<BulletComponentAsset>(
                definition, "projectile", ScriptableObject.CreateInstance<BasicCasingAsset>());

            var ex = Assert.Throws<InvalidOperationException>(() =>
                BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player));
            StringAssert.Contains("projectile", ex.Message);
            StringAssert.Contains("mismatch", ex.Message);
            Assert.AreEqual(0, BulletObjectCount());
        }

        [Test]
        public void Spawn_NullDefinition_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                BulletRuntime.Spawn(null, Vector2.zero, Vector2.right, BulletFaction.Player));
        }
    }
}
