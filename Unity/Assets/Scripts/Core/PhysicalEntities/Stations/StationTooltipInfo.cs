using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.UI;

namespace Gamelab.PhysicalEntities.Stations
{
    // Tooltip text for a station id, derived the way Src AbstractStation and ComponentResourceStation do.
    public static class StationTooltipInfo
    {
        public static string GetTitle(string id, StationRegistry stations, ComponentRegistry components)
        {
            if (StationIds.IsComponentStationId(id))
                return components.Get(StationIds.GetStationComponentId(id)).Name ?? "Unknown Component";
            return stations.Get(id)?.Name ?? id;
        }

        public static string GetDescription(string id, StationRegistry stations, ComponentRegistry components)
        {
            if (StationIds.IsComponentStationId(id))
                return components.Get(StationIds.GetStationComponentId(id)).Description ?? "No description";
            return stations.Get(id)?.Description ?? "";
        }

        public static string CategoryName(string id, StationRegistry stations)
        {
            if (StationIds.IsComponentStationId(id))
                return ShopItemIconAtlas.GetComponentTypeName(ComponentTraits.TypeOf(StationIds.GetStationComponentId(id)));
            return stations.Get(id)?.ItemType == null ? null : ShopItemIconAtlas.StationCategoryName;
        }

        public static string FunctionalityName(string id, StationRegistry stations)
        {
            if (StationIds.IsComponentStationId(id))
                return ShopItemIconAtlas.GetItemTypeName(ComponentItemType(id));
            var itemType = stations.Get(id)?.ItemType;
            return itemType == null ? null : ShopItemIconAtlas.GetItemTypeName(itemType.Value);
        }

        public static SpriteRect? IconSourceRect(string id)
        {
            if (!StationIds.IsComponentStationId(id)) return null;
            return ShopItemIconAtlas.TryGetRect(ComponentTraits.TypeOf(StationIds.GetStationComponentId(id)), ComponentItemType(id));
        }

        private static EItemType ComponentItemType(string id)
        {
            return ComponentTraits.IsBasic(StationIds.GetStationComponentId(id)) ? EItemType.Basic : EItemType.Upgrading;
        }
    }
}
