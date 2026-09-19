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
    }
}
