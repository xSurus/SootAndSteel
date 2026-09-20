using Gamelab.Map.Train;
using NUnit.Framework;

namespace Gamelab.Tests.Map
{
    public class WallHealthTests
    {
        [Test]
        public void Defaults_AreSrcValues()
        {
            var h = new WallHealth();
            Assert.AreEqual(50f, h.Max);
            Assert.AreEqual(50f, h.Current);
            Assert.AreEqual(0f, h.DamagePercent, 1e-4f);
        }

        [Test]
        public void TakeDamage_ReachingZero_BreachesOnce_AndBrokenIgnoresDamage()
        {
            var h = new WallHealth();
            int breaches = 0;
            h.Breached += () => breaches++;
            h.TakeDamage(20f);
            Assert.AreEqual(40f, h.DamagePercent, 1e-4f);
            Assert.AreEqual(0, breaches);
            h.TakeDamage(100f);
            Assert.IsTrue(h.IsBroken);
            Assert.IsTrue(h.IsBreached);
            h.TakeDamage(5f);
            Assert.AreEqual(1, breaches);
            Assert.AreEqual(0f, h.Current);
        }

        [Test]
        public void Repair_RestoresAtRate_AndFiresRepairedOnlyWhenFullAfterBreach()
        {
            var h = new WallHealth();
            int repaired = 0;
            h.Repaired += () => repaired++;
            h.TakeDamage(50f);
            h.Repair(1f); // +20
            Assert.AreEqual(20f, h.Current, 1e-4f);
            Assert.IsFalse(h.IsBroken);
            Assert.IsTrue(h.IsBreached, "stays breached until full");
            h.Repair(1f);
            Assert.AreEqual(0, repaired);
            h.Repair(1f);
            Assert.AreEqual(50f, h.Current);
            Assert.AreEqual(1, repaired);
            Assert.IsFalse(h.IsBreached);
            h.Repair(1f);
            Assert.AreEqual(1, repaired);
        }

        [Test]
        public void Repair_OfMereDamage_FiresNothing()
        {
            var h = new WallHealth();
            int repaired = 0;
            h.Repaired += () => repaired++;
            h.TakeDamage(10f);
            h.Repair(1f);
            Assert.AreEqual(50f, h.Current);
            Assert.AreEqual(0, repaired);
        }
    }

    public class WallSpriteNamesTests
    {
        [TestCase(0f, "WallTileTop")]
        [TestCase(24.9f, "WallTileTop")]
        [TestCase(25f, "WallTileTopBroken1_Variation2")]
        [TestCase(49.9f, "WallTileTopBroken1_Variation2")]
        [TestCase(50f, "WallTileTopBroken2_Variation2")]
        [TestCase(74.9f, "WallTileTopBroken2_Variation2")]
        [TestCase(75f, "WallTileTopBroken3_Variation2")]
        [TestCase(99.9f, "WallTileTopBroken3_Variation2")]
        public void Top_Thresholds(float damage, string expected) =>
            Assert.AreEqual(expected, WallSpriteNames.Select(true, damage, false, 2));

        [Test]
        public void Top_Breached_IsBroken4() =>
            Assert.AreEqual("WallTileTopBroken4_Variation3", WallSpriteNames.Select(true, 100f, true, 3));

        [TestCase(0f, "WallTileBottom")]
        [TestCase(32.9f, "WallTileBottom")]
        [TestCase(33f, "WallTileBottomBroken1")]
        [TestCase(65.9f, "WallTileBottomBroken1")]
        [TestCase(66f, "WallTileBottomBroken2")]
        [TestCase(99.9f, "WallTileBottomBroken2")]
        public void Bottom_Thresholds(float damage, string expected) =>
            Assert.AreEqual(expected, WallSpriteNames.Select(false, damage, false, 2));

        [Test]
        public void Bottom_Breached_IsBroken3_IgnoringVariation() =>
            Assert.AreEqual("WallTileBottomBroken3", WallSpriteNames.Select(false, 100f, true, 1));
    }
}
