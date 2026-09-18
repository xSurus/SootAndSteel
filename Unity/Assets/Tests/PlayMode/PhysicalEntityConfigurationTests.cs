using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities;

namespace Gamelab.Tests.PhysicalEntities
{
    public class PhysicalEntityConfigurationTests
    {
        private GameObject entityObject;

        [TearDown]
        public void TearDown()
        {
            if (entityObject != null) Object.Destroy(entityObject);
        }

        [UnityTest]
        public IEnumerator ConfigureAsDynamicCircle_SetsBodyAndColliderToMatchPlayerParity()
        {
            entityObject = new GameObject("TestEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();

            // Player-parity constants from Src/Config/GameplayConfig.cs: PlayerRadiusPixels=10,
            // PixelsPerMeter=100 -> 0.1m radius; PlayerDensity=3; PlayerLinearDamping=20; the player body
            // is FixedRotation=true (Src/Players/Player.cs:76-79).
            CircleCollider2D collider = entity.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            yield return null;

            Assert.AreEqual(RigidbodyType2D.Dynamic, entity.Body.bodyType);
            Assert.AreEqual(20f, entity.Body.linearDamping, 0.0001f);
            Assert.IsTrue(entity.Body.freezeRotation);
            Assert.AreEqual(0f, entity.Body.gravityScale, 0.0001f);
            Assert.AreEqual(0.1f, collider.radius, 0.0001f);
            Assert.AreEqual(3f, collider.density, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Position_GetSet_PassesThroughToRigidbody2D()
        {
            entityObject = new GameObject("TestEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            yield return null;

            entity.Position = new Vector2(1.5f, -2f);
            yield return null;

            Assert.AreEqual(new Vector2(1.5f, -2f), entity.Body.position);
            Assert.AreEqual(entity.Body.position, entity.Position);
        }
    }
}
