using System.Collections.Generic;
using System.Numerics;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.UI;
using NUnit.Framework;

namespace Gamelab.Tests.Shop
{
    public class TooltipTests
    {
        private sealed class Item : ITooltipable
        {
            public Vector2 Position => Vector2.Zero;
            public bool IsVisible => true;
            public int? Cost { get; set; }
            public string FunctionalityName { get; set; }
            public string CategoryName { get; set; }
            public string GetTitle() => "T";
            public string GetDescription() => "D";
        }

        private static void Check(ButtonSpec s, bool visible, ButtonIcon icon, string label)
        {
            Assert.AreEqual(visible, s.Visible);
            Assert.AreEqual(icon, s.Icon);
            Assert.AreEqual(label, s.Label);
        }

        [Test]
        public void Interactions_Table()
        {
            var (l, r) = TooltipInteractions.For(TooltipKind.None);
            Assert.IsFalse(l.Visible);
            Assert.IsFalse(r.Visible);
            (l, r) = TooltipInteractions.For(TooltipKind.Buyable, 45);
            Check(l, true, ButtonIcon.Coin, "45"); Check(r, true, ButtonIcon.X, "Buy");
            (l, r) = TooltipInteractions.For(TooltipKind.SpeedLever);
            Assert.IsFalse(l.Visible); Check(r, true, ButtonIcon.X, "Speed");
            (l, r) = TooltipInteractions.For(TooltipKind.Counter);
            Assert.IsFalse(l.Visible); Check(r, true, ButtonIcon.A, "Swap");
            (l, r) = TooltipInteractions.For(TooltipKind.ResourceStation);
            Assert.IsFalse(l.Visible); Check(r, true, ButtonIcon.A, "Take");
            (l, r) = TooltipInteractions.For(TooltipKind.CannonSlot);
            Check(l, true, ButtonIcon.Y, "Sit"); Check(r, true, ButtonIcon.X, "Fire");
            foreach (var k in new[] { TooltipKind.Workbench, TooltipKind.CraftingStation })
            {
                (l, r) = TooltipInteractions.For(k);
                Check(l, true, ButtonIcon.X, "Interact"); Check(r, true, ButtonIcon.A, "Place");
            }
            (l, r) = TooltipInteractions.For(TooltipKind.Station);
            Check(l, true, ButtonIcon.X, "Use"); Check(r, true, ButtonIcon.A, "Grab");
        }

        [Test]
        public void KindForStationId_CoversRegistry()
        {
            var reg = StationConfigTable.Default();
            var expected = new Dictionary<string, TooltipKind>
            {
                { "Cannon", TooltipKind.Station },
                { "Workbench", TooltipKind.Workbench },
                { "AutoWorkbench", TooltipKind.Workbench },
                { "SpeedLever", TooltipKind.SpeedLever },
                { "Counter", TooltipKind.Counter },
                { "ResourceCoal", TooltipKind.ResourceStation },
                { "BulletRack", TooltipKind.Station },
                { "Conveyor", TooltipKind.Station },
                { "BulletConveyor", TooltipKind.Station },
                { "UpgradedComponentConveyor", TooltipKind.Station },
            };
            foreach (var kvp in reg.GetAll())
                Assert.AreEqual(expected[kvp.Key], TooltipInteractions.KindForStationId(kvp.Key, reg), kvp.Key);
            Assert.AreEqual(TooltipKind.ResourceStation,
                TooltipInteractions.KindForStationId("ResourceComponentHomingPropellant", reg));
        }

        [Test]
        public void KindForStationId_CraftingItemType()
        {
            var reg = new StationRegistry(new[]
            {
                new KeyValuePair<string, StationConfig>("Forge", new StationConfig { ItemType = EItemType.Crafting })
            });
            Assert.AreEqual(TooltipKind.CraftingStation, TooltipInteractions.KindForStationId("Forge", reg));
        }

        [Test]
        public void Model_CopiesContent()
        {
            var m = TooltipModel.Create(new Item { Cost = 30, FunctionalityName = "Utility", CategoryName = "Station" },
                TooltipKind.Buyable, 100);
            Assert.AreEqual("T", m.Title);
            Assert.AreEqual("D", m.Description);
            Assert.AreEqual("- Utility -", m.Functionality);
            Assert.AreEqual("Station", m.CategoryName);
            Assert.AreEqual(30, m.Cost);
            Assert.AreEqual("30", m.Left.Label);
            Assert.AreEqual("Buy", m.Right.Label);
            Assert.AreEqual("", TooltipModel.Create(new Item(), TooltipKind.None, 0).Functionality);
        }

        [Test]
        public void Model_AffordabilityFlipsAndRaisesChangedOnlyOnFlip()
        {
            var m = TooltipModel.Create(new Item { Cost = 30 }, TooltipKind.Buyable, 10);
            Assert.IsFalse(m.LeftCanAfford);
            int n = 0;
            m.Changed += () => n++;
            m.Refresh(20);
            Assert.AreEqual(0, n);
            m.Refresh(30);
            Assert.IsTrue(m.LeftCanAfford);
            Assert.AreEqual(1, n);
            m.Refresh(99);
            Assert.AreEqual(1, n);
            m.Refresh(0);
            Assert.IsFalse(m.LeftCanAfford);
            Assert.AreEqual(2, n);
        }

        [Test]
        public void Model_NoCostIsAlwaysAffordable()
        {
            var m = TooltipModel.Create(new Item(), TooltipKind.Station, 0);
            Assert.IsTrue(m.LeftCanAfford);
            m.Refresh(0);
            Assert.IsTrue(m.LeftCanAfford);
        }
    }

    public class TooltipLayoutTests
    {
        private const float Tol = 1e-3f;

        [Test]
        public void Ideal_PlacesAboveWhenItFits()
        {
            var r = TooltipLayout.Ideal(new Vector2(960, 540), 370, 266);
            Assert.AreEqual(775f, r.X, Tol);
            Assert.AreEqual(209f, r.Y, Tol);
            Assert.AreEqual(370f, r.W, Tol);
            Assert.AreEqual(266f, r.H, Tol);
        }

        [Test]
        public void Ideal_FallsBackBelow()
        {
            Assert.AreEqual(265f, TooltipLayout.Ideal(new Vector2(960, 200), 370, 266).Y, Tol);
        }

        [Test]
        public void Ideal_ClampsWhenNeitherFits()
        {
            Assert.AreEqual(14f, TooltipLayout.Ideal(new Vector2(960, 540), 370, 1000).Y, Tol);
        }

        [Test]
        public void ClampToCanvas_BothXEdgesAndY()
        {
            var rects = new List<TooltipRect>
            {
                TooltipLayout.Ideal(new Vector2(0, 540), 370, 266),
                TooltipLayout.Ideal(new Vector2(1920, 540), 370, 266),
                new TooltipRect(500, 2000, 370, 266),
            };
            TooltipLayout.ClampToCanvas(rects);
            Assert.AreEqual(4f, rects[0].X, Tol);
            Assert.AreEqual(1546f, rects[1].X, Tol);
            Assert.AreEqual(1080f - 266f - 14f, rects[2].Y, Tol);
        }

        [Test]
        public void ResolveOverlaps_VerticalPush()
        {
            var a = new TooltipRect(100, 100, 100, 200);
            var b = new TooltipRect(100, 150, 100, 200);
            TooltipLayout.ResolveOverlaps(new List<TooltipRect> { a, b });
            Assert.AreEqual(20f, a.Y, Tol);
            Assert.AreEqual(230f, b.Y, Tol);
            Assert.AreEqual(100f, a.X, Tol);
            Assert.AreEqual(100f, b.X, Tol);
        }

        [Test]
        public void ResolveOverlaps_HorizontalPush()
        {
            var a = new TooltipRect(100, 100, 200, 100);
            var b = new TooltipRect(150, 100, 200, 100);
            TooltipLayout.ResolveOverlaps(new List<TooltipRect> { a, b });
            Assert.AreEqual(20f, a.X, Tol);
            Assert.AreEqual(230f, b.X, Tol);
            Assert.AreEqual(100f, a.Y, Tol);
        }

        [Test]
        public void ResolveOverlaps_LeavesSeparatedAndSingleRectsAlone()
        {
            var a = new TooltipRect(0, 0, 100, 100);
            var b = new TooltipRect(200, 0, 100, 100);
            TooltipLayout.ResolveOverlaps(new List<TooltipRect> { a, b });
            Assert.AreEqual(0f, a.X, Tol);
            Assert.AreEqual(200f, b.X, Tol);
            var c = new TooltipRect(5, 5, 100, 100);
            TooltipLayout.ResolveOverlaps(new List<TooltipRect> { c });
            Assert.AreEqual(5f, c.X, Tol);
        }
    }
}
