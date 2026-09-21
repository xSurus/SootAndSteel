using System.Numerics;
using Gamelab.Data;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Services.Shop
{
    /// <summary>
    /// Data shape of Src BuyableStationWrapper without physics or a wrapped station. The world wave
    /// sets IsVisible (Src IsHighlighted) and spawns the real station on purchase.
    /// </summary>
    public sealed class BuyableOffer : ITooltipable
    {
        public BuyableOffer(string stationKindId, Vector2 position, IShopService shop, StationRegistry stations)
        {
            StationKindId = stationKindId;
            Position = position;
            CatalogItem item = shop.GetCatalogItem(stationKindId);
            Cost = item?.Price ?? 0;
            Title = item != null ? item.Name : stationKindId;
            Description = item?.Description ?? string.Empty;
            CategoryName = StationTooltipInfo.CategoryName(stationKindId, stations);
            FunctionalityName = StationTooltipInfo.FunctionalityName(stationKindId, stations);
            IconSourceRect = StationTooltipInfo.IconSourceRect(stationKindId);
        }

        public string StationKindId { get; }
        public int Cost { get; }
        public string Title { get; }
        public string Description { get; }
        public Vector2 Position { get; }
        public bool IsVisible { get; set; }
        public string CategoryName { get; }
        public string FunctionalityName { get; }
        public SpriteRect? IconSourceRect { get; }

        int? ITooltipable.Cost => Cost;

        public string GetTitle() => Title;
        public string GetDescription() => Description;
    }
}
