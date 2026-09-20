using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src ResourceStation (also covers CoalResourceStation, which only differed by sounds).
    // Sounds are dropped here; the sound wave hooks in later.
    public class ResourceStationRuntime : StationRuntime
    {
        public string ResourceId { get; private set; }

        public void Initialize(StationCatalogEntry catalogEntry, string resourceId)
        {
            ResourceId = resourceId;
            catalogEntry.stationId = StationIds.GetResourceStationId(resourceId);
            Initialize(catalogEntry);
        }

        public override Item PeekNextItem() => new Item(ResourceId);

        public override bool CanReceiveItem(Item item, IItemProvider source) => item != null && item.Id == ResourceId;

        // Receiving does nothing (Src only played a sound).
        public override void ReceiveItem(Item item, IItemProvider source) { }

        public override bool CanProvideItem(IItemReceiver consumer) => IsConsumerFirstInLine(consumer);

        public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
        {
            if (!CanProvideItem(consumer))
            {
                item = null;
                return false;
            }

            item = new Item(ResourceId);
            if (consumer != null) consumerQueue.RemoveAll(t => t.Consumer == consumer);
            return true;
        }
    }
}
