using Gamelab.Items;
using Gamelab.Items.Bullets;
using Microsoft.Xna.Framework;

namespace Gamelab.UI;

public static class ShopItemIconAtlas
{
    public const string SheetFile = "BulletComponentSpriteSheet.png";

    private const int IconSize = 24;

    // Column X offsets for the 24-px-row icons. Cells are 96 px wide and the 24-px icon
    // sits centred horizontally → top-left of the crop is at col_origin + 36.
    private const int BasicColumnX = 36; // col 0 — plain icon
    private const int UpgradingColumnX = 132; // col 1 — grey ↑ badge
    // private const int FullyUpgradedColumnX = 228; // col 2 — yellow ↑ badge (reserved)

    // Vertical offset of the 24-px icon within each component section.
    private const int CasingY = 227;
    private const int ProjectileY = 517;
    private const int PropellantY = 803;

    public static Rectangle? TryGetRect(EComponentType? componentType, EItemType? itemType)
    {
        if (componentType is not { } ct) return null;
        if (itemType is not (EItemType.Basic or EItemType.Upgrading)) return null;

        int x = itemType == EItemType.Basic ? BasicColumnX : UpgradingColumnX;
        int? y = ct switch
        {
            EComponentType.Casing => CasingY,
            EComponentType.Projectile => ProjectileY,
            EComponentType.Propellant => PropellantY,
            _ => null,
        };
        return y is { } yi ? new Rectangle(x, yi, IconSize, IconSize) : null;
    }

    public static string GetItemTypeName(EItemType itemType) => itemType switch
    {
        EItemType.Basic => "Basic",
        EItemType.Upgrading => "Upgrading",
        EItemType.Utility => "Utility",
        EItemType.Crafting => "Crafting",
        EItemType.Storage => "Storage",
        EItemType.Resource => "Resource",
        _ => null,
    };

    public const string StationCategoryName = "Station";

    public static string GetComponentTypeName(EComponentType componentType) => componentType switch
    {
        EComponentType.Casing => "Casing",
        EComponentType.Projectile => "Projectile",
        EComponentType.Propellant => "Propellant",
        _ => null,
    };
}