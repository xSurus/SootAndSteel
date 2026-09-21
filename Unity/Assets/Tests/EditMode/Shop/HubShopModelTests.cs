using System.Linq;
using System.Numerics;
using Gamelab.Items.Bullets;
using Gamelab.Map;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Run;
using Gamelab.Services.Shop;
using NUnit.Framework;

namespace Gamelab.Tests.Shop
{
    public class HubShopModelTests
    {
        private StationRegistry stations;
        private ShopManager shop;
        private RunCredits credits;
        private HubMapModel map;
        private HubShopModel model;

        [SetUp]
        public void SetUp()
        {
            stations = StationConfigTable.Default();
            shop = new ShopManager(stations, ComponentConfigTable.Default());
            credits = new RunCredits();
            map = new HubMapModel(2160, 2160);
            model = new HubShopModel(shop, stations, credits, map);
        }

        private string[] Ids() => model.Offers.Select(o => o.StationKindId).ToArray();

        [Test]
        public void Restock_SameSeedSameOffers_DifferentLevelDiffers()
        {
            model.Restock(1000, 3);
            var first = Ids();
            model.Restock(1000, 3);
            CollectionAssert.AreEqual(first, Ids());
            model.Restock(1000, 4);
            CollectionAssert.AreNotEqual(first, Ids());
        }

        [Test]
        public void Restock_MatchesHubMapModelIndicesAndPositions()
        {
            model.Restock(777, 2);
            var catalog = shop.GenerateCatalog();
            var idx = HubMapModel.SelectOfferIndices(catalog.Count,
                new System.Random(HubMapModel.RestockSeed(777, 2)), 4);
            CollectionAssert.AreEqual(idx.Select(i => catalog[i].ItemId).ToArray(), Ids());
            var pos = map.OfferPositions(4);
            for (int i = 0; i < 4; i++) Assert.AreEqual(pos[i], model.Offers[i].Position);
        }

        // Produced by running Src's catalog.OrderBy(x => hubRandom.Next()).Take(4) over the 13 item catalog
        // with one real .NET System.Random(RestockSeed(seed, level)): (1000, 3) gives indices 2,8,3,1; (12345, 1) gives 8,11,0,12.
        [Test]
        public void Restock_Golden()
        {
            model.Restock(1000, 3);
            CollectionAssert.AreEqual(new[]
            {
                "BulletConveyor", "ResourceComponentPiercingProjectile", "UpgradedComponentConveyor", "Conveyor"
            }, Ids());
            model.Restock(12345, 1);
            CollectionAssert.AreEqual(new[]
            {
                "ResourceComponentPiercingProjectile", "ResourceComponentRapidFireCasing",
                "AutoWorkbench", "ResourceComponentMatryoshkaProjectile"
            }, Ids());
        }

        [Test]
        public void Restock_RaisesOffersChanged()
        {
            int n = 0;
            model.OffersChanged += () => n++;
            model.Restock(1, 1);
            Assert.AreEqual(1, n);
        }

        [Test]
        public void Purchase_WhenAffordable_SpendsRemovesAndRaises()
        {
            model.Restock(1000, 3);
            var offer = model.Offers[0];
            Assert.AreEqual(30, offer.Cost);
            credits.AddCredits(50);
            int bought = 0, changed = 0;
            BuyableOffer got = null;
            model.Purchased += o => { bought++; got = o; };
            model.OffersChanged += () => changed++;

            Assert.IsTrue(model.TryPurchase(offer));
            Assert.AreEqual(20, credits.Credits);
            Assert.AreEqual(1, credits.TotalUpgradesBought);
            Assert.AreEqual(3, model.Offers.Count);
            Assert.IsFalse(model.Offers.Contains(offer));
            Assert.AreEqual(1, bought);
            Assert.AreSame(offer, got);
            Assert.AreEqual(1, changed);
        }

        [Test]
        public void Purchase_WhenTooExpensive_ChangesNothing()
        {
            model.Restock(1000, 3);
            credits.AddCredits(29);
            int raised = 0;
            model.Purchased += _ => raised++;
            model.OffersChanged += () => raised++;
            Assert.IsFalse(model.TryPurchase(model.Offers[0]));
            Assert.AreEqual(29, credits.Credits);
            Assert.AreEqual(0, credits.TotalUpgradesBought);
            Assert.AreEqual(4, model.Offers.Count);
            Assert.AreEqual(0, raised);
        }

        [Test]
        public void Purchase_OfForeignOffer_IsFalse()
        {
            model.Restock(1000, 3);
            credits.AddCredits(100);
            var foreign = new BuyableOffer("Conveyor", Vector2.Zero, shop, stations);
            Assert.IsFalse(model.TryPurchase(foreign));
            Assert.AreEqual(100, credits.Credits);
        }

        [Test]
        public void Offer_TooltipableViewForStationKind()
        {
            var offer = new BuyableOffer("AutoWorkbench", new Vector2(3, 4), shop, stations);
            ITooltipable t = offer;
            Assert.AreEqual(45, t.Cost);
            Assert.AreEqual("Automatic Anvil", t.GetTitle());
            Assert.AreEqual(stations.Get("AutoWorkbench").Description, t.GetDescription());
            Assert.AreEqual("Station", t.CategoryName);
            Assert.AreEqual("Utility", t.FunctionalityName);
            Assert.IsNull(t.IconSourceRect);
            Assert.AreEqual(new Vector2(3, 4), t.Position);
            Assert.IsFalse(t.IsVisible);
            offer.IsVisible = true;
            Assert.IsTrue(t.IsVisible);
        }

        [Test]
        public void Offer_TooltipableViewForComponentKind()
        {
            const string id = "ResourceComponentHomingPropellant";
            ITooltipable t = new BuyableOffer(id, Vector2.Zero, shop, stations);
            Assert.AreEqual(10, t.Cost);
            Assert.AreEqual("Homing Propellant", t.GetTitle());
            Assert.AreEqual("Propellant", t.CategoryName);
            Assert.IsNotNull(t.FunctionalityName);
            Assert.IsNotNull(t.IconSourceRect);
        }

        [Test]
        public void Offer_UnknownKindIdHasCostZeroAndIdAsTitle()
        {
            ITooltipable t = new BuyableOffer("Nope", Vector2.Zero, shop, stations);
            Assert.AreEqual(0, t.Cost);
            Assert.AreEqual("Nope", t.GetTitle());
            Assert.AreEqual("", t.GetDescription());
        }
    }
}
