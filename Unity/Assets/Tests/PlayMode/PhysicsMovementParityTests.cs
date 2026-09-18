using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities;

namespace Gamelab.Tests.PhysicalEntities
{
    public class PhysicsMovementParityTests
    {
        private const float FixedDt = 1f / 60f;
        private const float Radius = 0.1f;
        private const float Density = 3f;
        private const float LinearDamping = 20f;
        private const float ForceMagnitude = 10f;

        private GameObject entityObject;
        private GameObject wallObject;

        [TearDown]
        public void TearDown()
        {
            if (entityObject != null) Object.Destroy(entityObject);
            if (wallObject != null) Object.Destroy(wallObject);
        }

        private static float Mass(float radius, float density) => density * Mathf.PI * radius * radius;

        private static Vector2 ExpectedVelocityAfterSteps(Vector2 force, float mass, float damping, int steps,
            float dt)
        {
            Vector2 velocity = Vector2.zero;
            Vector2 acceleration = force / mass;
            for (int i = 0; i < steps; i++)
            {
                velocity += acceleration * dt;
                velocity *= 1f / (1f + dt * damping);
            }

            return velocity;
        }

        [UnityTest]
        public IEnumerator DynamicCircle_UnderConstantForce_MatchesBox2DDampingRecurrence()
        {
            entityObject = new GameObject("MovingEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(Radius, Density, LinearDamping, true);
            entity.Position = Vector2.zero;

            Vector2 force = new Vector2(ForceMagnitude, 0f);
            const int steps = 30;

            for (int i = 0; i < steps; i++)
            {
                entity.Body.AddForce(force);
                yield return new WaitForFixedUpdate();
            }

            Vector2 expected = ExpectedVelocityAfterSteps(force, Mass(Radius, Density), LinearDamping, steps,
                FixedDt);
            Vector2 actual = entity.Body.linearVelocity;

            Assert.AreEqual(expected.x, actual.x, expected.x * 0.1f + 0.01f,
                $"expected ~{expected}, got {actual} after {steps} fixed steps");
            Assert.AreEqual(expected.y, actual.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator DynamicCircle_PushedIntoStaticWall_RestsAtWallInsteadOfTunneling()
        {
            entityObject = new GameObject("MovingEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(Radius, Density, LinearDamping, true);
            entity.Position = new Vector2(-1f, 0f);

            wallObject = new GameObject("Wall");
            wallObject.transform.position = Vector3.zero;
            BoxCollider2D wallCollider = wallObject.AddComponent<BoxCollider2D>();
            wallCollider.size = new Vector2(0.2f, 2f);
            Rigidbody2D wallBody = wallObject.AddComponent<Rigidbody2D>();
            wallBody.bodyType = RigidbodyType2D.Static;

            Vector2 force = new Vector2(ForceMagnitude, 0f);
            for (int i = 0; i < 120; i++)
            {
                entity.Body.AddForce(force);
                yield return new WaitForFixedUpdate();
            }

            // The wall's left face is at x = -0.1 (half-width 0.1); a body of radius 0.1 resting against
            // it settles around x ~= -0.2, and must never pass x = -0.1 (that would mean it tunneled
            // through the wall).
            Assert.Less(entity.Position.x, -0.1f,
                $"entity tunneled through the wall, ended up at {entity.Position}");
            Assert.Greater(entity.Position.x, -0.95f,
                $"entity should have moved substantially toward the wall, only reached {entity.Position}");
        }
    }
}
