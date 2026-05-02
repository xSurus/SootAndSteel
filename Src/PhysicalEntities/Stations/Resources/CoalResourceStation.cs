using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class CoalResourceStation : ResourceStation
{
    public CoalResourceStation(Vector2 position)
        : base(position, "Coal")
    {
        soundService.LoadSound(Sounds.ShovelUp);
        soundService.LoadSound(Sounds.ShovelDown);
    }

    public override bool CanReceiveItem(Item item, IItemProvider source)
    {
        return item is { Id: "Coal" };
    }

    public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
    {
        if (!CanProvideItem(consumer))
        {
            item = null;
            return false;
        }

        soundService.PlayOnce(Sounds.ShovelUp);
        item = new Item("Coal");
        if (consumer != null) ConsumerQueue.RemoveAll(t => t.Consumer == consumer);

        return true;
    }

    public override void ReceiveItem(Item item, IItemProvider source)
    {
        soundService.PlayOnce(Sounds.ShovelDown);
    }
}