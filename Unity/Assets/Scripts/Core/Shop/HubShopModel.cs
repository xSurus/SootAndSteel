using System;
using System.Collections.Generic;
using Gamelab.Map;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.Run;

namespace Gamelab.Services.Shop
{
    /// <summary>Hub shop state: the current offers, restocking and buying (Src HubMap.RestockHubDragOffers, BuyableStationWrapper.OnInteract).</summary>
    public sealed class HubShopModel
    {
        private readonly IShopService shop;
        private readonly StationRegistry stations;
        private readonly RunCredits credits;
        private readonly HubMapModel map;
        private readonly List<BuyableOffer> offers = new List<BuyableOffer>();

        public HubShopModel(IShopService shop, StationRegistry stations, RunCredits credits, HubMapModel map)
        {
            this.shop = shop;
            this.stations = stations;
            this.credits = credits;
            this.map = map;
        }

        public IReadOnlyList<BuyableOffer> Offers => offers;

        public event Action OffersChanged;

        /// <summary>Raised after a successful purchase. The world wave spawns the station, emits the buy particles, plays Sounds.Purchase and snaps the station to the train.</summary>
        public event Action<BuyableOffer> Purchased;

        public void Restock(int runSeed, int level, int offerCount = 4)
        {
            var catalog = shop.GenerateCatalog();
            var random = new System.Random(HubMapModel.RestockSeed(runSeed, level));
            int[] indices = HubMapModel.SelectOfferIndices(catalog.Count, random, offerCount);
            // Src spreads by offerCount even when fewer offers are selected.
            var positions = map.OfferPositions(offerCount);
            offers.Clear();
            for (int i = 0; i < indices.Length; i++)
                offers.Add(new BuyableOffer(catalog[indices[i]].ItemId, positions[i], shop, stations));
            OffersChanged?.Invoke();
        }

        public bool TryPurchase(BuyableOffer offer)
        {
            if (!offers.Contains(offer)) return false;
            if (!credits.TrySpendCredits(offer.Cost)) return false;
            credits.RegisterUpgradePurchased();
            offers.Remove(offer);
            Purchased?.Invoke(offer);
            OffersChanged?.Invoke();
            return true;
        }
    }
}
