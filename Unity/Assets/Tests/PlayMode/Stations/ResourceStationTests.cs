using NUnit.Framework;
using UnityEngine;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    public class ResourceStationTests
    {
        private static ResourceStationRuntime Make(string resourceId)
        {
            var go = new GameObject("Res");
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<ResourceStationRuntime>();
            s.Initialize(new StationCatalogEntry(), resourceId);
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

        [Test]
        public void ProvidesItemWithResourceId_AndSetsStationId()
        {
            var s = Make("Coal");
            Assert.AreEqual(StationIds.GetResourceStationId("Coal"), s.StationId);
            Assert.AreEqual("Coal", s.PeekNextItem().Id);
            Assert.IsTrue(s.TryProvideItem(out Item item));
            Assert.AreEqual("Coal", item.Id);
            Object.DestroyImmediate(s.gameObject);
        }

        [Test]
        public void SharedEntry_IsNotMutated_AndStationIdsDiffer()
        {
            var entry = new StationCatalogEntry { stationId = "Original" };
            var goA = new GameObject("ResA");
            goA.AddComponent<Rigidbody2D>();
            var a = goA.AddComponent<ResourceStationRuntime>();
            var goB = new GameObject("ResB");
            goB.AddComponent<Rigidbody2D>();
            var b = goB.AddComponent<ResourceStationRuntime>();
            a.Initialize(entry, "Coal");
            b.Initialize(entry, "Iron");

            Assert.AreEqual(StationIds.GetResourceStationId("Coal"), a.StationId);
            Assert.AreEqual(StationIds.GetResourceStationId("Iron"), b.StationId);
            Assert.AreNotEqual(a.StationId, b.StationId);
            Assert.AreEqual("Original", entry.stationId);
            Object.DestroyImmediate(goA);
            Object.DestroyImmediate(goB);
        }

        [Test]
        public void AcceptsOnlyOwnResourceId()
        {
            var s = Make("Coal");
            Assert.IsTrue(s.CanReceiveItem(new Item("Coal"), null));
            Assert.IsFalse(s.CanReceiveItem(new Item("Iron"), null));
            Assert.IsFalse(s.CanReceiveItem(null, null));
            Object.DestroyImmediate(s.gameObject);
        }

        [Test]
        public void QueuedConsumer_RespectsFifo()
        {
            var s = Make("Coal");
            var first = MakeConsumer();
            var second = MakeConsumer();
            s.PingPullIntent(first, 0f);
            s.PingPullIntent(second, 0f);

            Assert.IsFalse(s.TryProvideItem(out _, second));
            Assert.IsTrue(s.TryProvideItem(out _, first));
            Assert.IsTrue(s.TryProvideItem(out _, second), "Served ticket removed, second is now first.");
            Object.DestroyImmediate(s.gameObject);
            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }
    }
}
