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
            s.Initialize(new StationCatalogEntry(), componentId);
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
