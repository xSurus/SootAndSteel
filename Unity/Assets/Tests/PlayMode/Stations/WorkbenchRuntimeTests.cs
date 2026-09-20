using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Stations;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.Stations
{
    public class WorkbenchRuntimeTests
    {
        private static T Make<T>(string stationId = "") where T : StationRuntime
        {
            var go = new GameObject("WB");
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<T>();
            s.Initialize(new StationCatalogEntry { stationId = stationId });
            return s;
        }

        private static CounterRuntime MakeConsumer()
        {
            var go = new GameObject("C");
            go.AddComponent<Rigidbody2D>();
            var c = go.AddComponent<CounterRuntime>();
            c.Initialize(new StationCatalogEntry { stationId = StationIds.Counter });
            return c;
        }

        private static void Put(StationRuntime s, string id)
        {
            var item = new BulletItem(id);
            Assert.IsTrue(s.CanReceiveItem(item, null));
            s.ReceiveItem(item, null);
        }

        [Test]
        public void StationId_DefaultsWhenEntryEmpty_AndKeepsGivenId()
        {
            var w = Make<WorkbenchRuntime>();
            var a = Make<AutoWorkbenchRuntime>();
            var kept = Make<WorkbenchRuntime>("Custom");
            Assert.AreEqual(StationIds.Workbench, w.StationId);
            Assert.AreEqual(StationIds.AutoWorkbench, a.StationId);
            Assert.AreEqual("Custom", kept.StationId);
            Object.DestroyImmediate(w.gameObject);
            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(kept.gameObject);
        }

        [Test]
        public void ReceivePeekProvide_UsesLogic()
        {
            var w = Make<WorkbenchRuntime>();
            Assert.IsNull(w.PeekNextItem());
            Assert.IsFalse(w.TryProvideItem(out _));
            Put(w, ComponentIds.BasicCasing);
            Assert.IsFalse(w.CanReceiveItem(new Item("Coal"), null));
            Assert.AreEqual("Bullet", w.PeekNextItem().Id);
            Assert.IsTrue(w.TryProvideItem(out Item item));
            Assert.AreEqual(ComponentIds.BasicCasing, ((BulletItem)item).ComponentIds[0]);
            Object.DestroyImmediate(w.gameObject);
        }

        [Test]
        public void TryProvide_RemovesConsumerTicket_AndRespectsFifo()
        {
            var w = Make<WorkbenchRuntime>();
            Put(w, ComponentIds.BasicCasing);
            Put(w, ComponentIds.ScatterCasing);
            var first = MakeConsumer();
            var second = MakeConsumer();
            w.PingPullIntent(first, 0f);
            w.PingPullIntent(second, 0f);

            Assert.IsFalse(w.TryProvideItem(out _, second));
            Assert.IsTrue(w.TryProvideItem(out _, first));
            Assert.IsTrue(w.TryProvideItem(out _, second), "First ticket was removed, second is now first.");
            Object.DestroyImmediate(w.gameObject);
            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }

        [Test]
        public void InteractHeld_CraftsAfterTwoSeconds_ReleaseBlocksTake()
        {
            var w = Make<WorkbenchRuntime>();
            Put(w, ComponentIds.BasicCasing);
            Put(w, ComponentIds.ScatterCasing);
            w.OnInteractHeld(null, 0.5f);
            w.OnInteractReleased(null);
            Assert.IsFalse(w.CanProvideItem(null));
            for (int i = 0; i < 3; i++) w.OnInteractHeld(null, 0.5f);
            Assert.IsTrue(w.CanProvideItem(null));
            Assert.AreEqual(1, w.Crafting.PlacedItems.Count);
            Object.DestroyImmediate(w.gameObject);
        }

        [Test]
        public void AutoWorkbench_CraftsThroughUpdate()
        {
            var w = Make<AutoWorkbenchRuntime>();
            Put(w, ComponentIds.BasicCasing);
            Put(w, ComponentIds.ScatterCasing);
            for (int i = 0; i < 4; i++) w.Update(0.5f);
            Assert.AreEqual(1, w.Crafting.PlacedItems.Count);
            Assert.AreEqual(EComponentType.Casing, w.Crafting.PlacedItems[0].Type);
            Object.DestroyImmediate(w.gameObject);
        }
    }
}
