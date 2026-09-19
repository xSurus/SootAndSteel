using NUnit.Framework;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Bullets
{
    public class BulletStatsTests
    {
        [Test]
        public void CannonDefault_MatchesGameplayConfigLiterals()
        {
            BulletStats stats = BulletStats.CannonDefault();

            Assert.AreEqual(800f, stats.Speed);
            Assert.AreEqual(50f, stats.Damage);
            Assert.AreEqual(1f, stats.Pierce);
            Assert.AreEqual(12f, stats.Size);
            Assert.AreEqual(0.2f, stats.Spread);
            Assert.AreEqual(5f, stats.Lifetime);
        }
    }
}
