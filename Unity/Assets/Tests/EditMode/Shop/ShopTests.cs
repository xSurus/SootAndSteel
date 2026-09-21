using System;
using System.Linq;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Run;
using Gamelab.Services.Shop;
using Gamelab.UI;
using NUnit.Framework;

namespace Gamelab.Tests.Shop
{
    public class ShopTests
    {
        private StationRegistry stations;
        private ComponentRegistry components;
        private ShopManager shop;

        [SetUp]
        public void SetUp()
        {
            stations = StationConfigTable.Default();
            components = ComponentConfigTable.Default();
            shop = new ShopManager(stations, components);
        }

        // Produced by a python script over Src/Content/Data/{Station,Component}Config.json.
        private static readonly (string id, string name, int price)[] Golden =
        {
            ("AutoWorkbench", "Automatic Anvil", 45),
            ("Conveyor", "Conveyor", 20),
            ("BulletConveyor", "Bullet Conveyor", 30),
            ("UpgradedComponentConveyor", "Component Conveyor", 30),
            ("ResourceComponentScatterCasing", "Scatter Casing", 10),
            ("ResourceComponentHomingPropellant", "Homing Propellant", 10),
            ("ResourceComponentFrangibleProjectile", "Frangible Projectile", 10),
            ("ResourceComponentHeavyPropellant", "Heavy Propellant", 10),
            ("ResourceComponentPiercingProjectile", "Piercing Projectile", 10),
            ("ResourceComponentBurstCasing", "Burst Casing", 10),
            ("ResourceComponentBoomerangPropellant", "BOOMerang", 10),
            ("ResourceComponentRapidFireCasing", "Rapid Fire Casing", 10),
            ("ResourceComponentMatryoshkaProjectile", "Matryoshka Projectile", 10),
        };

        [Test]
        public void Catalog_MatchesGoldenOrderNamesAndPrices()
        {
            var actual = shop.GenerateCatalog().Select(c => (c.ItemId, c.Name, c.Price)).ToArray();
            CollectionAssert.AreEqual(Golden, actual);
        }

        [Test]
        public void GetCatalogItem_HitAndMiss()
        {
            var hit = shop.GetCatalogItem("Conveyor");
            Assert.AreEqual(20, hit.Price);
            Assert.IsNull(shop.GetCatalogItem("Cannon"));
            Assert.IsNull(shop.GetCatalogItem("nope"));
        }

        [Test]
        public void StationTable_HasAllTenEntriesInFileOrder()
        {
            var ids = stations.GetAll().Select(kvp => kvp.Key).ToArray();
            CollectionAssert.AreEqual(new[]
            {
                "Cannon", "Workbench", "AutoWorkbench", "SpeedLever", "Counter",
                "ResourceCoal", "BulletRack", "Conveyor", "BulletConveyor", "UpgradedComponentConveyor",
            }, ids);

            var cannon = stations.Get("Cannon");
            Assert.AreEqual("Load ammo with A, then Interact (X) to fire.", cannon.Description);
            Assert.AreEqual(EItemType.Utility, cannon.ItemType);
            Assert.IsFalse(cannon.AppearsInShop);
            Assert.AreEqual(0, cannon.ShopPrice);

            var workbench = stations.Get("Workbench");
            Assert.AreEqual("Place ingredients with A.\nHold Interact (X) to craft or repair.", workbench.Description);
            Assert.IsFalse(workbench.AppearsInShop);
            Assert.AreEqual(38, workbench.ShopPrice);

            Assert.AreEqual(EItemType.Storage, stations.Get("Counter").ItemType);
            Assert.AreEqual(EItemType.Resource, stations.Get("ResourceCoal").ItemType);
            Assert.IsNull(stations.Get("Missing"));
        }

        [Test]
        public void ComponentTable_HasAllEntriesAndUnknownThrows()
        {
            Assert.AreEqual(13, components.GetAll().Count());
            Assert.AreEqual("BasicProjectile", components.GetAll().First().Key);
            var basic = components.Get("BasicProjectile");
            Assert.AreEqual("Basic Projectile", basic.Name);
            Assert.IsFalse(basic.AppearsInShop);
            Assert.AreEqual(18, basic.ShopPrice);
            Assert.IsTrue(components.Get("ScatterCasing").AppearsInShop);
            Assert.Throws<Exception>(() => components.Get("Nope"));
        }

        [Test]
        public void RunCredits_Rules()
        {
            var run = new RunCredits();
            int changes = 0;
            run.Changed += () => changes++;
            Assert.IsTrue(run.HubScreenCraftingHelpVisible);

            run.AddCredits(0);
            run.AddCredits(-5);
            Assert.AreEqual(0, run.Credits);
            Assert.AreEqual(0, changes);

            run.AddCredits(30);
            Assert.AreEqual(30, run.Credits);
            Assert.AreEqual(1, changes);

            Assert.IsFalse(run.TrySpendCredits(-1));
            Assert.IsFalse(run.TrySpendCredits(31));
            Assert.AreEqual(30, run.Credits);
            Assert.AreEqual(1, changes);

            Assert.IsTrue(run.TrySpendCredits(30));
            Assert.AreEqual(0, run.Credits);
            Assert.AreEqual(2, changes);

            run.RegisterUpgradePurchased();
            Assert.AreEqual(1, run.TotalUpgradesBought);
        }

        [Test]
        public void Tooltip_Cannon()
        {
            Assert.AreEqual("Cannon", StationTooltipInfo.GetTitle("Cannon", stations, components));
            Assert.AreEqual("Load ammo with A, then Interact (X) to fire.", StationTooltipInfo.GetDescription("Cannon", stations, components));
            Assert.AreEqual("Station", StationTooltipInfo.CategoryName("Cannon", stations));
            Assert.AreEqual("Utility", StationTooltipInfo.FunctionalityName("Cannon", stations));
            Assert.IsNull(StationTooltipInfo.IconSourceRect("Cannon"));
        }

        [Test]
        public void Tooltip_ResourceCoal()
        {
            Assert.AreEqual("Coal Box", StationTooltipInfo.GetTitle("ResourceCoal", stations, components));
            Assert.AreEqual("Station", StationTooltipInfo.CategoryName("ResourceCoal", stations));
            Assert.AreEqual("Resource", StationTooltipInfo.FunctionalityName("ResourceCoal", stations));
            Assert.IsNull(StationTooltipInfo.IconSourceRect("ResourceCoal"));
        }

        [Test]
        public void Tooltip_UpgradingComponent()
        {
            const string id = "ResourceComponentHomingPropellant";
            Assert.AreEqual("Homing Propellant", StationTooltipInfo.GetTitle(id, stations, components));
            StringAssert.StartsWith("A propellant that allows", StationTooltipInfo.GetDescription(id, stations, components));
            Assert.AreEqual("Propellant", StationTooltipInfo.CategoryName(id, stations));
            Assert.AreEqual("Upgrading", StationTooltipInfo.FunctionalityName(id, stations));
            AssertRect(StationTooltipInfo.IconSourceRect(id), 132, 803, 24, 24);
        }

        [Test]
        public void Tooltip_BasicComponent()
        {
            const string id = "ResourceComponentBasicCasing";
            Assert.AreEqual("Basic Casing", StationTooltipInfo.GetTitle(id, stations, components));
            Assert.AreEqual("Casing", StationTooltipInfo.CategoryName(id, stations));
            Assert.AreEqual("Basic", StationTooltipInfo.FunctionalityName(id, stations));
            AssertRect(StationTooltipInfo.IconSourceRect(id), 36, 227, 24, 24);
        }

        [Test]
        public void Tooltip_UnknownId()
        {
            Assert.AreEqual("Mystery", StationTooltipInfo.GetTitle("Mystery", stations, components));
            Assert.AreEqual("", StationTooltipInfo.GetDescription("Mystery", stations, components));
            Assert.IsNull(StationTooltipInfo.CategoryName("Mystery", stations));
            Assert.IsNull(StationTooltipInfo.FunctionalityName("Mystery", stations));
            Assert.IsNull(StationTooltipInfo.IconSourceRect("Mystery"));
        }

        [Test]
        public void ShopItemIconAtlas_Rects()
        {
            AssertRect(ShopItemIconAtlas.TryGetRect(EComponentType.Projectile, EItemType.Basic), 36, 517, 24, 24);
            AssertRect(ShopItemIconAtlas.TryGetRect(EComponentType.Propellant, EItemType.Basic), 36, 803, 24, 24);
            AssertRect(ShopItemIconAtlas.TryGetRect(EComponentType.Casing, EItemType.Upgrading), 132, 227, 24, 24);
            Assert.IsNull(ShopItemIconAtlas.TryGetRect(EComponentType.Casing, EItemType.Utility));
            Assert.IsNull(ShopItemIconAtlas.TryGetRect(EComponentType.Bullet, EItemType.Basic));
            Assert.IsNull(ShopItemIconAtlas.TryGetRect(null, EItemType.Basic));
            Assert.AreEqual("Storage", ShopItemIconAtlas.GetItemTypeName(EItemType.Storage));
            Assert.AreEqual("Projectile", ShopItemIconAtlas.GetComponentTypeName(EComponentType.Projectile));
            Assert.IsNull(ShopItemIconAtlas.GetComponentTypeName(EComponentType.Bullet));
        }

        [Test]
        public void XboxButtonAtlas_Rects()
        {
            var a = XboxButtonAtlas.GetSourceRect(XboxButtonAtlas.Face.A);
            Assert.AreEqual((0, 0, 32, 32), (a.X, a.Y, a.Width, a.Height));
            var rt = XboxButtonAtlas.GetSourceRect(XboxButtonAtlas.Face.Rt);
            Assert.AreEqual((288, 0, 32, 32), (rt.X, rt.Y, rt.Width, rt.Height));
        }

        private static void AssertRect(Gamelab.PhysicalEntities.Interfaces.SpriteRect? r, int x, int y, int w, int h)
        {
            Assert.IsTrue(r.HasValue);
            Assert.AreEqual((x, y, w, h), (r.Value.X, r.Value.Y, r.Value.Width, r.Value.Height));
        }
    }
}
