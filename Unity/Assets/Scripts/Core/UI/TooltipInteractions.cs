using Gamelab.Items;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.UI
{
    public enum TooltipKind { None, Buyable, SpeedLever, Counter, ResourceStation, CannonSlot, Workbench, CraftingStation, Station }

    public enum ButtonIcon { None, Coin, A, X, Y }

    public readonly struct ButtonSpec
    {
        public ButtonSpec(bool visible, ButtonIcon icon, string label)
        {
            Visible = visible;
            Icon = icon;
            Label = label;
        }

        public bool Visible { get; }
        public ButtonIcon Icon { get; }
        public string Label { get; }
    }

    /// <summary>Port of Src WorldUiManager.ConfigureInteractionButtons.</summary>
    public static class TooltipInteractions
    {
        private static readonly ButtonSpec Hidden = new ButtonSpec(false, ButtonIcon.None, "");

        public static (ButtonSpec left, ButtonSpec right) For(TooltipKind kind, int cost = 0)
        {
            switch (kind)
            {
                case TooltipKind.Buyable:
                    return (new ButtonSpec(true, ButtonIcon.Coin, cost.ToString()), new ButtonSpec(true, ButtonIcon.X, "Buy"));
                case TooltipKind.SpeedLever:
                    return (Hidden, new ButtonSpec(true, ButtonIcon.X, "Speed"));
                case TooltipKind.Counter:
                    return (Hidden, new ButtonSpec(true, ButtonIcon.A, "Swap"));
                case TooltipKind.ResourceStation:
                    return (Hidden, new ButtonSpec(true, ButtonIcon.A, "Take"));
                case TooltipKind.CannonSlot:
                    return (new ButtonSpec(true, ButtonIcon.Y, "Sit"), new ButtonSpec(true, ButtonIcon.X, "Fire"));
                case TooltipKind.Workbench:
                case TooltipKind.CraftingStation:
                    return (new ButtonSpec(true, ButtonIcon.X, "Interact"), new ButtonSpec(true, ButtonIcon.A, "Place"));
                case TooltipKind.Station:
                    return (new ButtonSpec(true, ButtonIcon.X, "Use"), new ButtonSpec(true, ButtonIcon.A, "Grab"));
                default:
                    return (Hidden, Hidden);
            }
        }

        /// <summary>Src case order for stations. CannonSlot is not a station id, the caller passes it.</summary>
        public static TooltipKind KindForStationId(string id, StationRegistry stations)
        {
            if (id == StationIds.SpeedLever) return TooltipKind.SpeedLever;
            if (id == StationIds.Counter) return TooltipKind.Counter;
            if (StationIds.IsResourceStationId(id)) return TooltipKind.ResourceStation;
            if (id == StationIds.Workbench || id == StationIds.AutoWorkbench) return TooltipKind.Workbench;
            if (stations.Get(id)?.ItemType == EItemType.Crafting) return TooltipKind.CraftingStation;
            return TooltipKind.Station;
        }
    }
}
