using System.Collections.Generic;
using Gamelab.Items;

namespace Gamelab.PhysicalEntities.Configurable
{
    // Generated from Src/Content/Data/StationConfig.json, file order.
    public static class StationConfigTable
    {
        public static StationRegistry Default()
        {
            return new StationRegistry(new[]
            {
                Entry("Cannon", "Cannon", "Load ammo with A, then Interact (X) to fire.", EItemType.Utility, false, 0),
                Entry("Workbench", "Workbench", "Place ingredients with A.\nHold Interact (X) to craft or repair.", EItemType.Utility, false, 38),
                Entry("AutoWorkbench", "Automatic Anvil", "Place ingredients with A.\nHold Interact (X) to craft. Automatically crafts by itself", EItemType.Utility, true, 45),
                Entry("SpeedLever", "Speed Lever", "Interact (X) to cycle train speed.\nHold Interact (X) to repair.", EItemType.Utility, false, 0),
                Entry("Counter", "Counter", "Pickup (A) swaps your item with the counter item.", EItemType.Storage, false, 0),
                Entry("ResourceCoal", "Coal Box", "Pickup (A) grabs coal to fuel the train's engine.", EItemType.Resource, false, 0),
                Entry("BulletRack", "Bullet Rack", "Pickup (A) grabs or stores a Bullet", EItemType.Utility, false, 0),
                Entry("Conveyor", "Conveyor", "Moves items around", EItemType.Utility, true, 20),
                Entry("BulletConveyor", "Bullet Conveyor", "Moves only fully crafted Bullets around", EItemType.Utility, true, 30),
                Entry("UpgradedComponentConveyor", "Component Conveyor", "Moves only upgraded components around", EItemType.Utility, true, 30),
            });
        }

        private static KeyValuePair<string, StationConfig> Entry(string id, string name, string description, EItemType? itemType, bool appearsInShop, int shopPrice)
        {
            return new KeyValuePair<string, StationConfig>(id, new StationConfig
            {
                Name = name,
                Description = description,
                ItemType = itemType,
                AppearsInShop = appearsInShop,
                ShopPrice = shopPrice,
            });
        }
    }
}
