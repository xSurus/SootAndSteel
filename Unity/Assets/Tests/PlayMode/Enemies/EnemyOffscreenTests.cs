using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;

namespace Gamelab.Tests.Enemies
{
    public class EnemyOffscreenTests
    {
        private sealed class FakeBounds : IWorldBounds
        {
            public float MinX => 0f;
            public float MaxX => 1000f;
        }

        private GameObject go;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (go != null) Object.Destroy(go);
            yield return null;
        }

        private DummyEnemyRuntime Make(float xMeters, IWorldBounds bounds)
        {
            go = new GameObject("Dummy");
            go.AddComponent<Rigidbody2D>();
            go.transform.position = new Vector3(xMeters, 0f, 0f);
            var e = go.AddComponent<DummyEnemyRuntime>();
            e.Initialize(new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f });
            e.PhysicsBody.position = new Vector2(xMeters, 0f); // the cull reads the body position
            e.Bounds = bounds;
            return e;
        }

        [UnityTest]
        public IEnumerator PastLeftEdge_WithBounds_IsFlagged()
        {
            var e = Make(-1f, new FakeBounds()); // -100 px < 0 - 72
            yield return null;
            Assert.IsTrue(e.ShouldRemove);
        }

        [UnityTest]
        public IEnumerator InsideBounds_NotFlagged()
        {
            var e = Make(-0.5f, new FakeBounds()); // -50 px >= -72
            yield return null;
            Assert.IsFalse(e.ShouldRemove);
        }

        [UnityTest]
        public IEnumerator NullBounds_NeverFlagged()
        {
            var e = Make(-10f, null);
            yield return null;
            Assert.IsFalse(e.ShouldRemove);
        }
    }
}
