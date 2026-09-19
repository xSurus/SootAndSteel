using NUnit.Framework;
using Gamelab.Enemies.Movement;

namespace Gamelab.Tests.Enemies
{
    public class EnemyMovementProfileTests
    {
        [Test]
        public void CreateDefault_ScalesFromMaxForwardSpeed()
        {
            EnemyMovementProfile profile = EnemyMovementProfile.CreateDefault(700f);

            Assert.AreEqual(700f, profile.MaxForwardSpeed);
            Assert.AreEqual(245f, profile.MaxReverseSpeed, 0.01f);
            Assert.AreEqual(560f, profile.MaxLateralSpeed, 0.01f);
        }

        [Test]
        public void ToMeters_DividesEveryFieldByPixelsPerMeter()
        {
            EnemyMovementProfile m = new EnemyMovementProfile(200f, 70f, 160f, 480f, 640f, 18f, 120f).ToMeters();

            Assert.AreEqual(2f, m.MaxForwardSpeed, 1e-5f);
            Assert.AreEqual(0.7f, m.MaxReverseSpeed, 1e-5f);
            Assert.AreEqual(1.6f, m.MaxLateralSpeed, 1e-5f);
            Assert.AreEqual(4.8f, m.Acceleration, 1e-5f);
            Assert.AreEqual(6.4f, m.Deceleration, 1e-5f);
            Assert.AreEqual(0.18f, m.ArrivalRadius, 1e-5f);
            Assert.AreEqual(1.2f, m.BrakeRadius, 1e-5f);
        }
    }
}
