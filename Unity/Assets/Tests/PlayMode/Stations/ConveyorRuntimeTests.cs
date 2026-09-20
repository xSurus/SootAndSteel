using System.Collections.Generic;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Utils;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.Stations
{
    public class ConveyorRuntimeTests
    {
        private class FakePlayer : IPlayerActor, IItemProvider, IItemReceiver
        {
            public Item PeekNextItem() => null;
            public bool TryProvideItem(out Item item, IItemReceiver consumer = null) { item = null; return false; }
            public bool CanProvideItem(IItemReceiver consumer) => false;
            public bool CanReceiveItem(Item item, IItemProvider source) => true;
            public void ReceiveItem(Item item, IItemProvider source) { }
        }

        // Plain box: takes one item when Accepts is true, gives it back on request.
        private class BoxStation : StationRuntime
        {
            public bool Accepts = true;
            public override bool CanReceiveItem(Item item, IItemProvider source) => Accepts && HeldItem == null;
        }

        private class FakeGrid : IStationGrid
        {
            public readonly Dictionary<(Vector2, GridDirection), StationRuntime> Map =
                new Dictionary<(Vector2, GridDirection), StationRuntime>();

            public StationRuntime GetAdjacentStation(Vector2 position, GridDirection direction) =>
                Map.TryGetValue((position, direction), out StationRuntime s) ? s : null;
        }

        private readonly List<GameObject> spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in spawned) Object.DestroyImmediate(go);
            spawned.Clear();
        }

        private T Make<T>(Vector2 pos, string stationId = "") where T : StationRuntime
        {
            var go = new GameObject("S");
            spawned.Add(go);
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<T>();
            s.Initialize(new StationCatalogEntry { stationId = stationId });
            s.Position = pos;
            return s;
        }

        private static BulletItem B(string id) => new BulletItem(id);

        private static BulletItem FullBullet() =>
            new BulletItem(B(ComponentIds.BasicCasing), B(ComponentIds.BasicProjectile), B(ComponentIds.BasicPropellant));

        private static BulletItem UpgradedCasing() =>
            new BulletItem(B(ComponentIds.BasicCasing), B(ComponentIds.ScatterCasing));

        // Conveyor at (0,0) facing Right with a box on each side.
        private (ConveyorRuntime c, BoxStation src, BoxStation sink) Line()
        {
            var grid = new FakeGrid();
            var c = Make<ConveyorRuntime>(Vector2.zero);
            var src = Make<BoxStation>(new Vector2(-1, 0));
            var sink = Make<BoxStation>(new Vector2(1, 0));
            grid.Map[(c.Position, GridDirection.Left)] = src;
            grid.Map[(c.Position, GridDirection.Right)] = sink;
            c.Grid = grid;
            c.FacingDirection = GridDirection.Right;
            return (c, src, sink);
        }

        [Test]
        public void StationId_DefaultsPerSubclass()
        {
            Assert.AreEqual(StationIds.Conveyor, Make<ConveyorRuntime>(Vector2.zero).StationId);
            Assert.AreEqual(StationIds.BulletConveyor, Make<BulletConveyorRuntime>(Vector2.zero).StationId);
            Assert.AreEqual(StationIds.UpgradedComponentConveyor,
                Make<UpgradedComponentConveyorRuntime>(Vector2.zero).StationId);
        }

        [Test]
        public void OnInteract_ThroughInterface_RotatesClockwise()
        {
            var c = Make<ConveyorRuntime>(Vector2.zero);
            c.FacingDirection = GridDirection.Right;
            IInteractable i = c;
            i.OnInteract(null);
            Assert.AreEqual(GridDirection.Down, c.FacingDirection);
            i.OnInteract(null);
            Assert.AreEqual(GridDirection.Left, c.FacingDirection);
        }

        [Test]
        public void PlayerReceive_SkipsToHalf_AndPeekIsAvailable()
        {
            var c = Make<ConveyorRuntime>(Vector2.zero);
            Assert.IsNull(c.PeekNextItem());
            c.ReceiveItem(new Item("Coal"), new FakePlayer());
            Assert.AreEqual(ConveyorRuntime.TransportDuration / 2f, c.TransportTimer);
            Assert.AreEqual("Coal", c.PeekNextItem().Id);
        }

        [Test]
        public void ItemTravelsFromSourceToSink()
        {
            var (c, src, sink) = Line();
            src.HeldItem = new Item("Coal");

            c.Update(0.5f); // pulls, timer stays 0
            Assert.IsNull(src.HeldItem);
            Assert.AreEqual("Coal", c.HeldItem.Id);
            Assert.AreEqual(0f, c.TransportTimer);

            for (int i = 0; i < 6; i++) c.Update(0.5f); // 0 -> 3 s
            Assert.AreEqual(ConveyorRuntime.TransportDuration, c.TransportTimer, 1e-5f);
            Assert.IsNull(sink.HeldItem);

            c.Update(0.5f); // hand-off
            Assert.AreEqual("Coal", sink.HeldItem.Id);
            Assert.IsNull(c.HeldItem);
            Assert.AreEqual(0f, c.TransportTimer);
        }

        [Test]
        public void StallsAtHalf_WhenSinkNotReady_ThenContinues()
        {
            var (c, src, sink) = Line();
            sink.Accepts = false;
            src.HeldItem = new Item("Coal");
            c.Update(0.5f);
            for (int i = 0; i < 10; i++) c.Update(0.5f);
            Assert.AreEqual(ConveyorRuntime.TransportDuration / 2f, c.TransportTimer, 1e-5f);

            sink.Accepts = true;
            for (int i = 0; i < 3; i++) c.Update(0.5f);
            Assert.AreEqual(ConveyorRuntime.TransportDuration, c.TransportTimer, 1e-5f);
        }

        [Test]
        public void NoSinkOrNoGrid_HoldsAtHalf()
        {
            var c = Make<ConveyorRuntime>(Vector2.zero); // Grid == null
            c.ReceiveItem(new Item("Coal"), new FakePlayer());
            for (int i = 0; i < 10; i++) c.Update(0.5f);
            Assert.AreEqual(ConveyorRuntime.TransportDuration / 2f, c.TransportTimer, 1e-5f);
        }

        [Test]
        public void IsBeingHeld_FreezesUpdate()
        {
            var (c, src, _) = Line();
            src.HeldItem = new Item("Coal");
            c.IsBeingHeld = true;
            c.Update(0.5f);
            Assert.IsNull(c.HeldItem);
            Assert.IsNotNull(src.HeldItem);
        }

        [Test]
        public void ReceiveFromNonSource_SkipsToHalf()
        {
            var (c, _, _) = Line();
            var stranger = Make<BoxStation>(new Vector2(0, 5));
            c.ReceiveItem(new Item("Coal"), stranger);
            Assert.AreEqual(ConveyorRuntime.TransportDuration / 2f, c.TransportTimer);
        }

        [Test]
        public void Filters_PerSubclass()
        {
            var plain = Make<ConveyorRuntime>(Vector2.zero);
            var bullet = Make<BulletConveyorRuntime>(Vector2.zero);
            var upgraded = Make<UpgradedComponentConveyorRuntime>(Vector2.zero);
            var player = new FakePlayer();

            var coal = new Item("Coal");
            BulletItem full = FullBullet();
            BulletItem up = UpgradedCasing();
            BulletItem basic = B(ComponentIds.BasicCasing);
            BulletItem scatterOnly = B(ComponentIds.ScatterCasing);

            Assert.IsTrue(plain.CanReceiveItem(coal, player));
            Assert.IsTrue(plain.CanReceiveItem(full, player));
            Assert.IsFalse(plain.CanReceiveItem(null, player));

            Assert.IsTrue(bullet.CanReceiveItem(full, player));
            Assert.IsFalse(bullet.CanReceiveItem(up, player));
            Assert.IsFalse(bullet.CanReceiveItem(coal, player));

            Assert.IsTrue(upgraded.CanReceiveItem(up, player));
            Assert.IsFalse(upgraded.CanReceiveItem(full, player), "Type Bullet refused");
            Assert.IsFalse(upgraded.CanReceiveItem(basic, player), "No upgrade");
            Assert.IsFalse(upgraded.CanReceiveItem(scatterOnly, player), "No basic");
            Assert.IsFalse(upgraded.CanReceiveItem(coal, player));
        }

        [Test]
        public void CanReceive_RefusesWhenHolding()
        {
            var c = Make<ConveyorRuntime>(Vector2.zero);
            c.HeldItem = new Item("Coal");
            Assert.IsFalse(c.CanReceiveItem(new Item("Coal"), new FakePlayer()));
        }

        [Test]
        public void CanReceive_ProviderQueueIsFifo_PlayerBypasses()
        {
            var c = Make<ConveyorRuntime>(Vector2.zero);
            var a = Make<BoxStation>(new Vector2(3, 0));
            var b = Make<BoxStation>(new Vector2(4, 0));
            var item = new Item("Coal");
            Assert.IsTrue(c.CanReceiveItem(item, b), "Empty queue accepts anyone");
            c.PingPushIntent(a, 0f);
            c.PingPushIntent(b, 0f);
            Assert.IsTrue(c.CanReceiveItem(item, a));
            Assert.IsFalse(c.CanReceiveItem(item, b));
            Assert.IsTrue(c.CanReceiveItem(item, new FakePlayer()));
        }

        [Test]
        public void DoesNotPullFromSameFacingConveyor_ButPullsFromOthers()
        {
            foreach (var (facing, expectPull) in new[]
                     {
                         (GridDirection.Right, false),
                         (GridDirection.Down, true),
                     })
            {
                var grid = new FakeGrid();
                var c = Make<ConveyorRuntime>(Vector2.zero);
                var srcConv = Make<ConveyorRuntime>(new Vector2(-1, 0));
                grid.Map[(c.Position, GridDirection.Left)] = srcConv;
                c.Grid = grid;
                c.FacingDirection = GridDirection.Right;
                srcConv.FacingDirection = facing;
                srcConv.ReceiveItem(new Item("Coal"), new FakePlayer()); // at half, peekable

                c.Update(0.1f);
                Assert.AreEqual(expectPull, c.HeldItem != null, "source facing " + facing);
            }
        }

        [Test]
        public void PlayerConsumer_BypassesConsumerQueueOrder()
        {
            var c = Make<ConveyorRuntime>(Vector2.zero);
            var other = Make<BoxStation>(new Vector2(2, 0));
            c.ReceiveItem(new Item("Coal"), new FakePlayer());
            c.PingPullIntent(other, 0f);
            Assert.IsFalse(c.CanProvideItem(Make<BoxStation>(new Vector2(3, 0))));
            Assert.IsTrue(c.CanProvideItem(new FakePlayer()));
        }
    }
}
