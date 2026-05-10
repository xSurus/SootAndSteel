using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class ResourceStation(
    Vector2 position,
    string resourceId)
    : AbstractStation(StationIds.GetResourceStationId(resourceId), position)
{
    protected string ResourceId { get; } = resourceId;
    public override Item PeekNextItem() => new Item(ResourceId);

    public override bool CanReceiveItem(Item item, IItemProvider source)
    {
        return item != null && item.Id == ResourceId;
    }

    public override void ReceiveItem(Item item, IItemProvider source)
    {
        soundService.PlayOnce(ResourceId == "Coal" ? Sounds.ShovelDown : Sounds.DropItem);
    }

    public override bool CanProvideItem(IItemReceiver consumer)
    {
        return IsConsumerFirstInLine(consumer);
    }

    public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
    {
        if (!CanProvideItem(consumer))
        {
            item = null;
            return false;
        }

        soundService.PlayOnce(ResourceId == "Coal" ? Sounds.ShovelUp : Sounds.PickupItem);
        item = new Item(ResourceId);
        if (consumer != null) ConsumerQueue.RemoveAll(t => t.Consumer == consumer);

        return true;
    }
}