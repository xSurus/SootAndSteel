using NUnit.Framework;
using UnityEngine;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    public class ComponentResourceStationTests
    {
        private static ComponentResourceStationRuntime Make(string componentId)
        {
            var go = new GameObject("CompRes");
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<ComponentResourceStationRuntime>();
            s.InitializeComponent(new StationCatalogEntry(), componentId);
            return s;
        }

        [Test]
        public void StationId_IsResourcePlusComponentPrefix_AndRoundTrips()
        {
            var s = Make(ComponentIds.BasicCasing);
            // Src: ResourceStation prefixes "Resource" to "Component<id>".
            Assert.AreEqual("ResourceComponentBasicCasing", s.StationId);
            Assert.IsTrue(StationIds.IsComponentStationId(s.StationId));
            Assert.AreEqual(ComponentIds.BasicCasing, StationIds.GetStationComponentId(s.StationId));
            Assert.AreEqual(ComponentIds.BasicCasing, s.ComponentId);
            Object.DestroyImmediate(s.gameObject);
        }

        [Test]
        public void BaseInitialize_WithComponentResourceId_YieldsComponentId()
        {
            var go = new GameObject("CompRes");
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<ComponentResourceStationRuntime>();
            s.Initialize(new StationCatalogEntry(), StationIds.GetComponentResourceId(ComponentIds.BasicCasing));
            Assert.AreEqual(ComponentIds.BasicCasing, s.ComponentId);
            Assert.AreEqual("ResourceComponentBasicCasing", s.StationId);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void QueuedConsumer_RespectsFifo_AndTicketRemoved()
        {
            var s = Make(ComponentIds.BasicCasing);
            var goA = new GameObject("A");
            goA.AddComponent<Rigidbody2D>();
            var first = goA.AddComponent<CounterRuntime>();
            first.Initialize(new StationCatalogEntry { stationId = StationIds.Counter });
            var goB = new GameObject("B");
            goB.AddComponent<Rigidbody2D>();
            var second = goB.AddComponent<CounterRuntime>();
            second.Initialize(new StationCatalogEntry { stationId = StationIds.Counter });
            s.PingPullIntent(first, 0f);
            s.PingPullIntent(second, 0f);
            Assert.IsFalse(s.TryProvideItem(out _, second));
            Assert.IsTrue(s.TryProvideItem(out _, first));
            Assert.IsTrue(s.TryProvideItem(out _, second));
            Object.DestroyImmediate(s.gameObject);
            Object.DestroyImmediate(goA);
            Object.DestroyImmediate(goB);
        }

        [Test]
        public void ProvidesAndPeeksSingleComponentBullet()
        {
            var s = Make(ComponentIds.HomingPropellant);
            var peek = (BulletItem)s.PeekNextItem();
            CollectionAssert.AreEqual(new[] { ComponentIds.HomingPropellant }, peek.ComponentIds);
            Assert.IsTrue(s.TryProvideItem(out Item item));
            CollectionAssert.AreEqual(new[] { ComponentIds.HomingPropellant }, ((BulletItem)item).ComponentIds);
            Object.DestroyImmediate(s.gameObject);
        }

        [Test]
        public void AcceptsOnlyExactlyItsOwnComponent()
        {
            var s = Make(ComponentIds.BasicCasing);
            Assert.IsTrue(s.CanReceiveItem(new BulletItem(ComponentIds.BasicCasing), null));
            Assert.IsFalse(s.CanReceiveItem(new BulletItem(ComponentIds.ScatterCasing), null));
            Assert.IsFalse(s.CanReceiveItem(new BulletItem(new BulletItem(ComponentIds.BasicCasing), new BulletItem(ComponentIds.ScatterCasing)), null));
            Assert.IsFalse(s.CanReceiveItem(new Item("Coal"), null));
            Assert.IsFalse(s.CanReceiveItem(null, null));
            Object.DestroyImmediate(s.gameObject);
        }
    }
}
