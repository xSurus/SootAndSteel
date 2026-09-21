using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.UI
{
    public static class ShopItemIconAtlas
    {
        public const string SheetFile = "BulletComponentSpriteSheet.png";

        private const int IconSize = 24;

        // Cells are 96 px wide and the 24 px icon is centred, so the crop starts at column origin + 36.
        private const int BasicColumnX = 36; // col 0, plain icon
        private const int UpgradingColumnX = 132; // col 1, grey up-arrow badge

        private const int CasingY = 227;
        private const int ProjectileY = 517;
        private const int PropellantY = 803;

        public static SpriteRect? TryGetRect(EComponentType? componentType, EItemType? itemType)
        {
            if (componentType == null) return null;
            if (itemType != EItemType.Basic && itemType != EItemType.Upgrading) return null;

            int x = itemType == EItemType.Basic ? BasicColumnX : UpgradingColumnX;
            int y;
            switch (componentType.Value)
            {
                case EComponentType.Casing: y = CasingY; break;
                case EComponentType.Projectile: y = ProjectileY; break;
                case EComponentType.Propellant: y = PropellantY; break;
                default: return null;
            }
            return new SpriteRect(x, y, IconSize, IconSize);
        }

        public static string GetItemTypeName(EItemType itemType)
        {
            switch (itemType)
            {
                case EItemType.Basic: return "Basic";
                case EItemType.Upgrading: return "Upgrading";
                case EItemType.Utility: return "Utility";
                case EItemType.Crafting: return "Crafting";
                case EItemType.Storage: return "Storage";
                case EItemType.Resource: return "Resource";
                default: return null;
            }
        }

        public const string StationCategoryName = "Station";

        public static string GetComponentTypeName(EComponentType componentType)
        {
            switch (componentType)
            {
                case EComponentType.Casing: return "Casing";
                case EComponentType.Projectile: return "Projectile";
                case EComponentType.Propellant: return "Propellant";
                default: return null;
            }
        }
    }
}
