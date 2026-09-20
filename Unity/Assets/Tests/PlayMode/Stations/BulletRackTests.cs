using NUnit.Framework;
using UnityEngine;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    public class BulletRackTests
    {
        private static BulletRackRuntime Make()
        {
            var go = new GameObject("Rack");
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<BulletRackRuntime>();
            s.Initialize(new StationCatalogEntry());
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

        // A finished bullet: three distinct component types, Type == Bullet.
        private static BulletItem Finished() => new BulletItem(
            new BulletItem(ComponentIds.BasicCasing),
            new BulletItem(ComponentIds.BasicProjectile),
            new BulletItem(ComponentIds.BasicPropellant));

        [Test]
        public void DefaultsStationId_AndStartsEmpty()
        {
            var r = Make();
            Assert.AreEqual(StationIds.BulletRack, r.StationId);
            Assert.IsNull(r.PeekNextItem());
            Assert.IsFalse(r.CanProvideItem(null));
            Object.DestroyImmediate(r.gameObject);
        }

        [Test]
        public void AcceptsOnlyFinishedBullets_UpToCapacity()
        {
            var r = Make();
            Assert.IsFalse(r.CanReceiveItem(new BulletItem(ComponentIds.BasicCasing), null));
            Assert.IsFalse(r.CanReceiveItem(new Item("Coal"), null));
            for (int i = 0; i < BulletRackRuntime.MaxCapacity; i++)
            {
                Assert.IsTrue(r.CanReceiveItem(Finished(), null));
                r.ReceiveItem(Finished(), null);
            }
            Assert.IsFalse(r.CanReceiveItem(Finished(), null));
            Object.DestroyImmediate(r.gameObject);
        }

        [Test]
        public void ProvidesOldestFirst_RemovingIt()
        {
            var r = Make();
            var a = Finished();
            var b = Finished();
            r.ReceiveItem(a, null);
            r.ReceiveItem(b, null);
            Assert.AreSame(a, r.PeekNextItem());
            Assert.IsTrue(r.TryProvideItem(out Item first));
            Assert.AreSame(a, first);
            Assert.IsTrue(r.TryProvideItem(out Item second));
            Assert.AreSame(b, second);
            Assert.IsFalse(r.TryProvideItem(out _));
            Object.DestroyImmediate(r.gameObject);
        }

        [Test]
        public void EmptyRack_RefusesFirstInLineConsumer()
        {
            var r = Make();
            var c = MakeConsumer();
            r.PingPullIntent(c, 0f);
            Assert.IsFalse(r.CanProvideItem(c));
            Assert.IsFalse(r.TryProvideItem(out Item item, c));
            Assert.IsNull(item);
            Object.DestroyImmediate(r.gameObject);
            Object.DestroyImmediate(c.gameObject);
        }

        [Test]
        public void QueuedConsumer_RespectsFifo_AndTicketRemoved()
        {
            var r = Make();
            r.ReceiveItem(Finished(), null);
            r.ReceiveItem(Finished(), null);
            var first = MakeConsumer();
            var second = MakeConsumer();
            r.PingPullIntent(first, 0f);
            r.PingPullIntent(second, 0f);
            Assert.IsFalse(r.TryProvideItem(out _, second));
            Assert.IsTrue(r.TryProvideItem(out _, first));
            Assert.IsTrue(r.TryProvideItem(out _, second));
            Object.DestroyImmediate(r.gameObject);
            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }
    }
}
