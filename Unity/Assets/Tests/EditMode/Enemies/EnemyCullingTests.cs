using NUnit.Framework;
using Gamelab.Config;
using Gamelab.Enemies.Core;

namespace Gamelab.Tests.Enemies
{
    public class EnemyCullingTests
    {
        private sealed class Bounds : IWorldBounds
        {
            public float MinX { get; set; }
            public float MaxX { get; set; }
        }

        private static readonly Bounds B = new Bounds { MinX = 100f, MaxX = 1000f };

        [Test]
        public void IsOffScreenLeft_StrictlyBeyondMinXMinusSize()
        {
            Assert.IsTrue(EnemyCulling.IsOffScreenLeft(27f, 72f, B));
            Assert.IsFalse(EnemyCulling.IsOffScreenLeft(28f, 72f, B));
            Assert.IsFalse(EnemyCulling.IsOffScreenLeft(500f, 72f, B));
        }

        [Test]
        public void IsFleeCulled_LeftOrFarRight()
        {
            Assert.IsTrue(EnemyCulling.IsFleeCulled(27f, 72f, B));
            Assert.IsTrue(EnemyCulling.IsFleeCulled(1600.5f, 72f, B));
            Assert.IsFalse(EnemyCulling.IsFleeCulled(1600f, 72f, B));
            Assert.IsFalse(EnemyCulling.IsFleeCulled(500f, 72f, B));
        }

        [Test]
        public void EnemyTuning_Default_MatchesSrcEffectiveValues()
        {
            var t = EnemyTuning.Default;
            Assert.AreEqual(100f, t.EnemyHealth);
            Assert.AreEqual(72f, t.EnemySize);
            Assert.AreEqual(700f, t.RifleMaxSpeed);
            Assert.AreEqual(30f, t.RiflePreferredDistance);
            Assert.AreEqual(4f, t.EnemyShootCooldown);
            Assert.AreEqual(6f, t.TutorialEnemyShootCooldown);
            Assert.AreEqual(1f, t.TutorialEnemyHealth);
            Assert.AreEqual(0.2f, t.EnemyShootSpread);
            Assert.AreEqual(0.75f, t.EnemyFleeDelay);
            Assert.AreEqual(80, t.TrainTileSize);
        }
    }
}
