using System.Collections.Generic;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src BulletRack (Cannon/AmmoRack.cs). Dropped presentation: sounds, Draw
    // and the stored-bullet drawing. Src OnGrab returns false (racks cannot be picked up):
    // that is a player/grab seam, nothing to port here.
    public class BulletRackRuntime : StationRuntime
    {
        public const int MaxCapacity = 5;

        private readonly List<Item> storedBullets = new List<Item>();

        public override void Initialize(StationCatalogEntry catalogEntry)
        {
            base.Initialize(catalogEntry);
            if (string.IsNullOrEmpty(StationId)) StationId = StationIds.BulletRack;
        }

        public override Item PeekNextItem() => storedBullets.Count > 0 ? storedBullets[0] : null;

        public override bool CanProvideItem(IItemReceiver consumer) =>
            storedBullets.Count > 0 && IsConsumerFirstInLine(consumer);

        public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
        {
            item = null;
            if (!CanProvideItem(consumer)) return false;
            item = storedBullets[0];
            storedBullets.RemoveAt(0);
            if (consumer != null) consumerQueue.RemoveAll(t => t.Consumer == consumer);
            return true;
        }

        public override bool CanReceiveItem(Item item, IItemProvider source) =>
            storedBullets.Count < MaxCapacity && item is BulletItem { Type: EComponentType.Bullet };

        public override void ReceiveItem(Item item, IItemProvider source) => storedBullets.Add(item);
    }
}
