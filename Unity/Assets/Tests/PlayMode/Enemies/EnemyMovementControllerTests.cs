using Gamelab.Enemies;
using Gamelab.Enemies.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.Enemies
{
    // Profile is authored in pixels (200 px/s max); the body is in meters, so limits are 2 m/s etc.
    public class EnemyMovementControllerTests
    {
        private const float Dt = 1f / 60f;
        private GameObject go;
        private Rigidbody2D body;
        private EnemyMovementController controller;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Enemy");
            body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            controller = new EnemyMovementController(body, EnemyMovementProfile.CreateDefault(200f));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void FarTarget_AcceleratesTowardItAndCapsAtMetersPerSecond()
        {
            Vector2 target = new Vector2(100f, 0f);
            controller.UpdateTowardPoint(target, Dt);
            Assert.Greater(body.linearVelocity.x, 0f);
            Assert.Less(body.linearVelocity.x, 2f);

            for (int i = 0; i < 300; i++)
            {
                controller.UpdateTowardPoint(target, Dt);
                Assert.LessOrEqual(body.linearVelocity.x, 2f + 1e-4f);
            }
            Assert.AreEqual(2f, body.linearVelocity.x, 1e-3f);
        }

        [Test]
        public void InsideArrivalRadius_DeceleratesToStop()
        {
            body.linearVelocity = new Vector2(2f, 0f);
            for (int i = 0; i < 60; i++)
                controller.UpdateTowardPoint(new Vector2(0.17f, 0f), Dt);
            Assert.AreEqual(0f, body.linearVelocity.magnitude, 1e-4f);
        }

        [Test]
        public void InsideBrakeRadius_DesiredSpeedIsScaledDown()
        {
            // 1 m away, brake radius 1.2 m: desired speed = 2 * 1/1.2 = 1.67 m/s
            for (int i = 0; i < 120; i++)
                controller.UpdateTowardPoint(new Vector2(1f, 0f), Dt);
            Assert.AreEqual(2f / 1.2f, body.linearVelocity.x, 1e-3f);
            Assert.Less(body.linearVelocity.x, 2f);
        }

        [Test]
        public void ZeroDirection_Stops()
        {
            body.linearVelocity = new Vector2(1f, 1f);
            for (int i = 0; i < 60; i++)
                controller.UpdateTowardDirection(Vector2.zero, 2f, Dt);
            Assert.AreEqual(0f, body.linearVelocity.magnitude, 1e-4f);
        }

        [Test]
        public void ReverseSpeed_ClampedToMeters()
        {
            for (int i = 0; i < 300; i++)
                controller.UpdateTowardDirection(Vector2.left, 2f, Dt);
            Assert.AreEqual(-0.7f, body.linearVelocity.x, 1e-4f);
        }

        [Test]
        public void LateralSpeed_ClampedToMeters()
        {
            for (int i = 0; i < 300; i++)
                controller.UpdateTowardDirection(Vector2.up, 2f, Dt);
            Assert.AreEqual(1.6f, body.linearVelocity.y, 1e-4f);
        }
    }
}
