using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Stations;
using NUnit.Framework;

namespace Gamelab.Tests.Stations
{
    public class WorkbenchCraftingTests
    {
        private static BulletItem B(string id) => new BulletItem(id);

        private static WorkbenchCrafting With(params string[] ids)
        {
            var w = new WorkbenchCrafting();
            foreach (string id in ids) w.Place(B(id));
            return w;
        }

        [Test]
        public void EmptyTable_AcceptsBulletItemsOnly()
        {
            var w = new WorkbenchCrafting();
            Assert.IsTrue(w.CanPlace(B(ComponentIds.BasicCasing)));
            Assert.IsFalse(w.CanPlace(new Gamelab.Items.Item("Coal")));
            Assert.IsFalse(w.CanPlace(null));
        }

        [Test]
        public void DuplicateComponentId_Refused()
        {
            var w = With(ComponentIds.BasicCasing);
            Assert.IsFalse(w.CanPlace(B(ComponentIds.BasicCasing)));
        }

        [Test]
        public void UpgradeRule_SameTypeUpToTwo()
        {
            var w = With(ComponentIds.BasicCasing);
            Assert.IsTrue(w.CanPlace(B(ComponentIds.ScatterCasing)));
            w.Place(B(ComponentIds.ScatterCasing));
            Assert.IsFalse(w.CanPlace(B(ComponentIds.BurstCasing)), "Upgrade holds two at most");
        }

        [Test]
        public void FinalBulletRule_ThreeDistinctTypesAllBasic()
        {
            var w = With(ComponentIds.BasicCasing);
            Assert.IsTrue(w.CanPlace(B(ComponentIds.BasicProjectile)));
            w.Place(B(ComponentIds.BasicProjectile));
            Assert.IsTrue(w.CanPlace(B(ComponentIds.BasicPropellant)));
            w.Place(B(ComponentIds.BasicPropellant));
            Assert.IsFalse(w.CanPlace(B(ComponentIds.HeavyPropellant)), "Max three items");
        }

        [Test]
        public void FinalBulletRule_RefusesNewTypeWithoutBasic()
        {
            var w = With(ComponentIds.ScatterCasing);
            Assert.IsFalse(w.CanPlace(B(ComponentIds.BasicProjectile)));
            var w2 = With(ComponentIds.BasicCasing);
            Assert.IsFalse(w2.CanPlace(B(ComponentIds.PiercingProjectile)));
        }

        [Test]
        public void CanCraft_OnlyForValidSets()
        {
            Assert.IsFalse(new WorkbenchCrafting().CanCraft());
            Assert.IsFalse(With(ComponentIds.BasicCasing).CanCraft());
            Assert.IsTrue(With(ComponentIds.BasicCasing, ComponentIds.ScatterCasing).CanCraft());
            Assert.IsFalse(With(ComponentIds.BasicCasing, ComponentIds.BasicProjectile).CanCraft());
            Assert.IsTrue(With(ComponentIds.BasicCasing, ComponentIds.BasicProjectile, ComponentIds.BasicPropellant).CanCraft());
            Assert.IsFalse(With(ComponentIds.ScatterCasing, ComponentIds.BasicProjectile, ComponentIds.BasicPropellant).CanCraft());
        }

        [Test]
        public void InvalidSet_DoesNotProgress()
        {
            var w = With(ComponentIds.BasicCasing);
            Assert.IsFalse(w.InteractHeld(5f));
            Assert.AreEqual(0f, w.CraftProgress);
            Assert.IsFalse(w.IsCrafting);
        }

        [Test]
        public void Craft_TakesTwoSeconds_InHalfSecondChunks()
        {
            var w = With(ComponentIds.BasicCasing, ComponentIds.ScatterCasing);
            Assert.IsFalse(w.InteractHeld(0.5f));
            Assert.IsTrue(w.IsCrafting);
            Assert.IsFalse(w.InteractHeld(0.5f));
            Assert.IsFalse(w.InteractHeld(0.5f));
            Assert.IsTrue(w.InteractHeld(0.5f));
            Assert.AreEqual(1, w.PlacedItems.Count);
            Assert.AreEqual(EComponentType.Casing, w.Peek().Type);
            CollectionAssert.AreEquivalent(new[] { ComponentIds.BasicCasing, ComponentIds.ScatterCasing }, w.Peek().ComponentIds);
            Assert.AreEqual(0f, w.CraftProgress);
            Assert.IsFalse(w.IsCrafting);
        }

        [Test]
        public void FullBullet_CraftsToBulletType()
        {
            var w = With(ComponentIds.BasicCasing, ComponentIds.BasicProjectile, ComponentIds.BasicPropellant);
            Assert.IsTrue(w.InteractHeld(2f));
            Assert.AreEqual(EComponentType.Bullet, w.Peek().Type);
            Assert.AreEqual(3, w.Peek().ComponentIds.Count);
        }

        [Test]
        public void Release_KeepsProgress_AndBlocksPlaceAndTake()
        {
            var w = With(ComponentIds.BasicCasing, ComponentIds.ScatterCasing);
            w.InteractHeld(0.5f);
            w.InteractReleased();
            Assert.IsFalse(w.IsCrafting);
            Assert.AreEqual(0.5f, w.CraftProgress, 1e-6f);
            Assert.IsFalse(w.CanTake);
            Assert.IsNull(w.Peek());
            Assert.IsFalse(w.CanPlace(B(ComponentIds.BasicProjectile)));
            // Resuming continues from the kept progress.
            Assert.IsTrue(w.InteractHeld(1.5f));
        }

        [Test]
        public void Take_IsLifo()
        {
            var w = With(ComponentIds.BasicCasing, ComponentIds.ScatterCasing);
            Assert.IsTrue(w.CanTake);
            Assert.AreEqual(ComponentIds.ScatterCasing, w.Take().ComponentIds.Single());
            Assert.AreEqual(ComponentIds.BasicCasing, w.Peek().ComponentIds.Single());
        }
    }
}
