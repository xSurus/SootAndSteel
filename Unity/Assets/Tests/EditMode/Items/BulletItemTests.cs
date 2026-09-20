using System;
using Gamelab.Items.Bullets;
using NUnit.Framework;

namespace Gamelab.Tests.Items
{
    public class BulletItemTests
    {
        private static BulletItem B(string id) => new BulletItem(id);

        [Test]
        public void SingleIdSetsTypeAndId()
        {
            var item = B(ComponentIds.ScatterCasing);
            Assert.AreEqual("Bullet", item.Id);
            Assert.AreEqual(EComponentType.Casing, item.Type);
            CollectionAssert.AreEqual(new[] { ComponentIds.ScatterCasing }, item.ComponentIds);
            Assert.IsFalse(item.HasBasic);
            Assert.IsTrue(item.HasUpgrade);
        }

        [Test]
        public void BasicIdIsBasicNotUpgrade()
        {
            var item = B(ComponentIds.BasicPropellant);
            Assert.IsTrue(item.HasBasic);
            Assert.IsFalse(item.HasUpgrade);
        }

        [Test]
        public void SingleComponentCtorCopies()
        {
            var copy = new BulletItem(B(ComponentIds.HeavyPropellant));
            Assert.AreEqual(EComponentType.Propellant, copy.Type);
            CollectionAssert.AreEqual(new[] { ComponentIds.HeavyPropellant }, copy.ComponentIds);
        }

        [Test]
        public void SameTypeMergesAsUpgradeWithBasicFirst()
        {
            var item = new BulletItem(B(ComponentIds.BurstCasing), B(ComponentIds.BasicCasing));
            Assert.AreEqual(EComponentType.Casing, item.Type);
            CollectionAssert.AreEqual(new[] { ComponentIds.BasicCasing, ComponentIds.BurstCasing }, item.ComponentIds);
            Assert.IsTrue(item.HasBasic);
            Assert.IsTrue(item.HasUpgrade);
        }

        [Test]
        public void ThreeDistinctTypesMakeBullet()
        {
            var item = new BulletItem(
                B(ComponentIds.HomingPropellant), B(ComponentIds.PiercingProjectile), B(ComponentIds.BasicCasing));
            Assert.AreEqual(EComponentType.Bullet, item.Type);
            CollectionAssert.AreEqual(
                new[] { ComponentIds.BasicCasing, ComponentIds.HomingPropellant, ComponentIds.PiercingProjectile },
                item.ComponentIds);
        }

        [Test]
        public void TwoDistinctTypesThrow()
        {
            Assert.Throws<Exception>(() => new BulletItem(B(ComponentIds.BasicCasing), B(ComponentIds.BasicProjectile)));
        }

        [Test]
        public void BulletTypeInsideThreeThrows()
        {
            var bullet = new BulletItem(
                B(ComponentIds.BasicCasing), B(ComponentIds.BasicProjectile), B(ComponentIds.BasicPropellant));
            Assert.Throws<Exception>(() => new BulletItem(bullet, B(ComponentIds.BasicCasing), B(ComponentIds.BasicProjectile)));
        }

        [Test]
        public void ToRecipeCarriesTypeAndIds()
        {
            var recipe = new BulletItem(B(ComponentIds.ScatterCasing), B(ComponentIds.BasicCasing)).ToRecipe();
            Assert.AreEqual(EComponentType.Casing, recipe.Type);
            CollectionAssert.AreEqual(new[] { ComponentIds.BasicCasing, ComponentIds.ScatterCasing }, recipe.ComponentIds);
        }

        [Test]
        public void TraitsCoverAllIdsAndRejectUnknown()
        {
            foreach (var f in typeof(ComponentIds).GetFields())
            {
                var id = (string)f.GetRawConstantValue();
                Assert.DoesNotThrow(() => ComponentTraits.TypeOf(id), id);
                Assert.AreEqual(id.StartsWith("Basic"), ComponentTraits.IsBasic(id), id);
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => ComponentTraits.TypeOf("Nope"));
            Assert.Throws<ArgumentOutOfRangeException>(() => ComponentTraits.IsBasic("Nope"));
        }
    }
}
