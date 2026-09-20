using System.Linq;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src ComponentResourceStation. StationId is "Resource" + "Component" + id
    // (ResourceStation prefixes the resource id "Component<id>"), which is what
    // StationIds.IsComponentStationId / GetStationComponentId expect.
    // Dropped presentation: sounds, title/description, CategoryName, FunctionalityName,
    // IconSourceRect, DispensedItemType (shop tooltips) and Draw.
    public class ComponentResourceStationRuntime : ResourceStationRuntime
    {
        public string ComponentId { get; private set; }

        public new void Initialize(StationCatalogEntry catalogEntry, string componentId)
        {
            ComponentId = componentId;
            base.Initialize(catalogEntry, StationIds.GetComponentResourceId(componentId));
        }

        public override Item PeekNextItem() => new BulletItem(ComponentId);

        public override bool CanReceiveItem(Item item, IItemProvider source) =>
            item is BulletItem bullet && bullet.ComponentIds.SequenceEqual(new[] { ComponentId });

        public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
        {
            if (!CanProvideItem(consumer))
            {
                item = null;
                return false;
            }

            item = new BulletItem(ComponentId);
            if (consumer != null) consumerQueue.RemoveAll(t => t.Consumer == consumer);
            return true;
        }
    }
}
